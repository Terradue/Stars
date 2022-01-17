using System;
using System.Collections.Generic;
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
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;

namespace Terradue.Stars.Data.Model.Metadata.Vrss
{
    public class VrssMetadataExtractor : MetadataExtraction
    {
        // Possible identifiers:
        // VRSS-1_PAN-2_0698_0239_20200814_L2B_817135102196
        // VRSS-1_MSS-2_0698_0239_20200814_L2B_81713505511_1
        // VRSS-2_PAN_0996_0379_20210127_L2B_128183941985
        // VRSS-2_MSS_0996_0379_20210127_L2B_12818393908
        private Regex identifierRegex = new Regex(@"(?'id'VRSS-[12]_(?'type'[A-Z]{3})(-\d)?_\d{4}_\d{4}_\d{8}_.{3}_\d+)(_(?'n'\d+))?");

        private Regex utmZoneRegex = new Regex(@"(?'num'\d+)(?'hem'[NS])");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));
 
        public override string Label => "Venezuelan Remote Sensing Satellite (ABAE) mission product metadata extractor";

        public VrssMetadataExtractor(ILogger<VrssMetadataExtractor> logger) : base(logger)
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
            catch (Exception)
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

            AddEoStacExtension(metadata, stacItem);
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);

            return StacItemNode.Create(stacItem, item.Uri);;
        }

        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata)
        {

            Match identifierMatch = identifierRegex.Match(metadata.imageName);
            if (!identifierMatch.Success)
            {
                throw new InvalidOperationException(String.Format("Identifier not recognised from image name: {0}", metadata.imageName));
            }

            string identifier = identifierMatch.Groups["id"].Value;
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            
            return stacItem;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.dataLowerLeftLat),
                                                        double.Parse(metadata.dataLowerLeftLong)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.dataLowerRightLat),
                                                        double.Parse(metadata.dataLowerRightLong)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.dataUpperRightLat),
                                                        double.Parse(metadata.dataUpperRightLong)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.dataUpperLeftLat),
                                                        double.Parse(metadata.dataUpperLeftLong)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.dataLowerLeftLat),
                                                        double.Parse(metadata.dataLowerLeftLong)),
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"VRSS-[12]_.*\d\.xml$");
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
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                return (Schemas.Metadata)metadataSerializer.Deserialize(reader);
            }
        }


        private string GetProcessingLevel(Schemas.Metadata metadata)
        {
            switch (metadata.productLevel)
            {
                case "LEVEL2B":
                    return "L2B";
            }
            return null;
        }

        private IDictionary<string, object> GetCommonMetadata(Schemas.Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(metadata, properties);
            FillBasicsProperties(metadata, properties);

            return properties;
        }

        private void FillDateTimeProperties(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = DateTime.MinValue;
            bool hasStartDate = DateTime.TryParse(metadata.Scene_imagingStartTime, null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            bool hasEndDate = DateTime.TryParse(metadata.Scene_imagingStopTime, null, DateTimeStyles.AssumeUniversal, out endDate);

            if (hasStartDate && hasEndDate)
            {
                properties["start_datetime"] = startDate.ToUniversalTime();
                properties["end_datetime"] = endDate.ToUniversalTime();
                properties["datetime"] = startDate.ToUniversalTime();
            }
            else if (hasStartDate)
            {
                properties["datetime"] = startDate.ToUniversalTime();
            }

            DateTime createdDate = DateTime.MinValue;
            bool hasCreatedDate = DateTime.TryParse(metadata.productDate, null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (hasCreatedDate)
            {
                properties["created"] = createdDate.ToUniversalTime();
            }
        }


        private void FillInstrument(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties["platform"] = metadata.satelliteId.ToLower();
            properties["mission"] = metadata.satelliteId.ToLower();
            properties["instruments"] = new string[] { metadata.sensorId.ToLower() };
            properties["sensor_type"] = "optical";
            if (Double.TryParse(metadata.sensorGSD, out double gsd))
            {
                properties["gsd"] = gsd;
            }
        }

        private void FillBasicsProperties(Schemas.Metadata metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            properties["title"] = String.Format("{0} {1} {2} {3}",
                metadata.satelliteId,
                metadata.sensorId,
                GetProcessingLevel(metadata),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)
            );
        }


        private void AddEoStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
        }


        private void AddSatStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            if (int.TryParse(metadata.orbitId, out int absOrbit))
            {
                sat.AbsoluteOrbit = absOrbit;
            }
            // sat.RelativeOrbit = 
            // sat.OrbitState = 
            // sat.PlatformInternationalDesignator = 
        }


        private void AddProjStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            if (metadata.mapProjection != "UTM" || metadata.Zone_Number == null) return;
            Match utmZoneMatch = utmZoneRegex.Match(metadata.Zone_Number);
            if (!utmZoneMatch.Success) return;

            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            //proj.Wkt2 = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84.WKT;

            int zone = Int32.Parse(utmZoneMatch.Groups["num"].Value);
            bool north = utmZoneMatch.Groups["hem"].Value == "N";
            ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
            proj.SetCoordinateSystem(utm);
        }


        private void AddViewStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            if (Double.TryParse(metadata.satOffNadir, out double offNadir))
            {
                view.OffNadir = Math.Abs(offNadir);
            } 
            if (metadata.sunAzimuth != null && metadata.sunAzimuth.Length != 0 && Double.TryParse(metadata.sunAzimuth[0].Value, out double sunAzimuth))
            {
                view.SunAzimuth = sunAzimuth;
            }
            if (metadata.sunElevation != null && metadata.sunElevation.Length != 0 && Double.TryParse(metadata.sunElevation[0].Value, out double sunElevation))
            {
                view.SunElevation = sunElevation;
            }
        }


        private void AddProcessingStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }


        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata)
        {
            string[] imageFiles = metadata.imageName.Split(',');
            foreach (string imageFile in imageFiles)
            {
                IAsset imageAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", imageFile));
                if (imageAsset == null)
                    throw new FileNotFoundException(string.Format("Image file declared in metadata, but not present '{0}'", metadata.browseName));
                Match match = identifierRegex.Match(imageFile);
                
                string type = match.Groups["type"].Value;
                string key; 
                if ((type == "MSS" || type == "WMC") && match.Groups["n"].Success) key = String.Format("band-{0}", match.Groups["n"].Value);
                else if (type == "PAN") key = "pan";
                else key = "image";
                int index = (match.Groups["n"].Success ? Int32.Parse(match.Groups["n"].Value) - 1 : 0); 
                
                AddBandStacAsset(stacItem, type, key, index, imageAsset, metadata);
            }
            
            if (metadata.browseName != null)
            {
                IAsset browseAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", metadata.browseName));
                if (browseAsset == null)
                    throw new FileNotFoundException(string.Format("Browse image declared in metadata, but not present '{0}'", metadata.browseName));
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, browseAsset.Uri, new ContentType(MimeTypes.GetMimeType(browseAsset.Uri.OriginalString)), "Browse image"));
                stacItem.Assets["overview"].Properties.AddRange(browseAsset.Properties);
            }

            if (metadata.thumbName != null)
            {
                IAsset thumbAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", metadata.thumbName));
                if (thumbAsset == null)
                    throw new FileNotFoundException(string.Format("Thumbnail image declared in metadata, but not present '{0}'", metadata.thumbName));
                stacItem.Assets.Add("thumbnail", StacAsset.CreateThumbnailAsset(stacItem, thumbAsset.Uri, new ContentType(MimeTypes.GetMimeType(thumbAsset.Uri.OriginalString)), "Thumbnail image"));
                stacItem.Assets["thumbnail"].Properties.AddRange(thumbAsset.Properties);
            }

            IAsset metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.OriginalString)), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            IAsset additionalMetadataAsset = FindFirstAssetFromFileNameRegex(item, @"VRSS-[12]_.*ADDITION\.xml$");
            if (additionalMetadataAsset != null){
                stacItem.Assets.Add("metadata-addition", StacAsset.CreateMetadataAsset(stacItem, additionalMetadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(additionalMetadataAsset.Uri.OriginalString)), "Additional metadata"));
                stacItem.Assets["metadata-addition"].Properties.AddRange(additionalMetadataAsset.Properties);
            }
        }


        private void AddBandStacAsset(StacItem stacItem, string type, string assetkey, int index, IAsset imageAsset, Schemas.Metadata metadata)
        {

            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType(MimeTypes.GetMimeType(imageAsset.Uri.OriginalString)), "Image file");
            stacAsset.Properties.AddRange(imageAsset.Properties);
            double wavelength = 0;
            EoBandCommonName commonName = new EoBandCommonName();
            bool notFound = false;

            var dataSize = Array.ConvertAll<string, int>(metadata.dataSize.Split(','), a => Int32.Parse(a));
            stacAsset.ProjectionExtension().Shape = dataSize;

            if (type == "PAN")
            {
                string fileName = Path.GetFileName(imageAsset.Uri.OriginalString);
                if (fileName.StartsWith("VRSS-1"))
                    wavelength = 0.675;
                else if (fileName.StartsWith("VRSS-2"))
                    wavelength = 0.650;
                commonName = EoBandCommonName.pan;
            }
            else if (type == "MSS" || type == "WMC")
            {
                switch (index + 1) {
                    case 1:
                        wavelength = 0.485;
                        commonName = EoBandCommonName.blue;
                        break;
                    case 2:
                        wavelength = 0.555;
                        commonName = EoBandCommonName.green;
                        break;
                    case 3:
                        wavelength = 0.66;
                        commonName = EoBandCommonName.red;
                        break;
                    case 4:
                        wavelength = 0.83;
                        commonName = EoBandCommonName.nir;
                        break;
                    default:
                        notFound = true;
                        break;
                }
            }
            else
            {
                notFound = true;
            }

            if (notFound)
            {
                throw new InvalidOperationException(String.Format("Band information not found for {0}", assetkey));
            }

            EoBandObject eoBandObject = new EoBandObject(assetkey, commonName);
            eoBandObject.CenterWavelength = wavelength;
            if (index < metadata.ab_calibra_param.Length && index < metadata.SolarIrradiance.Length)
            {
                // Note: for VRSS-2 IRC-2 <C> has to be used instead of <B> for the offset calculation
                // No such sample data exists at the moment and no corresponding property in the metadata class
                bool existK = Double.TryParse(metadata.ab_calibra_param[index].K, out double k) && k != 0;
                bool existB = Double.TryParse(metadata.ab_calibra_param[index].B, out double b);
                if (Double.TryParse(metadata.SolarIrradiance[index].Value, out double eai))
                    eoBandObject.SolarIllumination = eai;
                if (existK && existB)
                {
                    RasterBand rasterBand = new RasterBand();
                    rasterBand.Scale = 1 / k;
                    rasterBand.Offset = - (b / k);
                    stacAsset.RasterExtension().Bands = new RasterBand[] { rasterBand };
                }
            }

            stacAsset.EoExtension().Bands = new EoBandObject[] { eoBandObject };
            stacItem.Assets.Add(assetkey, stacAsset);
        }

    }

}
