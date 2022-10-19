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
using Terradue.Stars.Data.Model.Atom;
using System.Linq;

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

        [Fact]
        public async Task OverviewAssets()
        {
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/cci_fss_CFD_2002.json")));
            StacItemNode stacItemNode = new StacItemNode(item, new Uri("https://catalog.terradue.com/cci_fss_CFD_2002.json"));
            var assets = _titilerService.SelectOverviewCombinationAssets(item);
            StarsAtomItem starsAtomItem = StarsAtomItem.Create(item, new Uri("https://catalog.terradue.com/cci_fss_CFD_2002.json"));
            // Add EO Profile if possible
            starsAtomItem.TryAddEarthObservationProfile(stacItemNode.StacItem);

            // Add TMS offering via titiler if possible
            var imageOfferingSet = starsAtomItem.TryAddTitilerOffering(stacItemNode, _titilerService);
            Assert.True(starsAtomItem.ElementExtensions.Any(i => i.OuterName == "offering" && i.OuterNamespace == "http://www.opengis.net/owc/1.0"));
        }

        [Fact]
        public async Task SnowHeightTest()
        {
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/snowgrid_meanHS_1953.json")));
            StacItemNode stacItemNode = new StacItemNode(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1953.json"));
            var assets = _titilerService.SelectOverviewCombinationAssets(item);
            StarsAtomItem starsAtomItem = StarsAtomItem.Create(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1953.json"));
            // Add EO Profile if possible
            Assert.True(starsAtomItem.TryAddEarthObservationProfile(stacItemNode.StacItem));

            // Add TMS offering via titiler if possible
            var imageOfferingSet = starsAtomItem.TryAddTitilerOffering(stacItemNode, _titilerService);
            Assert.True(starsAtomItem.ElementExtensions.Any(i => i.OuterName == "offering" && i.OuterNamespace == "http://www.opengis.net/owc/1.0"));
        }
    }
}
