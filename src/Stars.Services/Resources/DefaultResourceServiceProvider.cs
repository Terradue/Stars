using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
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

        public async Task<IStreamResource> CreateStreamResourceAsync(IResource resource)
        {

            if (resource is IStreamResource)
            {
                return resource as IStreamResource;
            }

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
            catch
            {
            }

            // S3
            if (s3Url != null && (resource.Uri.Scheme == "s3" || s3Url.Endpoint.Contains("s3.")))
            {
                IS3ClientFactory s3ClientFactory = _serviceProvider.GetService<IS3ClientFactory>();
                try
                {
                    if (resource is IAsset)
                        return await s3ClientFactory.CreateAndLoadAsync(resource as IAsset);
                    return await s3ClientFactory.CreateAndLoadAsync(s3Url);
                }
                catch (AmazonS3Exception e)
                {
                    logger.LogError(e, "Error loading S3 resource {0} : {1}", resource.Uri, e.Message);
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
                triedS3 = true;
            }

            // HTTP
            if (resource.Uri.Scheme.StartsWith("http"))
            {
                var clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
                if (clientFactory == null)
                    throw new SystemException("HttpClient Factory not provided");

                var client = clientFactory.CreateClient("stars");

                HttpResponseMessage response = await client.GetAsync(resource.Uri);

                // S3 resource case
                if (s3Url != null && !triedS3 && response.Headers.Any(h => h.Key.StartsWith("x-amz", true, System.Globalization.CultureInfo.InvariantCulture)))
                {
                    try
                    {
                        IS3ClientFactory s3ClientFactory = _serviceProvider.GetService<IS3ClientFactory>();
                        if (resource is IAsset)
                            return await s3ClientFactory.CreateAndLoadAsync(resource as IAsset);
                        return await s3ClientFactory.CreateAndLoadAsync(s3Url);
                    }
                    catch (AmazonS3Exception e)
                    {
                        logger.LogError(e, "Error loading S3 resource {0} : {1}", resource.Uri, e.Message);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                    }
                    triedS3 = true;
                }

                return new HttpResource(resource.Uri, client, response.Content.Headers);
            }

            // Unknown
            throw new SystemException("Unknown resource type");
        }

        public async Task<Stream> GetAssetStreamAsync(IAsset asset)
        {
            return await (await CreateStreamResourceAsync(asset)).GetStreamAsync();
        }

        public Task<IAssetsContainer> GetAssetsInFolder(IResource resource)
        {
            return Task.FromResult<IAssetsContainer>(new LocalDirectoryResource(_serviceProvider.GetService<IFileSystem>(), resource.Uri.AbsolutePath));
        }

        public async Task<IStreamResource> GetStreamResourceAsync(IResource resource)
        {
            if (resource is IStreamResource)
            {
                return (IStreamResource)resource;
            }
            IStreamResource sresource = await CreateStreamResourceAsync(resource);
            if (resource.ContentType == null || resource.ContentType.MediaType == null || resource.ContentType.MediaType.EndsWith("octet-stream") || sresource.ContentType.MediaType.EndsWith("octet-stream"))
                return sresource;
            if (sresource.ContentType.MediaType != resource.ContentType.MediaType)
            {
                throw new Exception($"Requested Stream type '{sresource.ContentType}' is different from the reference type '{resource.ContentType}'");
            }
            return sresource;
        }

        public async Task Delete(IResource resource)
        {
            IStreamResource streamResource = await GetStreamResourceAsync(resource);
            if (streamResource is IDeletableResource)
            {
                await ((IDeletableResource)streamResource).Delete();
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