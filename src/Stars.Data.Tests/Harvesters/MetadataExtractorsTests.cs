// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: MetadataExtractorsTests.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Data.Model.Metadata;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;
using Xunit.Abstractions;

namespace Terradue.Data.Tests.Harvesters
{
    public class MetadataExtractorsTests : TestBase
    {
        private readonly IFileSystem fileSystem;

        public MetadataExtractorsTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            fileSystem = ServiceProvider.GetService<IFileSystem>();
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
            StacRouter stacRouter = ServiceProvider.GetService<StacRouter>();
            TestItem testNode = new TestItem(datadir);
            IResource route = new ItemContainerNode(testNode, new Dictionary<string, IAsset>(testNode.Assets), "");
            IDestination destination = LocalFileDestination.Create(fileSystem.Directory.CreateDirectory("out/"), route);
            destination.PrepareDestination();

            Assert.True(extractor.CanProcess(route, destination));
            IResource stacResource = await extractor.ProcessAsync(route, destination, CancellationToken.None);
            Assert.NotNull(stacResource);
            List<StacItemNode> stacItemNodes = new List<StacItemNode>();
            if (stacResource is StacItemNode)
                stacItemNodes.Add(await stacRouter.RouteAsync(stacResource, CancellationToken.None) as StacItemNode);
            if (stacResource is StacCatalogNode)
                stacItemNodes.AddRange((stacResource as StacCatalogNode)
                                        .GetRoutes(stacRouter)
                                        .Where(r => r.ResourceType == ResourceType.Item)
                                        .Select(r => r as StacItemNode)
                                      );
            foreach (var stacItemNode in stacItemNodes)
            {
                Assert.NotNull(stacItemNode);
                StacItem stacItem = stacItemNode.StacObject as StacItem;
                Assert.NotNull(stacItem);
                Assert.NotNull(stacItem.Providers);
                stacItem.Properties.Remove("updated");
                stacItemNode.MakeAssetUriRelative();
                RemoveAssetUriTmp(stacItemNode);
                CheckAssetLocalPath(stacItem, key);
                var actualJson = StacConvert.Serialize(stacItem, new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
                try
                {
                    stacValidator.ValidateJson(actualJson);
                }
                catch (Exception)
                {
                    System.Console.WriteLine(actualJson);
                    throw;
                }
                // Dot NOT uncomment unless you are changing the expected JSON
                // WriteJson(Path.Join(datadir, "../.."), actualJson, stacItem.Id);
                string expectedJson = null;
                try
                {
                    expectedJson = GetJson(Path.Join(datadir, "../.."), stacItem.Id);
                }
                catch (Exception)
                {
                    System.Console.WriteLine(actualJson);
                    throw;
                }
                // stacValidator.ValidateJson(expectedJson);

                try
                {
                    JsonAssert.AreEqual(expectedJson, actualJson);
                }
                catch (Exception)
                {
                    System.Console.WriteLine(actualJson);
                    throw;
                }
            }
        }

        private void CheckAssetLocalPath(StacItem stacItem, string key)
        {
            foreach (var asset in stacItem.Assets)
            {
                Assert.True(asset.Value.Properties.ContainsKey("filename"), $"[{key}] Asset " + asset.Key + " does not contain filename property");
            }
        }

        private static void RemoveAssetUriTmp(StacItemNode stacItemNode)
        {
            foreach (var asset in stacItemNode.StacItem.Assets)
            {
                var newurl = Regex.Replace(asset.Value.Uri.ToString(), ".*/tmp/[^/]*/(.*)", "$1");
                asset.Value.Uri = new Uri(newurl, UriKind.Relative);
            }
        }
    }
}
