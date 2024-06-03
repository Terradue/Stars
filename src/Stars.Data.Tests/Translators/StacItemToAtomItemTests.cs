using System;
using System.Linq;
using System.Threading;
using System.Xml;
using Stac;
using Terradue.Metadata.EarthObservation.Ogc.Extensions;
using Terradue.Metadata.EarthObservation.OpenSearch.Extensions;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using Terradue.Stars.Data.Translators;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Xunit;

namespace Terradue.Data.Tests.Translators
{
    public class StacItemToAtomItemTests : TestBase
    {
        [Fact]
        public async System.Threading.Tasks.Task S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://eoepca-ades/wf-d0534740-b97b-11eb-82cf-0a580a830350/S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808/S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            Assert.Equal("S2A_MSIL2A_20191216T004701_N0213_R102_T53HPA_20191216T024808", atomItemNode.AtomItem.Identifier);

            // Test if the cloud cover is set
            var eop = atomItemNode.AtomItem.GetEarthObservationProfile();
            Assert.NotNull(eop);
            var cloudCover = eop.result.Opt21EarthObservationResult.cloudCoverPercentage;
            Assert.NotNull(cloudCover);
            Assert.Equal(26.4, cloudCover.Value);

        }

        [Fact]
        public async System.Threading.Tasks.Task S1A_OPER_SAR_EOSSP__CORE_L1A_OLF_20211117T174240()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://dc-development-catalog/calls/call-969/calibratedDatasets/S1A_OPER_SAR_EOSSP__CORE_L1A_OLF_20211117T174240-calibrated/S1A_OPER_SAR_EOSSP__CORE_L1A_OLF_20211117T174240-calibrated.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            Assert.Equal("S1A_OPER_SAR_EOSSP__CORE_L1A_OLF_20211117T174240", atomItemNode.AtomItem.Identifier);

            // Get the vector offering
            var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            var twmOfferings = offerings.Where(r => r.Code == "http://www.terradue.com/twm");

