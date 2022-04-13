using Terradue.Stars.Services;
using Terradue.Stars.Services.Supplier;
using Xunit;
using Xunit.Abstractions;
using Terradue.Stars.Services.Model.Stac;
using Stac;
using Terradue.Stars.Data.Model.Metadata;
using System.IO;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Terradue.Stars.Services.Store;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Terradue.Data.Test.Harvesters
{
    public class MetadataExtractorsTests : TestBase
    {
        private readonly IFileSystem fileSystem;

        public MetadataExtractorsTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            this.fileSystem = ServiceProvider.GetService<IFileSystem>();
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                return new MetadataExtractorsData();
            }
        }

        [Theory, MemberData("TestData", DisableDiscoveryEnumeration = true)]
        public async void TestExtractors(string key, string datadir, MetadataExtraction extractor)
        {
            StacRouter stacRouter = new StacRouter(null);
            TestItem testNode = new TestItem(datadir);
            IResource route = new ContainerNode(testNode, new Dictionary<string, IAsset>(testNode.Assets), "");
            IDestination destination = LocalFileDestination.Create(fileSystem.Directory.CreateDirectory("out/"), route);
            destination.PrepareDestination();

            Assert.True(extractor.CanProcess(route, destination));
            IResource stacResource = await extractor.Process(route, destination);
            Assert.NotNull(stacResource);
            List<StacItemNode> stacItemNodes = new List<StacItemNode>();
            if (stacResource is StacItemNode)
                stacItemNodes.Add(await stacRouter.Route(stacResource) as StacItemNode);
            if (stacResource is StacCatalogNode)
                stacItemNodes.AddRange((stacResource as StacCatalogNode)
                                        .GetRoutes(null)
                                        .Where(r => r.ResourceType == ResourceType.Item)
                                        .Select(r => r as StacItemNode)
                                      );
            foreach (var stacItemNode in stacItemNodes)
            {
                Assert.NotNull(stacItemNode);
                StacItem stacItem = stacItemNode.StacObject as StacItem;
                Assert.NotNull(stacItem);
                stacItem.Properties.Remove("updated");
                stacItemNode.MakeAssetUriRelative();
                CheckAssetLocalPath(stacItem, key);
                var actualJson = StacConvert.Serialize(stacItem, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
                stacValidator.ValidateJson(actualJson);
                // Dot NOT uncomment unless you are changing the expected JSON
                // WriteJson(Path.Join(datadir, "../.."), actualJson, stacItem.Id);
                var expectedJson = GetJson(Path.Join(datadir, "../.."), stacItem.Id);
                // stacValidator.ValidateJson(expectedJson);
                JsonAssert.AreEqual(expectedJson, actualJson);
            }
        }

        private void CheckAssetLocalPath(StacItem stacItem, string key)
        {
            foreach (var asset in stacItem.Assets)
            {
                Assert.True(asset.Value.Properties.ContainsKey("filename"), $"[{key}] Asset " + asset.Key + " does not contain filename property");
            }
        }
    }
}
