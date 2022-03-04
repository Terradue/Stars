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
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;
using Newtonsoft.Json;

namespace Terradue.Stars.Data.Model.Metadata.BlackSkyGlobal
{
    public class BlackSkyMetadataExtractor : MetadataExtraction
    {
        // Possible identifiers:
        // CSKS4_SCS_B_HI_16_HH_RA_FF_20211016045150_20211016045156
        private Regex identifierRegex = new Regex(@"(?'id'CSKS(?'i'\d)_(?'pt'RAW_B|SCS_B|SCS_U|DGM_B|GEC_B|GTC_B)_(?'mode'HI|PP|WR|HR|S2)_(?'swath'..)_(?'pol'HH|VV|HV|VH|CO|CH|CV)_(?'look'L|R)(?'dir'A|D)_.._\d{14}_\d{14})");
        private Regex coordinateRegex = new Regex(@"(?'lat'[^ ]+) (?'lon'[^ ]+)");
        private static Regex h5dumpValueRegex = new Regex(@".*\(0\): *(?'value'.*)");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));
 
        public override string Label => "COSMO SkyMed (ASI) mission product metadata extractor";

        public BlackSkyMetadataExtractor(ILogger<BlackSkyMetadataExtractor> logger) : base(logger)
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

            StacItem stacItem = CreateStacItem(metadata);

            AddAssets(stacItem, item, metadata);

            AddSatStacExtension(stacItem, metadata);
            AddProjStacExtension(stacItem, metadata);
            AddViewStacExtension(stacItem, metadata);
            AddProcessingStacExtension(stacItem, metadata);

            return StacItemNode.Create(stacItem, item.Uri);;
        }

        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata)
        {

            string identifier = metadata.id;
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            
            return stacItem;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            return null;
            //return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        private double[] GetCoordinates(string input)
        {
            Match coordinateMatch = coordinateRegex.Match(input);
            if (!coordinateMatch.Success)
            {
                throw new InvalidOperationException(String.Format("Invalid coordinate input: {0}", input));
            }

            if (!Double.TryParse(coordinateMatch.Groups["lat"].Value, out double lat) || !Double.TryParse(coordinateMatch.Groups["lon"].Value, out double lon))
            {
                throw new InvalidOperationException(String.Format("Invalid coordinate value: {0}", input));
            }

            return new double[] { lat, lon };

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


        private IDictionary<string, object> GetCommonMetadata(Schemas.Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties, metadata);
            FillInstrument(properties, metadata);
            FillBasicsProperties(properties, metadata);

            return properties;
        }

        private void FillDateTimeProperties(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            DateTime? acquisitionDate = GetAcquisitionDateTime(metadata);
            if (acquisitionDate != null)
            {
                properties["datetime"] = acquisitionDate.Value.ToUniversalTime();
            }

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


        private void FillInstrument(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            // platform & constellation
            
            properties["agency"] = "BlackSky";
            properties["platform"] = metadata.sensorName.ToLower();
            properties["mission"] = "Global";
            properties["instruments"] = new string[] { metadata.sensorName.ToLower() };
            properties["sensor_type"] = "optical";
        }

        private void FillBasicsProperties(IDictionary<String, object> properties, Schemas.Metadata metadata)
        {
            DateTime? acquisitionDate = GetAcquisitionDateTime(metadata);
            string dateStr = (acquisitionDate != null ? String.Format(" {0}", acquisitionDate.Value.ToUniversalTime().ToString("G")) : String.Empty);
            CultureInfo culture = new CultureInfo("fr-FR");
            properties["title"] = String.Format("BlackSky {0} {1}",
                metadata.sensorName,
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




        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata)
        {
        }


    }

}
