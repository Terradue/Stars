using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Stac.Extensions.Projection;
using Terradue.Stars.Data.Translators;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Model.Atom;

namespace Terradue.Data.Test.Harvesters
{
    public class StacItemToAtomItemTests : TestBase
    {
        [Fact]
        public async System.Threading.Tasks.Task S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(null, ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new System.Uri("s3://eoepca-ades/wf-d0534740-b97b-11eb-82cf-0a580a830350/S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808/S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.Translate<AtomItemNode>(stacItemNode);

            Assert.Equal("S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808", atomItemNode.AtomItem.Identifier);

        }

        [Fact]
        public async System.Threading.Tasks.Task LegendTest()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(null, ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new System.Uri("s3://eoepca-ades/wf-test/test.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.Translate<AtomItemNode>(stacItemNode);

            ServiceModel.Syndication.SyndicationLink legendLink = atomItemNode.AtomItem.Links.FirstOrDefault(r => r.RelationshipType == "legend");

            Assert.NotNull(legendLink);
            Assert.Equal("https://test.com/legend.png", legendLink.Uri.AbsoluteUri);
        }

    }

}
