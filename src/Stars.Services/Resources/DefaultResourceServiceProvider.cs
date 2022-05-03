using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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

        public async Task<IStreamResource> CreateAsync(Uri url)
        {
            // Local file
            if ( url.IsFile )
            {
                return new LocalFileSystemResource(url.AbsolutePath, ResourceType.Unknown);
            }

            // HTTP
            var client = _serviceProvider.GetService<HttpClient>();
            if ( client == null )
                throw new SystemException("HttpClient not provided");

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
            return await (await CreateAsync(asset.Uri)).GetStreamAsync();
        }

        public async Task<IStreamResource> GetStreamResourceAsync(IResource resource)
        {
            return await CreateAsync(resource.Uri);
        }

    }
}