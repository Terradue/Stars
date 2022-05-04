using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Resources
{
    public class DefaultResourceServiceProvider : IResourceServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IStreamResource> CreateStreamResourceAsync(Uri url)
        {
            // Local file
            if ( url.IsFile )
            {
                return new LocalFileResource(_serviceProvider.GetService<IFileSystem>(), url.AbsolutePath, ResourceType.Unknown);
            }

            // HTTP
            var clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            if ( clientFactory == null )
                throw new SystemException("HttpClient Factory not provided");

            var client = clientFactory.CreateClient("stars");

            HttpResponseMessage response = await client.GetAsync(url);

            // S3 resource case
            if (response.Headers.Any(h => h.Key.StartsWith("x-amz")))
            {
                S3Url s3Url = S3Url.ParseUri(url);
                IOptions<S3Options> s3Options = _serviceProvider.GetService<IOptions<S3Options>>();
                return await S3Resource.CreateAsync(s3Url, s3Options.Value, null);
            }

            return new HttpResource(url, client, response.Content.Headers);
        }

        public async Task<Stream> GetAssetStreamAsync(IAsset asset)
        {
            return await (await CreateStreamResourceAsync(asset.Uri)).GetStreamAsync();
        }

        public Task<IAssetsContainer> GetAssetsInFolder(Uri uri)
        {
            return Task.FromResult(new LocalDirectoryResource(_serviceProvider.GetService<IFileSystem>(), uri.AbsolutePath));        }

        public async Task<IStreamResource> CreateStreamResourceAsync(IResource resource)
        {
            if ( resource is IStreamResource )
            {
                return (IStreamResource)resource;
            }
            return await CreateStreamResourceAsync(resource.Uri);
        }
    }
}