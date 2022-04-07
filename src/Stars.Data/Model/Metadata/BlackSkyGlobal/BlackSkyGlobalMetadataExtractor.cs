using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Eo;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;
using Newtonsoft.Json;

namespace Terradue.Stars.Data.Model.Metadata.BlackSkyGlobal
{
    public class BlackSkyGlobalMetadataExtractor : MetadataExtraction
    {
        // Possible identifiers:
        // CSKS4_SCS_B_HI_16_HH_RA_FF_20211016045150_20211016045156
        private Regex identifierRegex = new Regex(@"(?'id'CSKS(?'i'\d)_(?'pt'RAW_B|SCS_B|SCS_U|DGM_B|GEC_B|GTC_B)_(?'mode'HI|PP|WR|HR|S2)_(?'swath'..)_(?'pol'HH|VV|HV|VH|CO|CH|CV)_(?'look'L|R)(?'dir'A|D)_.._\d{14}_\d{14})");
        private Regex coordinateRegex = new Regex(@"(?'lat'[^ ]+) (?'lon'[^ ]+)");
        private static Regex h5dumpValueRegex = new Regex(@".*\(0\): *(?'value'.*)");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));
 
        public override string Label => "COSMO SkyMed (ASI) mission product metadata extractor";

        public BlackSkyGlobalMetadataExtractor(ILogger<BlackSkyGlobalMetadataExtractor> logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {   
                IAsset metadataAsset = GetMetadataAsset(item);
                Schemas.Metadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            Schemas.Metadata metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata, item);

            AddAssets(stacItem, item, metadata);

            AddSatStacExtension(stacItem, metadata);
            AddProjStacExtension(stacItem, metadata);
            AddViewStacExtension(stacItem, metadata);
            AddProcessingStacExtension(stacItem, metadata);
            AddEoStacExtension(stacItem, metadata);

            return StacItemNode.Create(stacItem, item.Uri);
        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"BSG.*\.json$");
            if (metadataAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return metadataAsset;
        }


        public virtual async Task<Schemas.Metadata> ReadMetadata(IAsset metadataAsset)
        {
            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            using (var stream = await metadataAsset.GetStreamable().GetStreamAsync())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);
                    string metadataStr = reader.ReadToEnd();
                    Schemas.Metadata metadata = JsonConvert.DeserializeObject<Schemas.Metadata>(metadataStr);
                    return metadata;
                }
            }
        }


        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata, IItem item)
        {
            string suffix = String.Empty;
            if (FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_ortho\.tif$", metadata.id)) != null) suffix = "_ortho";
            else if (FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_georeferenced\.tif$", metadata.id)) != null) suffix = "_non-ortho";

            string identifier = String.Format("{0}{1}", metadata.id, suffix);
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties, metadata);
            FillInstrument(properties, metadata);
            FillBasicsProperties(properties, metadata);

            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), properties);
            
            return stacItem;
        }


        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata)
        {
            //string imageFile = String.Format("{0}_ortho.tif", metadata.id);

            IAsset metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType("application/json"), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            // Overview/browse
            IAsset browseAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_browse\.png$", metadata.id));
            if (browseAsset != null)
            {
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, browseAsset.Uri, new ContentType("image/png"), "Browse image"));
                stacItem.Assets["overview"].Properties.AddRange(browseAsset.Properties);
            }

            // RGB TIFF (ortho)
            IAsset imageAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_ortho\.tif$", metadata.id));
            if (imageAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"), "RGB image file");
                stacItem.Assets.Add("ortho", stacAsset);
                stacAsset.Properties.AddRange(imageAsset.Properties);
                stacAsset.Properties["gsd"] = metadata.gsd;
                stacAsset.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("blue", EoBandCommonName.blue) { CenterWavelength = 0.485, FullWidthHalfMax = 0.07 },
                    new EoBandObject("green", EoBandCommonName.green) { CenterWavelength = 0.545, FullWidthHalfMax = 0.09 },
                    new EoBandObject("red", EoBandCommonName.red) { CenterWavelength = 0.645, FullWidthHalfMax = 0.11 },
                };
            }

            // PAN TIFF (ortho)
            IAsset imageAssetPan = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_ortho-pan\.tif$", metadata.id));
            if (imageAssetPan != null)
            {
                StacAsset stacAssetPan = StacAsset.CreateDataAsset(stacItem, imageAssetPan.Uri, new ContentType("image/tiff; application=geotiff"), "PAN image file");
                stacItem.Assets.Add("ortho-pan", stacAssetPan);
                stacAssetPan.Properties.AddRange(imageAssetPan.Properties);
                stacAssetPan.Properties["gsd"] = metadata.gsd;
                stacAssetPan.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("pan", EoBandCommonName.pan) { CenterWavelength = 0.575, FullWidthHalfMax = 0.25 },
                };
            }

            // RGB TIFF (non-ortho)
            imageAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_georeferenced\.tif$", metadata.id));
            if (imageAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"), "RGB image file");
                stacItem.Assets.Add("georeferenced", stacAsset);
                stacAsset.Properties.AddRange(imageAsset.Properties);
                stacAsset.Properties["gsd"] = metadata.gsd;
                stacAsset.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("blue", EoBandCommonName.blue) { CenterWavelength = 0.485, FullWidthHalfMax = 0.07 },
                    new EoBandObject("green", EoBandCommonName.green) { CenterWavelength = 0.545, FullWidthHalfMax = 0.09 },
                    new EoBandObject("red", EoBandCommonName.red) { CenterWavelength = 0.645, FullWidthHalfMax = 0.11 },
                };
            }

            // PAN TIFF (non-ortho)
            imageAssetPan = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_georeferenced-pan\.tif$", metadata.id));
            if (imageAssetPan != null)
            {
                StacAsset stacAssetPan = StacAsset.CreateDataAsset(stacItem, imageAssetPan.Uri, new ContentType("image/tiff; application=geotiff"), "PAN image file");
                stacItem.Assets.Add("georeferenced-pan", stacAssetPan);
                stacAssetPan.Properties.AddRange(imageAssetPan.Properties);
                stacAssetPan.Properties["gsd"] = metadata.gsd;
                stacAssetPan.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("pan", EoBandCommonName.pan) { CenterWavelength = 0.575, FullWidthHalfMax = 0.25 },
                };
            }

            // Mask TIFF (non-ortho)
            IAsset imageAssetMask = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_mask\.tif$", metadata.id));
            if (imageAssetMask != null)
            {
                StacAsset stacAssetMask = StacAsset.CreateDataAsset(stacItem, imageAssetMask.Uri, new ContentType("image/tiff; application=geotiff"), "Pixel mask");
                stacItem.Assets.Add("mask", stacAssetMask);
                stacAssetMask.Properties.AddRange(imageAssetMask.Properties);
                stacAssetMask.Properties["gsd"] = metadata.gsd;
            }

            // RGB RPC (non-ortho)
            IAsset rpcAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_georeferenced_rpc\.txt$", metadata.id));
            if (rpcAsset != null)
            {
                StacAsset stacAssetRpc = StacAsset.CreateMetadataAsset(stacItem, rpcAsset.Uri, new ContentType("text/plain"), "RPC (RGB)");
                stacItem.Assets.Add("georefrenced-rpc", stacAssetRpc);
                stacAssetRpc.Properties.AddRange(rpcAsset.Properties);
            }

            // PAN RPC (non-ortho)
            rpcAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_georeferenced-pan_rpc\.txt$", metadata.id));
            if (rpcAsset != null)
            {
                StacAsset stacAssetRpc = StacAsset.CreateMetadataAsset(stacItem, rpcAsset.Uri, new ContentType("text/plain"), "RPC (PAN)");
                stacItem.Assets.Add("georefrenced-pan-rpc", stacAssetRpc);
                stacAssetRpc.Properties.AddRange(rpcAsset.Properties);
            }

            // Mask RPC (non-ortho)
            rpcAsset = FindFirstAssetFromFileNameRegex(item, String.Format(@"{0}_mask_rpc\.txt$", metadata.id));
            if (rpcAsset != null)
            {
                StacAsset stacAssetRpc = StacAsset.CreateMetadataAsset(stacItem, rpcAsset.Uri, new ContentType("text/plain"), "RPC (mask)");
                stacItem.Assets.Add("mask-rpc", stacAssetRpc);
                stacAssetRpc.Properties.AddRange(rpcAsset.Properties);
            }
        }


        private void FillDateTimeProperties(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            DateTime? acquisitionDate = GetAcquisitionDateTime(metadata);
            if (acquisitionDate != null)
            {
                properties["datetime"] = acquisitionDate.Value.ToUniversalTime();
            }

        }


        private void FillInstrument(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            // platform & constellation
            
            properties["agency"] = "BlackSky";
            properties["platform"] = metadata.sensorName.ToLower();
            properties["mission"] = "Global";
            properties["instruments"] = new string[] { metadata.sensorName.ToLower() };
            properties["sensor_type"] = "optical";
            properties["gsd"] = metadata.gsd;

        }

        private void FillBasicsProperties(IDictionary<String, object> properties, Schemas.Metadata metadata)
        {
            DateTime? acquisitionDate = GetAcquisitionDateTime(metadata);
            string dateStr = (acquisitionDate != null ? String.Format(" {0:yyyy-MM-dd HH:mm:ss}", acquisitionDate.Value.ToUniversalTime()) : String.Empty);
            CultureInfo culture = new CultureInfo("fr-FR");
            properties["title"] = String.Format("{0}{1}",
                metadata.sensorName.ToUpper(),
                dateStr
            );
        }


        private void AddSatStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
        }


        private void AddProjStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();

            /*string ellipsoidDesignator = GetTagValue(metadata.AncillaryDataReference.Tag, "Ellipsoid Designator");
            if (ellipsoidDesignator == "WGS84")
            {
                proj.Epsg = 4326;
            }*/
        }


        private void AddViewStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = metadata.offNadirAngle;
            view.SunAzimuth = metadata.sunAzimuth;
            view.SunElevation = metadata.sunElevation;
        }


        private void AddProcessingStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = "ORTHO";
        }


        private void AddEoStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
            EoStacExtension eo = stacItem.EoExtension();
            eo.CloudCover = metadata.cloudCoverPercent;
            List<EoBandObject> bands = new List<EoBandObject>();
            foreach (StacAsset asset in stacItem.Assets.Values)
            {
                EoStacExtension assetEo = asset.EoExtension();
                if (assetEo.Bands != null) bands.AddRange(assetEo.Bands);
            }
            eo.Bands = bands.ToArray();
        }




        private DateTime? GetAcquisitionDateTime(Schemas.Metadata metadata)
        {
            DateTime acquisitionDate = DateTime.MinValue;
            if (DateTime.TryParse(metadata.acquisitionDate, null, DateTimeStyles.AssumeUniversal, out DateTime result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            string s = JsonConvert.SerializeObject(metadata.geometry);
            GeoJSON.Net.Geometry.Polygon polygon = JsonConvert.DeserializeObject<GeoJSON.Net.Geometry.Polygon>(s);
            return polygon;
        }


    }

}
