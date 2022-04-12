using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface;

namespace Stars.Services.Router
{
    public class DefaultResourceServiceProvider : IResourceServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IResource> CreateAsync(Uri url)
        {
            var client = _serviceProvider.GetService<HttpClient>();

            HttpResponseMessage response = await client.GetAsync(url);

            // S3 resource case
            if (response.Headers.Any(h => h.Key.StartsWith("x-amz")))
            {
                S3Url s3Url = S3Url.ParseUri(url);
                IOptionsMonitor<S3Options> s3OptionsMonitor = _serviceProvider.GetService<IOptionsMonitor<S3Options>>();
                return new S3Route(s3Url, s3OptionsMonitor, identityProvider);
            }

            return new HttpRoute(url, client, response.Content.Headers);
        }
    }
}