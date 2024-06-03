// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacCollectionToAtomItemTests.cs

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Stac;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using Terradue.Stars.Data.Translators;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Supplier;
using Xunit;

namespace Terradue.Data.Tests.Translators
{
    public class StacCollectionToAtomItemTests : TestBase
    {
        [Fact]
        public async System.Threading.Tasks.Task EGMS()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacCollection stacCollection = StacConvert.Deserialize<StacCollection>(json);

            StacCollectionToAtomItemTranslator stacCollectionToAtomItemTranslator = new StacCollectionToAtomItemTranslator(ServiceProvider);

            StacCollectionNode stacItemNode = new StacCollectionNode(stacCollection, new Uri("https://localhost:5001/api/ns/emathot/cs/0000024-220301000006171-oozie-oozi-W"));

            AtomItemNode atomItemNode = await stacCollectionToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            var fs = File.OpenWrite("egms.atom");
            atomItemNode.GetStreamAsync(CancellationToken.None).Result.CopyTo(fs);
            fs.Close();

            bool egmsIsPresent = false;
            bool wmsIsPresent = false;
            if (atomItemNode.AtomItem.ElementExtensions != null && atomItemNode.AtomItem.ElementExtensions.Count > 0)
            {
                var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));

                foreach (var offering in offerings)
                {
                    if (offering != null && offering.Code == "http://www.terradue.com/egms") egmsIsPresent = true;
                    if (offering != null && offering.Code == "http://www.opengis.net/spec/owc-atom/1.0/req/wms") wmsIsPresent = true;
                }

            }

            Assert.True(egmsIsPresent);
            Assert.True(wmsIsPresent);
        }

        [Fact]
        public async System.Threading.Tasks.Task EGMS_2018_2022()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacCollection stacCollection = StacConvert.Deserialize<StacCollection>(json);

            StacCollectionToAtomItemTranslator stacCollectionToAtomItemTranslator = new StacCollectionToAtomItemTranslator(ServiceProvider);

            StacCollectionNode stacCollectionNode = new StacCollectionNode(stacCollection, new Uri("https://api.terradue.com/timeseries/v1/ns/gep-egms/cs/EGMS-2018-2022"));

            // Filter assets to remove the timeseries assets
            AssetFilters assetFilters = AssetFilters.CreateAssetFilters(
                new string[] { "{type}!application/csv" }
            );
            // Create a filtered asset container
            FilteredAssetContainer filteredAssetContainer = new FilteredAssetContainer(stacCollectionNode, assetFilters);
            // Create a container node with the filtered asset container
            CollectionContainerNode filteredNode = new CollectionContainerNode(stacCollectionNode, filteredAssetContainer.Assets, "filtered");
            StacCollection stacCollection1 = new StacCollection(stacCollection);
            stacCollection1.Assets.Clear();
            stacCollection1.Assets.AddRange(filteredAssetContainer.Assets.ToDictionary(asset => asset.Key, asset => (asset.Value as StacAssetAsset).StacAsset));
            StacCollectionNode stacCollectionNode1 = new StacCollectionNode(stacCollection1, new Uri("https://api.terradue.com/timeseries/v1/ns/gep-egms/cs/EGMS-2018-2022"));

            AtomItemNode atomItemNode = await stacCollectionToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacCollectionNode1, CancellationToken.None);

            bool egmsIsPresent = false;
            if (atomItemNode.AtomItem.ElementExtensions != null && atomItemNode.AtomItem.ElementExtensions.Count > 0)
            {
                var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));

                foreach (var offering in offerings)
                {
                    if (offering != null && offering.Code == "http://www.terradue.com/egms") egmsIsPresent = true;
                }
            }

            Assert.True(egmsIsPresent);

            // Check that there is no link with the relationship type "enclosure"
            Assert.DoesNotContain(atomItemNode.AtomItem.Links,
                                  l => l.MediaType == "application/csv" && l.RelationshipType == "enclosure");
        }

    }

}
