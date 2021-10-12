using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;
using Terradue.Stars.Services;

namespace Terradue.Stars.Data.Model.Metadata.Gaofen
{
    public class GaofenMetadataExtractor : MetadataExtraction
    {
        private const string GAOFEN1_PLATFORM_NAME = "Gaofen-1";
        private const string GAOFEN2_PLATFORM_NAME = "Gaofen-2";

        public override string Label => "Gaofen-1/2 High-resolution Imaging Satellite (CNSA) missions product metadata extractor";

        public GaofenMetadataExtractor(ILogger<GaofenMetadataExtractor> logger) : base(logger) { }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            IAsset metadataFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.xml)$");
            if (metadataFile == null)
            {
                return false;
            }

            IStreamable metadataFileStreamable = metadataFile.GetStreamable();
            if (metadataFileStreamable == null)
            {
                return false;
            }

            try
            {
                DeserializeProductMetadata( metadataFile.GetStreamable() ).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }

            return true;
        }


        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata files in the product package");


            List<IAsset> metadatafiles = FindAssetsFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.xml)$").ToList();


            if (metadatafiles == null || metadatafiles.Count == 0)
            {
                throw new FileNotFoundException("Unable to find any metadata file asset");
            }

            // deserialize product medatadatas
            List<ProductMetaData> productMetadataList = await DeserializeProductMetadataList(metadatafiles);

            // retrieving id from filename
            // GF2_PMS1_W91.0_N17.6_20200510_L1A0004793969-MSS1.xml
            string stacItemId = Path.GetFileNameWithoutExtension(metadatafiles[0].Uri.OriginalString).Split('-')[0];

            // retrieve lowest gsd
            double lowestGsd = GetLowestGsd(productMetadataList);

            // to retrieve the properties, any product metadata is ok
            var stacItem = GetStacItemWithProperties(productMetadataList[0], stacItemId, lowestGsd);

            string satelliteID = productMetadataList[0].SatelliteID;

            AddAssets(stacItem, satelliteID, item);

            // AddEoBandPropertyInItem(stacItem);
            FillBasicsProperties(productMetadataList[0], stacItem.Properties);

            return StacItemNode.CreateUnlocatedNode(stacItem);
        }


        private StacItem GetStacItemWithProperties(ProductMetaData productMetadata, string stacItemId, double gsd)
        {
            // retrieving GeometryObject from metadata
            var geometryObject = GetGeometryObjectFromProductMetadata(productMetadata);

            // retrieving the common metadata properties (i.e. time and instruments)
            var commonMetadata = GetCommonMetadata(productMetadata, gsd);

            // initializing the stac item object
            var stacItem = new StacItem(stacItemId, geometryObject, commonMetadata);

            AddEoStacExtension(productMetadata, stacItem);
            AddSatStacExtension(productMetadata, stacItem);
            AddViewStacExtension(productMetadata, stacItem);
            AddProcessingStacExtension(productMetadata, stacItem);
            AddOtherProperties(productMetadata, stacItem);

            return stacItem;
        }


        private double GetLowestGsd(List<ProductMetaData> productMetadataList)
        {
            double lowestGsd = double.Parse(productMetadataList[0].ImageGSD);
            // loop through each metadata file to extract the asset
            foreach (var productMetaData in productMetadataList)
            {
                if (lowestGsd > double.Parse(productMetaData.ImageGSD))
                {
                    lowestGsd = double.Parse(productMetaData.ImageGSD);
                }
            }

            return lowestGsd;
        }


        private async Task<List<ProductMetaData>> DeserializeProductMetadataList(List<IAsset> medatafileList)
        {
            List<ProductMetaData> productMetadataList = new List<ProductMetaData>();
            foreach (var metadataFile in medatafileList.OrderBy(m => m.Uri.ToString()))
            {
                logger.LogDebug("Metadata file is {0}", metadataFile.Uri);

                IStreamable metadataFileStreamable = metadataFile.GetStreamable();
                if (metadataFileStreamable == null)
                {
                    logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                }

                logger.LogDebug("Deserializing metadata files");
                ProductMetaData productMetadata = await DeserializeProductMetadata(metadataFileStreamable);
                logger.LogDebug("Metadata files deserialized. Starting metadata generation");

                productMetadataList.Add(productMetadata);
            }

            return productMetadataList;
        }


        private void FillBasicsProperties(ProductMetaData productMetadata,
            IDictionary<string, object> properties)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)));
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null)
                .SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        private void AddAssets(StacItem stacItem, string satelliteId, IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values.OrderBy(a => a.Uri.ToString()))
            {
                AddAsset(stacItem, satelliteId, asset);
            }
        }


        private void AddAsset(StacItem stacItem, string satelliteId, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            string sensorName = filename.Split('_')[1];
            // thumbnail
            if (filename.EndsWith("-MSS1_thumb.jpg", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-MSS2_thumb.jpg", true, CultureInfo.InvariantCulture)
            )
            {
                stacItem.Assets.Add("MSS-thumbnail",
                    GetGenericAsset(stacItem, asset.Uri, "thumbnail"));
                stacItem.Assets["MSS-thumbnail"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.StartsWith("GF1", true, CultureInfo.InvariantCulture) &&
                filename.EndsWith("thumb.jpg", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("MSS-thumbnail",
                    GetGenericAsset(stacItem, asset.Uri, "thumbnail"));
                stacItem.Assets["MSS-thumbnail"].Properties.AddRange(asset.Properties);
                return;
            }


            if (filename.EndsWith("-PAN1_thumb.jpg", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-PAN2_thumb.jpg", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("PAN-thumbnail",
                    GetGenericAsset(stacItem, asset.Uri, "thumbnail"));
                stacItem.Assets["PAN-thumbnail"].Properties.AddRange(asset.Properties);
                return;
            }

            // overview
            if (filename.EndsWith("-MSS1.jpg", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-MSS2.jpg", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.TryAdd("MSS-overview",
                    GetGenericAsset(stacItem, asset.Uri, "overview"));
                stacItem.Assets["MSS-overview"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.StartsWith("GF1", true, CultureInfo.InvariantCulture) &&
                filename.EndsWith(".jpg", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.TryAdd("MSS-overview",
                    GetGenericAsset(stacItem, asset.Uri, "overview"));
                stacItem.Assets["MSS-overview"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.EndsWith("-PAN1.jpg", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-PAN2.jpg", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("PAN-overview",
                    GetGenericAsset(stacItem, asset.Uri, "overview"));
                stacItem.Assets["PAN-overview"].Properties.AddRange(asset.Properties);
                return;
            }

            // metadata
            if (filename.EndsWith("-MSS1.xml", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-MSS2.xml", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("MSS-metadata",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["MSS-metadata"].Properties.AddRange(asset.Properties);
                return;
            }


            if (filename.StartsWith("GF1", true, CultureInfo.InvariantCulture) &&
               filename.EndsWith(".xml", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("MSS-metadata",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["MSS-metadata"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.EndsWith("-PAN1.xml", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-PAN2.xml", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("PAN-metadata",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["PAN-metadata"].Properties.AddRange(asset.Properties);
                return;
            }

            // rpb metadata
            if (filename.EndsWith("-MSS1.rpb", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-MSS2.rpb", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("MSS-rpb",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["MSS-rpb"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.StartsWith("GF1", true, CultureInfo.InvariantCulture) &&
               filename.EndsWith(".rpb", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("MSS-rpb",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["MSS-rpb"].Properties.AddRange(asset.Properties);
                return;
            }

            if (filename.EndsWith("-PAN1.rpb", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-PAN2.rpb", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("PAN-rpb",
                    GetGenericAsset(stacItem, asset.Uri, "metadata"));
                stacItem.Assets["PAN-rpb"].Properties.AddRange(asset.Properties);
                return;
            }


            if (filename.EndsWith("-MSS1.tiff", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-MSS2.tiff", true, CultureInfo.InvariantCulture))
            {
                string mssBandName = "MSS";
                var bandAsset = GetBandAsset(stacItem, mssBandName, sensorName, asset, satelliteId);
                stacItem.Assets.Add(mssBandName, bandAsset);
                return;
            }

            // GAOFEN1 WFV 1 2 3 4
            if (filename.StartsWith("GF1_WFV", true, CultureInfo.InvariantCulture) &&
                filename.EndsWith(".tiff", true, CultureInfo.InvariantCulture))
            {
                string mssBandName = "MSS";
                var bandAsset = GetBandAsset(stacItem, mssBandName, sensorName, asset, satelliteId);
                stacItem.Assets.Add(mssBandName, bandAsset);
                return;
            }


            if (filename.EndsWith("-PAN1.tiff", true, CultureInfo.InvariantCulture) ||
                filename.EndsWith("-PAN2.tiff", true, CultureInfo.InvariantCulture))
            {
                string panBandName = "PAN";
                var bandAsset = GetBandAsset(stacItem, panBandName, sensorName, asset, satelliteId);
                stacItem.Assets.Add(panBandName, bandAsset);
                return;
            }
        }


        private StacAsset GetBandAsset(StacItem stacItem, string bandName, string sensorName, IAsset asset, string satelliteId)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri,
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()))
            );
            stacAsset.Properties.AddRange(asset.Properties);
            ////////////
            // GAOFEN 1
            if (satelliteId == "GF1")
            {
                if (sensorName == "PMS1")
                {
                    if (bandName == "PAN")
                    {
                        stacAsset.SetProperty("gsd", 2);
                        EoBandObject eoBandObject =
                            CreateEoBandObject("PAN", EoBandCommonName.pan, 0.675, 0.450, 1361.43);
                        stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                        RasterBand rasterBandObject = CreateRasterBandObject(0.0, 0.1982);
                        stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBandObject };
                    }
                    else
                    {
                        stacAsset.Properties.Add("gsd", 8);
                        EoBandObject b01EoBandObject =
                            CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1966.811);
                        EoBandObject b02EoBandObject =
                            CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1822.607);
                        EoBandObject b03EoBandObject =
                            CreateEoBandObject("B03", EoBandCommonName.red, 0.660, 0.06, 1523.189);
                        EoBandObject b04EoBandObject =
                            CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1066.547);
                        stacAsset.EoExtension().Bands = new[]
                            {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};


                        RasterBand b01RasterBandObject =
                            CreateRasterBandObject(0.0, 0.1982);
                        RasterBand b02RasterBandObject =
                            CreateRasterBandObject(0.0, 0.232);
                        RasterBand b03RasterBandObject =
                            CreateRasterBandObject(0.0, 0.187);
                        RasterBand b04RasterBandObject =
                            CreateRasterBandObject(0.0, 0.1795);
                        stacAsset.RasterExtension().Bands = new[]
                            {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                    }
                }
                else if (sensorName == "PMS2")
                {
                    if (bandName == "PAN")
                    {
                        stacAsset.SetProperty("gsd", 2);
                        EoBandObject eoBandObject =
                            CreateEoBandObject("PAN", EoBandCommonName.pan, 0.675, 0.45, 0.1979);
                        stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                        RasterBand rasterBandObject = CreateRasterBandObject(0.0, 0.1979);
                        stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBandObject };
                    }
                    else
                    {
                        stacAsset.Properties.Add("gsd", 8);
                        EoBandObject b01EoBandObject =
                            CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1967.309);
                        EoBandObject b02EoBandObject =
                            CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1822.157);
                        EoBandObject b03EoBandObject =
                            CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1524.103);
                        EoBandObject b04EoBandObject =
                            CreateEoBandObject("B04", EoBandCommonName.nir, 0.830, 0.12, 1067.72);


                        stacAsset.EoExtension().Bands = new[]
                            {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};


                        RasterBand b01RasterBandObject =
                            CreateRasterBandObject(0.0, 0.224);
                        RasterBand b02RasterBandObject =
                            CreateRasterBandObject(0.0, 0.1851);
                        RasterBand b03RasterBandObject =
                            CreateRasterBandObject(0.0, 0.1793);
                        RasterBand b04RasterBandObject =
                            CreateRasterBandObject(0.0, 0.1863);
                        stacAsset.RasterExtension().Bands = new[]
                            {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                    }
                }
                else if (sensorName == "WFV1")
                {
                    stacAsset.SetProperty("gsd", 16);
                    // gaofen1 with sensor WFV1 only have Multispectral bands
                    EoBandObject b01EoBandObject =
                        CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1978.77);
                    EoBandObject b02EoBandObject =
                        CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1816.156);
                    EoBandObject b03EoBandObject =
                        CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1548.074);
                    EoBandObject b04EoBandObject =
                        CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.120, 1064.252);

                    stacAsset.EoExtension().Bands = new[]
                        {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                    RasterBand b01RasterBandObject =
                        CreateRasterBandObject(-0.0039, 0.1709);
                    RasterBand b02RasterBandObject =
                        CreateRasterBandObject(-0.0047, 0.1398);
                    RasterBand b03RasterBandObject =
                        CreateRasterBandObject(-0.003, 0.1195);
                    RasterBand b04RasterBandObject =
                        CreateRasterBandObject(-0.0274, 0.1338);
                    stacAsset.RasterExtension().Bands = new[]
                        {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                }
                else if (sensorName == "WFV2")
                {
                    stacAsset.SetProperty("gsd", 16);
                    // gaofen1 with sensor WFV2 only have Multispectral bands
                    EoBandObject b01EoBandObject =
                        CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1978.77);
                    EoBandObject b02EoBandObject =
                        CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1816.156);
                    EoBandObject b03EoBandObject =
                        CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1546.325);
                    EoBandObject b04EoBandObject =
                        CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1075.322);

                    stacAsset.EoExtension().Bands = new[]
                        {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                    RasterBand b01RasterBandObject =
                        CreateRasterBandObject(5.5303, 0.1588);
                    RasterBand b02RasterBandObject =
                        CreateRasterBandObject(-13.642, 0.1515);
                    RasterBand b03RasterBandObject =
                        CreateRasterBandObject(-15.382, 0.1251);
                    RasterBand b04RasterBandObject =
                        CreateRasterBandObject(-7.985, 0.1209);
                    stacAsset.RasterExtension().Bands = new[]
                        {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                }
                else if (sensorName == "WFV3")
                {
                    stacAsset.SetProperty("gsd", 16);
                    // gaofen1 with sensor WFV3 only have Multispectral bands
                    EoBandObject b01EoBandObject =
                        CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1979.535);
                    EoBandObject b02EoBandObject =
                        CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1808.804);
                    EoBandObject b03EoBandObject =
                        CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1524.958);
                    EoBandObject b04EoBandObject =
                        CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1069.152);

                    stacAsset.EoExtension().Bands = new[]
                        {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                    RasterBand b01RasterBandObject =
                        CreateRasterBandObject(12.28, 0.1556);
                    RasterBand b02RasterBandObject =
                        CreateRasterBandObject(-7.9336, 0.17);
                    RasterBand b03RasterBandObject =
                        CreateRasterBandObject(-7.031, 0.1392);
                    RasterBand b04RasterBandObject =
                        CreateRasterBandObject(-4.3578, 0.1354);
                    stacAsset.RasterExtension().Bands = new[]
                        {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                }
                else if (sensorName == "WFV4")
                {
                    stacAsset.SetProperty("gsd", 16);
                    // gaofen1 with sensor WFV4 only have Multispectral bands
                    EoBandObject b01EoBandObject =
                        CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1997.109);
                    EoBandObject b02EoBandObject =
                        CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1810.212);
                    EoBandObject b03EoBandObject =
                        CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1524.561);
                    EoBandObject b04EoBandObject =
                        CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1054.761);

                    stacAsset.EoExtension().Bands = new[]
                        {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                    RasterBand b01RasterBandObject =
                        CreateRasterBandObject(3.6469, 0.1819);
                    RasterBand b02RasterBandObject =
                        CreateRasterBandObject(-13.54, 0.1762);
                    RasterBand b03RasterBandObject =
                        CreateRasterBandObject(-10.998, 0.1463);
                    RasterBand b04RasterBandObject =
                        CreateRasterBandObject(-12.142, 0.1522);
                    stacAsset.RasterExtension().Bands = new[]
                        {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                }
            }

            ////////////
            // GAOFEN 2
            else if (satelliteId == "GF2")
            {
                if (sensorName == "PMS1")
                {
                    if (bandName == "PAN")
                    {
                        stacAsset.SetProperty("gsd", 0.81);
                        EoBandObject eoBandObject =
                            CreateEoBandObject("PAN", EoBandCommonName.pan, 0.670, 0.440, 1361.43);
                        stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                        RasterBand rasterBandObject = CreateRasterBandObject(-0.6077, 0.163);
                        stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBandObject };
                    }
                    else
                    {
                        stacAsset.SetProperty("gsd", 3.24);
                        EoBandObject b01EoBandObject =
                            CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1966.811);
                        EoBandObject b02EoBandObject =
                            CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1822.607);
                        EoBandObject b03EoBandObject =
                            CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1523.189);
                        EoBandObject b04EoBandObject =
                            CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1066.547);


                        stacAsset.EoExtension().Bands = new[]
                            {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                        RasterBand b01RasterBandObject =
                            CreateRasterBandObject(-0.8765, 0.1585);
                        RasterBand b02RasterBandObject =
                            CreateRasterBandObject(-0.9742, 0.1883);
                        RasterBand b03RasterBandObject =
                            CreateRasterBandObject(-0.7652, 0.174);
                        RasterBand b04RasterBandObject =
                            CreateRasterBandObject(-0.7233, 0.1897);
                        stacAsset.RasterExtension().Bands = new[]
                            {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                    }
                }
                else if (sensorName == "PMS2")
                {
                    if (bandName == "PAN")
                    {
                        stacAsset.SetProperty("gsd", 0.81);
                        EoBandObject eoBandObject =
                            CreateEoBandObject("PAN", EoBandCommonName.pan, 0.670, 0.440, 1363.494);
                        stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                        RasterBand rasterBandObject = CreateRasterBandObject(0.1654, 0.1823);
                        stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBandObject };
                    }
                    else
                    {
                        stacAsset.SetProperty("gsd", 3.24);
                        EoBandObject b01EoBandObject =
                            CreateEoBandObject("B01", EoBandCommonName.blue, 0.485, 0.07, 1967.309);
                        EoBandObject b02EoBandObject =
                            CreateEoBandObject("B02", EoBandCommonName.green, 0.555, 0.07, 1822.157);
                        EoBandObject b03EoBandObject =
                            CreateEoBandObject("B03", EoBandCommonName.red, 0.66, 0.06, 1524.103);
                        EoBandObject b04EoBandObject =
                            CreateEoBandObject("B04", EoBandCommonName.nir, 0.83, 0.12, 1067.72);


                        stacAsset.EoExtension().Bands = new[]
                            {b01EoBandObject, b02EoBandObject, b03EoBandObject, b04EoBandObject};

                        RasterBand b01RasterBandObject =
                            CreateRasterBandObject(-0.593, 0.1748);
                        RasterBand b02RasterBandObject =
                            CreateRasterBandObject(-0.2717, 0.1817);
                        RasterBand b03RasterBandObject =
                            CreateRasterBandObject(-0.2717, 0.1741);
                        RasterBand b04RasterBandObject =
                            CreateRasterBandObject(-0.2773, 0.1975);
                        stacAsset.RasterExtension().Bands = new[]
                            {b01RasterBandObject, b02RasterBandObject, b03RasterBandObject, b04RasterBandObject};
                    }
                }
            }

            return stacAsset;
        }

        private EoBandObject CreateEoBandObject(string name, EoBandCommonName eoBandCommonName, double centerWaveLength,
            double fullWidthHalfMax, double eai)
        {
            EoBandObject eoBandObject = new EoBandObject(name, eoBandCommonName);
            eoBandObject.Properties.Add("full_width_half_max", fullWidthHalfMax);
            eoBandObject.SolarIllumination = eai;
            eoBandObject.CenterWavelength = centerWaveLength;
            return eoBandObject;
        }

        private RasterBand CreateRasterBandObject(double offset, double gain)
        {
            RasterBand rasterBandObject = new RasterBand();
            rasterBandObject.Offset = offset;
            rasterBandObject.Scale = gain;
            return rasterBandObject;
        }


        private StacAsset GetGenericAsset(StacItem stacItem, Uri uri, string role)
        {
            StacAsset stacAsset = new StacAsset(stacItem, uri);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType =
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(uri.ToString())));
            return stacAsset;
        }

        private void AddOtherProperties(ProductMetaData productMetadata, StacItem stacItem)
        {
            stacItem.Properties.Add("product_type", "PAN_MS_" + productMetadata.ProductLevel.Replace("LEVEL", "L"));
        }

        private void AddProcessingStacExtension(ProductMetaData productMetadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = productMetadata.ProductLevel.Replace("LEVEL", "L");
        }

        private void AddViewStacExtension(ProductMetaData productMetadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = double.Parse(productMetadata.PitchViewingAngle);
            view.IncidenceAngle = double.Parse(productMetadata.RollViewingAngle);
            //view.Azimuth = double.Parse(productMetadata.;
            view.SunAzimuth = double.Parse(productMetadata.SolarAzimuth);
            view.SunElevation = double.Parse(productMetadata.SolarZenith);
        }

        private void AddSatStacExtension(ProductMetaData productMetadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = int.Parse(productMetadata.OrbitID);
            sat.RelativeOrbit = int.Parse(productMetadata.OrbitID);
            sat.OrbitState = "descending";
        }


        private void AddEoStacExtension(ProductMetaData productMetadata, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
            if (productMetadata.CloudPercent != null)
                eo.CloudCover = double.Parse(productMetadata.CloudPercent) / 100;
            else
                eo.CloudCover = 0;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometryObjectFromProductMetadata(
            ProductMetaData productMetadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5] {
                    new GeoJSON.Net.Geometry.Position(productMetadata.BottomLeftLatitude,
                        productMetadata.BottomLeftLongitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.BottomRightLatitude,
                        productMetadata.BottomRightLongitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.TopRightLatitude,
                        productMetadata.TopRightLongitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.TopLeftLatitude,
                        productMetadata.TopLeftLongitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.BottomLeftLatitude,
                        productMetadata.BottomLeftLongitude)
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new[] { lineString });
        }

        private IDictionary<string, object> GetCommonMetadata(ProductMetaData productMetadata, double gsd)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            FillDateTimeProperties(productMetadata, properties);
            FillInstrument(productMetadata, properties, gsd);

            return properties;
        }


        public static async Task<ProductMetaData> DeserializeProductMetadata(IStreamable productMetadataFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(ProductMetaData));
            ProductMetaData auxiliary;
            using (var stream = await productMetadataFile.GetStreamAsync())
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (ProductMetaData)ser.Deserialize(reader);
                }
            }

            return auxiliary;
        }


        private void FillDateTimeProperties(ProductMetaData productMetadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime.TryParseExact(productMetadata.StartTime, format, provider, DateTimeStyles.AssumeUniversal,
                out var startDate);
            DateTime.TryParseExact(productMetadata.EndTime, format, provider, DateTimeStyles.AssumeUniversal,
                out var endDate);

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate);

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            // datetime, start_datetime, end_datetime
            if (dateInterval.IsAnytime)
            {
                properties.Add("datetime", null);
            }

            if (dateInterval.IsMoment)
            {
                properties.Add("datetime", dateInterval.Start.ToUniversalTime());
            }
            else
            {
                properties.Add("datetime", dateInterval.Start.ToUniversalTime());
                properties.Add("start_datetime", dateInterval.Start.ToUniversalTime());
                properties.Add("end_datetime", dateInterval.End.ToUniversalTime());
            }

            DateTime.TryParseExact(productMetadata.ProduceTime, format, provider, DateTimeStyles.AssumeUniversal,
                out var createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate.ToUniversalTime());
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillInstrument(ProductMetaData productMetadata,
            Dictionary<string, object> properties, double gsd)
        {
            string platformName = "";
            // platform & constellation
            if (productMetadata.SatelliteID == "GF1")
            {
                platformName = GAOFEN1_PLATFORM_NAME.ToLower();
            }
            else if (productMetadata.SatelliteID == "GF2")
            {
                platformName = GAOFEN2_PLATFORM_NAME.ToLower();
            }
            else
            {
                throw new InvalidDataException("Platform id not found or not recognized");
            }


            if (!string.IsNullOrEmpty(platformName))
            {
                properties.Remove("platform");
                properties.Add("platform", platformName);

                properties.Remove("constellation");
                properties.Add("constellation", platformName);

                properties.Remove("mission");
                properties.Add("mission", platformName);
            }

            // instruments
            var instrumentName = productMetadata.SensorID.ToLower();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }

            properties.Remove("gsd");
            properties.Add("gsd", gsd);
        }
    }
}