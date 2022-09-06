using System;
using System.IO;
using System.Threading.Tasks;
using Stac;
using Stac.Exceptions;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;
using Microsoft.Extensions.Logging;
using MELT;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.ThirdParty.Titiler;

namespace Stars.Tests
{
    public class TilerTests
    {
        private readonly ITestLoggerFactory loggerFactory;
        private readonly ILogger<StacTests> logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly TitilerService _titilerService;

        public TilerTests(IResourceServiceProvider resourceServiceProvider, TitilerService titilerService)
        {
            this.resourceServiceProvider = resourceServiceProvider;
            _titilerService = titilerService;
            loggerFactory = TestLoggerFactory.Create();
            logger = loggerFactory.CreateLogger<StacTests>();
        }

        [Fact]
        public async Task MimetypeTest()
        {
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/cci_fss_CFD_1993.json")));
            var assets = _titilerService.SelectOverviewCombinationAssets(item);
            Assert.True(assets.ContainsKey("cci_fss_CFD_1993"));
        }
    }
}