            Assert.NotNull(twmOfferings);
            Assert.Equal(1, twmOfferings.Count());
            Assert.Equal("GetMap", twmOfferings.ElementAt(0).Operations.First().Code);
            Assert.Equal("image/png", twmOfferings.ElementAt(0).Operations.First().Type);

        }

        [Fact]
        public async System.Threading.Tasks.Task VectorResults()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://eoepca-ades/wf-test/test.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            // Get the vector offering
            var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            var vectorOfferings = offerings.Where(r => r.Code == "http://www.terradue.com/fgb");

            Assert.NotNull(vectorOfferings);
            Assert.Equal(2, vectorOfferings.Count());
            Assert.Equal("GetMap", vectorOfferings.ElementAt(0).Operations.First().Code);
            Assert.Equal("application/flatgeobuf", vectorOfferings.ElementAt(0).Operations.First().Type);
            Assert.Equal("GetMap", vectorOfferings.ElementAt(1).Operations.First().Code);
            Assert.Equal("application/geo+json", vectorOfferings.ElementAt(1).Operations.First().Type);
        }

        [Fact]
        public async System.Threading.Tasks.Task LegendTest()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://eoepca-ades/wf-test/test.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            ServiceModel.Syndication.SyndicationLink legendLink = atomItemNode.AtomItem.Links.FirstOrDefault(r => r.RelationshipType == "legend");

            Assert.NotNull(legendLink);
            Assert.Equal("https://test.com/legend.png", legendLink.Uri.AbsoluteUri);
        }

        [Fact]
        public async System.Threading.Tasks.Task TWMTest()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://eoepca-ades/wf-test/test.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            // Get the vector offering
            var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            var twmOfferings = offerings.Where(r => r.Code == "http://www.terradue.com/twm");

            Assert.NotNull(twmOfferings);
            Assert.Equal(1, twmOfferings.Count());
            Assert.Equal("GetMap", twmOfferings.ElementAt(0).Operations.First().Code);
            Assert.Equal("image/png", twmOfferings.ElementAt(0).Operations.First().Type);
        }

        [Fact]
        public async System.Threading.Tasks.Task HANDTest()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://dc-acceptance-catalog/calls/call-969/calibratedDatasets/act-813_Auxiliary_Dataset_HAND-calibrated/act-813_Auxiliary_Dataset_HAND-calibrated.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            // Get the vector offering
            var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            var twmOfferings = offerings.Where(r => r.Code == "http://www.terradue.com/twm");

            Assert.NotNull(twmOfferings);
            Assert.Equal(1, twmOfferings.Count());
            Assert.Equal("GetMap", twmOfferings.ElementAt(0).Operations.First().Code);
            Assert.Equal("image/png", twmOfferings.ElementAt(0).Operations.First().Type);
        }

        [Fact]
        public async System.Threading.Tasks.Task NEWSAT20240320041309SN24L1SRTest()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://cpe-operations-catalog/calls/call-997/datasets/20240320_041309_SN24_L1_SR/20240320_041309_SN24_L1_SR.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

        }

        [Fact]
        public async System.Threading.Tasks.Task S2A_MSIL2A_20221221T100431_N0509_R122_T33TWM_20221221T141957()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://cpe-operations-catalog/calls/call-6/calibratedDatasets/S2A_MSIL2A_20221221T100431_N0509_R122_T33TWM_20221221T141957-calibrated/S2A_MSIL2A_20221221T100431_N0509_R122_T33TWM_20221221T141957/S2A_MSIL2A_20221221T100431_N0509_R122_T33TWM_20221221T141957.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            // Test if the cloud cover is set
            var eop = atomItemNode.AtomItem.GetEarthObservationProfile();
            Assert.NotNull(eop);
            var cloudCover = eop.result.Opt21EarthObservationResult.cloudCoverPercentage;
            Assert.NotNull(cloudCover);
            Assert.Equal(99.146658, cloudCover.Value);

            // Test that the sensor type is set
            var sensorType = eop.procedure.Eop21EarthObservationEquipment.sensor.Sensor.sensorType;
            Assert.NotNull(sensorType);
            Assert.Contains("OPTICAL", sensorType);

            // Test that the platform is set
            var platform = eop.procedure.Eop21EarthObservationEquipment.platform.Platform.shortName;
            Assert.NotNull(platform);
            Assert.Contains("sentinel-2a", platform);

            // Test that the instrument is set
            var instrument = eop.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName;
            Assert.NotNull(instrument);
            Assert.Contains("msi", instrument);

        }

        [Fact]
        public async System.Threading.Tasks.Task S1A_IW_GRDH_1SDV_20220903T165054_20220903T165119_044843_055B06_B33A()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("s3://cpe-operations-catalog/calls/call-6/calibratedDatasets/S1A_IW_GRDH_1SDV_20220903T165054_20220903T165119_044843_055B06_B33A-calibrated/S1A_IW_GRDH_1SDV_20220903T165054_20220903T165119_044843_055B06_B33A/S1A_IW_GRDH_1SDV_20220903T165054_20220903T165119_044843_055B06_B33A.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            var eop = atomItemNode.AtomItem.GetEarthObservationProfile();
            Assert.NotNull(eop);

            // Test that the sensor type is set
            var sensorType = eop.procedure.Eop21EarthObservationEquipment.sensor.Sensor.sensorType;
            Assert.NotNull(sensorType);
            Assert.Contains("RADAR", sensorType);

            // Test that the platform is set
            var platform = eop.procedure.Eop21EarthObservationEquipment.platform.Platform.shortName;
            Assert.NotNull(platform);
            Assert.Contains("sentinel-1a", platform);

            // Test that the instrument is set
            var instrument = eop.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName;
            Assert.NotNull(instrument);
            Assert.Contains("sar", instrument);

        }

        [Fact]
        public async System.Threading.Tasks.Task VAPTest()
        {
            string json = GetJson("Translators");

            StacItem stacItem = StacConvert.Deserialize<StacItem>(json);

            StacItemToAtomItemTranslator stacItemToAtomItemTranslator = new StacItemToAtomItemTranslator(ServiceProvider);

            StacItemNode stacItemNode = new StacItemNode(stacItem, new Uri("https://supervisor.disasterscharter.org/api/activations/act-874/vaps/act-874-vap-1002-6/items/act-874-vap-1002-6.json"));

            AtomItemNode atomItemNode = await stacItemToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            // find browse link
            var browseLink = atomItemNode.AtomItem.Links.FirstOrDefault(r => r.RelationshipType == "icon");

            Assert.NotNull(browseLink);
            Assert.True(browseLink.Uri.ToString() == "https://supervisor.disasterscharter.org/assets/activations/act-874/vaps/act-874-vap-1002-6/act-874-vap-1002-6.json?key=overview");
            // Check that the asset reference is set in the link attributes
            Assert.True(browseLink.AttributeExtensions.ContainsKey(new XmlQualifiedName("asset")));
            Assert.True(browseLink.AttributeExtensions[new XmlQualifiedName("asset")].ToString() == "overview");

            // Check that description is not in markdown
            Assert.DoesNotContain("Value----", atomItemNode.AtomItem.Summary.Text);

        }


    }

}
