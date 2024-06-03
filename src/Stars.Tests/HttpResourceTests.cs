using System;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Resources;
using Xunit;

namespace Stars.Tests
{
    [Collection(nameof(StarsCollection))]
    public class HttpResourceTests
    {
        private readonly ITestLoggerFactory loggerFactory;
        private readonly ILogger<StacTests> logger;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public HttpResourceTests(IResourceServiceProvider resourceServiceProvider)
        {
            this.resourceServiceProvider = resourceServiceProvider;
            loggerFactory = TestLoggerFactory.Create();
            logger = loggerFactory.CreateLogger<StacTests>();
        }

        [Fact(Skip = "External resource")]
        public async Task LargeHttpResourceTest()
        {
            GenericResource genericResource = new GenericResource(new Uri("https://download.disasterscharter.org/cos-api/service/acquisition/916/CSA/RCM-1/urn_ogc_def_EOP_CSA_RCM1_OK2417781_PK2421038_1_FSL25_20230119_105132_HH_HV_GRD/urn_ogc_def_EOP_CSA_RCM1_OK2417781_PK2421038_1_FSL25_20230119_105132_HH_HV_GRD-product.zip"));
            HttpResource httpResource = await resourceServiceProvider.CreateStreamResourceAsync(genericResource, System.Threading.CancellationToken.None) as HttpResource;
            var streamTask = httpResource.GetStreamAsync(System.Threading.CancellationToken.None);
            var stream = await streamTask.WaitAsync(TimeSpan.FromSeconds(10));
        }
    }
}
