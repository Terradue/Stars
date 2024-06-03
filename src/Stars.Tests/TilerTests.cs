using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MELT;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using Terradue.Stars.Data.Model.Atom;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.ThirdParty.Titiler;
using Xunit;

namespace Stars.Tests
{
    [Collection(nameof(StarsCollection))]
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

        [Fact]
        public async Task SnowHeightTest2()
        {
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/snowgrid_meanHS_1973.json")));
            StacItemNode stacItemNode = new StacItemNode(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1973.json"));
            var assets = _titilerService.SelectOverviewCombinationAssets(item);
            StarsAtomItem starsAtomItem = StarsAtomItem.Create(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1973.json"));
            // Add EO Profile if possible
            Assert.True(starsAtomItem.TryAddEarthObservationProfile(stacItemNode.StacItem));

            var date = starsAtomItem.ElementExtensions.First(i => i.OuterName == "date" && i.OuterNamespace == "http://purl.org/dc/elements/1.1/").GetObject<string>();

            Assert.Equal(DateTime.Parse("1972-12-01T00:00:00Z"), DateTime.Parse(date.Split("/")[0]));

            // Add TMS offering via titiler if possible
            var imageOfferingSet = starsAtomItem.TryAddTitilerOffering(stacItemNode, _titilerService);
            Assert.True(starsAtomItem.ElementExtensions.Any(i => i.OuterName == "offering" && i.OuterNamespace == "http://www.opengis.net/owc/1.0"));
        }

        [Fact]
        public async Task MultiTilerTest()
        {
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/snowgrid_meanHS_1974.json")));
            StacItemNode stacItemNode = new StacItemNode(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1974.json"));
            var assets = _titilerService.SelectOverviewCombinationAssets(item);
            StarsAtomItem starsAtomItem = StarsAtomItem.Create(item, new Uri("https://catalog.terradue.com/snowgrid_meanHS_1974.json"));
            // Add EO Profile if possible
            Assert.True(starsAtomItem.TryAddEarthObservationProfile(stacItemNode.StacItem));

            var date = starsAtomItem.ElementExtensions.First(i => i.OuterName == "date" && i.OuterNamespace == "http://purl.org/dc/elements/1.1/").GetObject<string>();

            Assert.Equal(DateTime.Parse("1972-12-01T00:00:00Z"), DateTime.Parse(date.Split("/")[0]));

            // Add TMS offering via titiler if possible
            var imageOfferingSet = starsAtomItem.TryAddTitilerOffering(stacItemNode, _titilerService);
            var offerings = starsAtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            Assert.Equal("http://www.terradue.com/twm", offerings.First().Code);
            var op = offerings.First().Operations.First();
            Assert.Equal("GetMap", op.Code);
            Assert.Equal("http://tiler.test2.terradue.com/stac/tiles/WebMercatorQuad/{z}/{x}/{y}.png?url=https://catalog.terradue.com/snowgrid_meanHS_1974.json&assets=snowgrid_meanHS_1974&rescale=0,255&color_formula=&resampling_method=average", op.Href);
        }
    }
}
