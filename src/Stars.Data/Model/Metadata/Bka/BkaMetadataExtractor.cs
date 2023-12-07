using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Geometry.GeoJson;
using System.Xml;
using System.Xml.Serialization;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Stac.Extensions.Raster;




namespace Terradue.Stars.Data.Model.Metadata.Bka
{
    public class BkaMetadataExtractor : MetadataExtraction
    {
        public override string Label => "BKA (Belarus) mission product metadata extractor";

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(BkaMetadata));
 
        public BkaMetadataExtractor(ILogger<BkaMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                BkaMetadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                return metadata != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            BkaMetadata metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata);

            AddAssets(stacItem, item, metadata);

            return StacItemNode.Create(stacItem, item.Uri); ;
        }


        internal virtual StacItem CreateStacItem(BkaMetadata metadata)
        {
            string identifier = metadata.Processing.Scene01FileName.Replace(".tif", String.Empty);
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddProjViewExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            AddEoStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            AddOtherProperties(metadata, stacItem);

            return stacItem;
        }

        private void AddEoStacExtension(BkaMetadata metadata, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }


        private void AddSatStacExtension(BkaMetadata metadata, StacItem stacItem)
        {
            SatStacExtension sat = stacItem.SatExtension();
            sat.PlatformInternationalDesignator = "2012-039B";
        }

        private void AddProjStacExtension(BkaMetadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = metadata.GeoReference?.EpsgCode;
            if (metadata.ImageInfo != null && metadata.ImageInfo.Height != null && metadata.ImageInfo.Width != null)
            {
                proj.Shape = new int[2] { metadata.ImageInfo.Height.Value, metadata.ImageInfo.Width.Value };
            }
        }

        private void AddProjViewExtension(BkaMetadata metadata, StacItem stacItem)
        {
            ViewStacExtension view = stacItem.ViewExtension();
            if (metadata.SatelliteData != null && metadata.SatelliteData.SceneAcquisition != null)
            {
                BkaSceneAcquisition sceneAcquisition = metadata.SatelliteData.SceneAcquisition;
                if (sceneAcquisition.ViewingAngle != null) view.Azimuth = sceneAcquisition.ViewingAngle.Value;
                if (sceneAcquisition.SunAzimuth != null) view.Azimuth = sceneAcquisition.SunAzimuth.Value;
                if (sceneAcquisition.SunElevation != null) view.Azimuth = sceneAcquisition.SunElevation.Value;
            }
        }

        private void AddProcessingStacExtension(BkaMetadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            if (metadata.Processing != null && metadata.Processing.Level != null)
            {
                proc.Level = GetProcessingLevel(metadata);
            }
        }

        private string GetProcessingLevel(BkaMetadata metadata)
        {
            return String.Format("L1{0}", metadata.Processing.Level);
        }

        private string GetInstrument(BkaMetadata metadata)
        {
            return "MSS";
        }

        private IDictionary<string, object> GetCommonMetadata(BkaMetadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillPlatformDefinition(metadata, properties);


            return properties;
        }

        private void FillBasicsProperties(BkaMetadata metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                GetInstrument(metadata),
                GetProcessingLevel(metadata),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture))
            );
        }

        private void AddOtherProperties(BkaMetadata metadata, StacItem item)
        {
            IDictionary<String, object> properties = item.Properties;
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    properties,
                    "NAS", 
                    "BKA (formerly known as BelKa 2) is a Belarusian remote sensing satellite developed under an agreement between the National Academy of Sciences of Belarus (NAS) and the Federal Space Agency of the Russian Federation. The BKA satellite is almost an exact copy of the Russian Kanopus-Vulkan N1 Environmental Satellite (Kanopus-V 1).",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://gis.by/en/tech/bka/")
                );
            }
            properties["licence"] = "proprietary";
            if (metadata.GeoReference.SceneGeoposition.GroundResolution != null)
            {
                item.Gsd = metadata.GeoReference.SceneGeoposition.GroundResolution.PixelSizeNorthing;
            }
        }


        private void FillDateTimeProperties(BkaMetadata metadata, Dictionary<string, object> properties)
        {
            var format = "yyyy-MM-ddTHH:mm:ss";
            var format2 = "dd.MM.yyyy HH:mm:ss";

            if (DateTime.TryParseExact(metadata.SatelliteData.SceneAcquisition.AcquisitionTime, format, null, DateTimeStyles.AssumeUniversal, out DateTime dt))
            {
                properties["datetime"] = dt.ToUniversalTime().ToString("O");
            }
            if (DateTime.TryParseExact(metadata.Production?.CreationDate, format, null, DateTimeStyles.AssumeUniversal, out dt))
            {
                properties["created"] = dt.ToUniversalTime().ToString("O");
            }
            else if (DateTime.TryParseExact(metadata.Production?.CreationDate, format2, null, DateTimeStyles.AssumeUniversal, out dt))
            {
                properties["created"] = dt.ToUniversalTime().ToString("O");
            }
        }

        private void FillPlatformDefinition(BkaMetadata metadata, Dictionary<string, object> properties)
        {
            string platform = "bka";
            if (metadata.Processing?.Satellite != null) platform = metadata.Processing.Satellite.ToLower();
            string mission = "bka";
            if (metadata.Processing?.Mission != null) mission = metadata.Processing.Mission.ToLower();
            properties["platform"] = platform;
            properties["constellation"] = platform;
            properties["mission"] = mission;
            properties["instruments"] = new string[] { GetInstrument(metadata).ToLower() };
            properties["sensor_type"] = "optical";
            if (metadata.Processing?.Instrument != null)
            properties["spectral_mode"] = metadata.Processing?.Instrument;
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(BkaMetadata metadata)
        {
            BkaGeodeticCoordinates coordinates = metadata.GeoReference.SceneGeoposition.GeodeticCoordinates;
            List<GeoJSON.Net.Geometry.Position> positions = new List<Position>
            {
                new GeoJSON.Net.Geometry.Position(coordinates.Corner1SWLat, coordinates.Corner1SWLon),
                new GeoJSON.Net.Geometry.Position(coordinates.Corner4SELat, coordinates.Corner4SELon),
                new GeoJSON.Net.Geometry.Position(coordinates.Corner3NELat, coordinates.Corner3NELon),
                new GeoJSON.Net.Geometry.Position(coordinates.Corner2NWLat, coordinates.Corner2NWLon),
                new GeoJSON.Net.Geometry.Position(coordinates.Corner1SWLat, coordinates.Corner1SWLon)
            };
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(positions.ToArray());
            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
        }


        protected void AddAssets(StacItem stacItem, IItem item, BkaMetadata metadata)
        {
            IAsset imageAsset = FindFirstAssetFromFileNameRegex(item, @".*\.tif");
            if (imageAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"));
                stacAsset.Roles.Add("dn");

                string key = String.Format("{0}_{1}",
                    GetProcessingLevel(metadata),
                    metadata.Processing?.Instrument
                );
                stacAsset.Properties["title"] = key.Replace("_", " ");
                stacAsset.Properties.AddRange(imageAsset.Properties);

                BkaSpectral spectral = metadata.Radiometry.Spectral;
                if (spectral.BandMS1 != null) AddBandAsset(spectral.BandMS1, stacAsset);
                if (spectral.BandMS2 != null) AddBandAsset(spectral.BandMS2, stacAsset);
                if (spectral.BandMS3 != null) AddBandAsset(spectral.BandMS3, stacAsset);
                if (spectral.BandMS4 != null) AddBandAsset(spectral.BandMS4, stacAsset);

                stacItem.Assets.Add(key, stacAsset);
            }

            IAsset metadataAsset = GetMetadataAsset(item);
            if (metadataAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString())));
                stacItem.Assets.Add("metadata", stacAsset);
                stacAsset.Properties.AddRange(metadataAsset.Properties);
            }

            IAsset overviewAsset = FindFirstAssetFromFileNameRegex(item, @".*_preview\.jpg");
            if (overviewAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri, new ContentType(MimeTypes.GetMimeType(overviewAsset.Uri.ToString())));
                stacAsset.Properties.AddRange(overviewAsset.Properties);
                stacItem.Assets.Add("overview", stacAsset);
            }
        }


        private void AddBandAsset(BkaBandInfo bandInfo, StacAsset stacAsset = null)
        {
            double? waveLength;
            double? fullWidthHalfMax;
            double? scale = null;

            EoBandCommonName commonName;

            switch (bandInfo.BandCode)
            {
                case "MS1":
                    waveLength = 0.492;
                    fullWidthHalfMax = 0.04;
                    commonName = EoBandCommonName.blue;
                    break;
                case "MS2":
                    waveLength = 0.558;
                    fullWidthHalfMax = 0.07;
                    commonName = EoBandCommonName.green;
                    break;
                case "MS3":
                    waveLength = 0.675;
                    fullWidthHalfMax = 0.09;
                    commonName = EoBandCommonName.red;
                    break;
                case "MS4":
                    waveLength = 0.782;
                    fullWidthHalfMax = 0.1;
                    commonName = EoBandCommonName.nir;
                    break;
                default:
                    return;
            }

            EoBandObject eoBandObject = new EoBandObject(bandInfo.BandCode, commonName)
            {
                CenterWavelength = waveLength,
                FullWidthHalfMax = fullWidthHalfMax,
            };

            RasterBand rasterBand = new RasterBand()
            {
                DataType = Stac.Common.DataType.int8,
                Scale = scale,
                Offset = 0
            };

            EoStacExtension eo = stacAsset.EoExtension();
            if (eo.Bands == null)
            {
                eo.Bands = new EoBandObject[] { eoBandObject };
            }
            else
            {
                List<EoBandObject> bands = new List<EoBandObject>(eo.Bands) { eoBandObject };
                eo.Bands = bands.ToArray();
            }
            
            if (rasterBand != null)
            {
                RasterStacExtension raster = stacAsset.RasterExtension();
                if (raster.Bands == null)
                {
                    raster.Bands = new RasterBand[] { rasterBand };
                }
                else
                {
                    List<RasterBand> bands = new List<RasterBand>(raster.Bands) { rasterBand };
                    raster.Bands = bands.ToArray();
                }
            }
        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"^.*_pasp-en\.xml");
            if (metadataAsset != null) return metadataAsset;

            throw new FileNotFoundException(String.Format("Unable to find the summary file asset"));
        }

        public virtual async Task<BkaMetadata> ReadMetadata(IAsset metadataAsset)
        {
            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None))
            {
                XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };
                var reader = XmlReader.Create(stream, settings);
                
                logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                return (BkaMetadata)metadataSerializer.Deserialize(reader);
            }
        }
    }
}