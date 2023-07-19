using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Resources
{
    public class DefaultResourceServiceProvider : IResourceServiceProvider
    {
        private readonly ILogger<DefaultResourceServiceProvider> logger;
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceServiceProvider(ILogger<DefaultResourceServiceProvider> logger,
                                              IServiceProvider serviceProvider)
        {
            this.logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<IStreamResource> CreateStreamResourceAsync(IResource resource, CancellationToken ct)
        {

            Exception finalException = null;

            // Local file
            if (resource.Uri.IsFile)
            {
                return new LocalFileResource(_serviceProvider.GetRequiredService<IFileSystem>(), resource.Uri.AbsolutePath, ResourceType.Unknown);
            }

            S3Url s3Url = null;
            bool triedS3 = false;
            // Try to parse S3 URL
            try
            {
                s3Url = S3Url.ParseUri(resource.Uri);
            }
            catch (Exception e)
            {
                finalException = e;
            }

            // S3
            if (s3Url != null && (resource.Uri.Scheme == "s3" || s3Url.Endpoint.Contains("s3.")))
            {
                IS3ClientFactory s3ClientFactory = _serviceProvider.GetService<IS3ClientFactory>();
                try
                {
                    if (resource is IAsset)
                        return await s3ClientFactory.CreateAndLoadAsync(resource as IAsset, ct);
                    return await s3ClientFactory.CreateAndLoadAsync(s3Url, ct);
                }
                catch (AmazonS3Exception e)
                {
                    logger.LogError(e, "Error loading S3 resource {0} : {1}", resource.Uri, e.Message);
                    finalException = e;
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                    finalException = e;
                }
                triedS3 = true;
            }

            bool badStatusCode = false;

            // HTTP
            if (resource.Uri.Scheme.StartsWith("http"))
            {
                var clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
                if (clientFactory == null)
                    throw new SystemException("HttpClient Factory not provided");

                var client = clientFactory.CreateClient("stars");

                HttpCachedHeaders contentHeaders = null;

                try
                {
                    // First try head request
                    using (var headResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, resource.Uri), ct))
                    {
                        contentHeaders = new HttpCachedHeaders(headResponse);
                        headResponse.EnsureSuccessStatusCode();
                    }
                }
                catch (Exception e)
                {
                    contentHeaders = null;
                    finalException = e;
                }
                if (contentHeaders == null)
                {
                    try
                    {
                        using (var response = await client.GetAsync(resource.Uri, HttpCompletionOption.ResponseHeadersRead, ct))
                        {
                            contentHeaders = new HttpCachedHeaders(response);
                            contentHeaders.AddRange(response.Content.Headers);
                            try
                            {
                                response.EnsureSuccessStatusCode();
                            }
                            catch (Exception e)
                            {
                                badStatusCode = true;
                                throw;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        finalException = e;
                    }
                }

                // S3 resource case
                if (s3Url != null
                    && !triedS3
                    && badStatusCode
                    && contentHeaders.Any(h => h.Key.StartsWith("x-amz", true, System.Globalization.CultureInfo.InvariantCulture)))
                {
                    try
                    {
                        IS3ClientFactory s3ClientFactory = _serviceProvider.GetService<IS3ClientFactory>();
                        if (resource is IAsset)
                            return await s3ClientFactory.CreateAndLoadAsync(resource as IAsset, ct);
                        return await s3ClientFactory.CreateAndLoadAsync(s3Url, ct);
                    }
                    catch (AmazonS3Exception e)
                    {
                        logger.LogError(e, "Error loading S3 resource {0} : {1}", resource.Uri, e.Message);
                        finalException = e;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                        finalException = e;
                    }
                    triedS3 = true;
                }

                if (!badStatusCode)
                {
                    return new HttpResource(resource.Uri, client, contentHeaders);
                }
            }

            // Unknown
            if (finalException is AmazonS3Exception amazonS3Exception)
            {
                logger.LogDebug(amazonS3Exception.ResponseBody);
                throw new SystemException($"Error loading resource {resource.Uri} : {amazonS3Exception.Message}", amazonS3Exception);
            }
            throw finalException;
        }

        public async Task<Stream> GetAssetStreamAsync(IAsset asset, CancellationToken ct)
        {
            return await (await CreateStreamResourceAsync(asset, ct)).GetStreamAsync(ct);
        }

        public Task<IAssetsContainer> GetAssetsInFolderAsync(IResource resource, CancellationToken ct)
        {
            return Task.FromResult<IAssetsContainer>(new LocalDirectoryResource(_serviceProvider.GetService<IFileSystem>(), resource.Uri.AbsolutePath));
        }

        public async Task<IStreamResource> GetStreamResourceAsync(IResource resource, CancellationToken ct)
        {
            if (resource is IStreamResource)
            {
                return (IStreamResource)resource;
            }
            IStreamResource sresource = await CreateStreamResourceAsync(resource, ct);
            if (resource.ContentType == null || resource.ContentType.MediaType == null || resource.ContentType.MediaType.EndsWith("octet-stream") || sresource.ContentType.MediaType.EndsWith("octet-stream"))
                return sresource;
            if (sresource.ContentType.MediaType != resource.ContentType.MediaType)
            {
                logger.LogWarning($"Requested Stream type '{sresource.ContentType}' is different from the reference type '{resource.ContentType}'");
            }
            return sresource;
        }

        public async Task DeleteAsync(IResource resource, CancellationToken ct)
        {
            IStreamResource streamResource = await CreateStreamResourceAsync(resource, ct);
            if (streamResource is IDeletableResource)
            {
                await ((IDeletableResource)streamResource).DeleteAsync(ct);
                return;
            }
            throw new SystemException("Resource cannot be deleted");
        }

        public Uri ComposeLinkUri(IResourceLink childLink, IResource resource)
        {
            Uri linkUri = childLink.Uri;
            // Simple case: link is relative to the resource
            if (!childLink.Uri.IsAbsoluteUri && resource.Uri.IsAbsoluteUri)
            {
                linkUri = new Uri(resource.Uri, childLink.Uri);
                return linkUri;
            }
            // Complex case: mixing S3
            if (childLink.Uri.IsAbsoluteUri && resource.Uri.IsAbsoluteUri && childLink.Uri.Scheme == "s3")
            {
                S3Url s3Url = S3Url.ParseUri(childLink.Uri);
                S3Url resourceS3Url = S3Url.ParseUri(resource.Uri);
                if (s3Url.Bucket == resourceS3Url.Bucket)
                {
                    linkUri = new Uri(resource.Uri, "/" + s3Url.Key);
                    return linkUri;
                }
            }
            return linkUri;
        }
    }
}