using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.Sar;
using Stac.Extensions.View;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using System.Xml.Linq;

namespace Terradue.Stars.Data.Model.Metadata.TerrasarX
{
    public class TerrasarXMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(level1Product));

        public override string Label => "TerraSAR-X/TanDEM-X (DLR) missions product metadata extractor";

        public TerrasarXMetadataExtractor(ILogger<TerrasarXMetadataExtractor> logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                level1Product metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            level1Product metadata = await ReadMetadata(metadataAsset);

            // Create Stac Item
            StacItem stacItem = CreateStacItem(metadata, item);
            // Add assets
            AddAssets(stacItem, item, metadata);

            var stacNode = StacItemNode.Create(stacItem, item.Uri);

            return stacNode;
        }

        internal virtual StacItem CreateStacItem(level1Product metadata, IItem item)
        {
            string file_name = metadata.generalHeader.fileName.Substring(0, metadata.generalHeader.fileName.Length - 4);
            StacItem stacItem = new StacItem(file_name, GetGeometry(metadata, item), GetCommonMetadata(metadata, item));
            AddSatStacExtension(metadata, stacItem);
            //AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddSarStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        private void AddProcessingStacExtension(level1Product metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }

        private string GetProcessingLevel(level1Product metadata)
        {
            var product_type = metadata.setup.orderInfo.orderType;
            return product_type.Substring(0, product_type.IndexOf("-"));
        }


        private void AddProjStacExtension(level1Product metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Wkt2 = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84.WKT;
        }

        private void AddViewStacExtension(level1Product metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.SunAzimuth = Math.Round(metadata.productInfo.sceneInfo.headingAngle, 4);
            try
            {
                view.OffNadir = (metadata.productInfo.acquisitionInfo.imagingModeSpecificInfo.Item as level1ProductProductInfoAcquisitionInfoImagingModeSpecificInfoSpotLight).azimuthSteeringAngleFirst;
            }
            catch { }
            view.IncidenceAngle = metadata.productInfo.sceneInfo.sceneCenterCoord.incidenceAngle;
        }

        private void AddSatStacExtension(level1Product metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = metadata.productInfo.missionInfo.absOrbit;
            sat.RelativeOrbit = metadata.productInfo.missionInfo.relOrbit;
            sat.OrbitState = metadata.productInfo.missionInfo.orbitDirection.ToString().ToLower();
            sat.PlatformInternationalDesignator = "2019-038D";
        }


        private string GetProductType(level1Product metadata)
        {
            return metadata.productInfo.productVariantInfo.productVariant.ToString();
        }

        private string GetInstrumentMode(level1Product metadata)
        {
            return metadata.setup.orderInfo.imagingMode.ToString();
        }

        private SarCommonFrequencyBandName GetFrequencyBand(level1Product metadata)
        {
            return SarCommonFrequencyBandName.X;
        }

        private void AddSarStacExtension(level1Product metadata, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();

            sar.Required(
                GetInstrumentMode(metadata),
                GetFrequencyBand(metadata),
                GetPolarizations(metadata),
                GetProductType(metadata)
            );

            sar.ObservationDirection = ParseObservationDirection(metadata.productInfo.acquisitionInfo.lookDirection.ToString());
            sar.CenterFrequency = metadata.instrument.radarParameters.centerFrequency;
        }

        private IDictionary<string, object> GetCommonMetadata(level1Product metadata, IItem item)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(metadata, properties, item);
            FillBasicsProperties(metadata, properties);

            return properties;
        }

        private void FillInstrument(level1Product metadata, Dictionary<string, object> properties, IItem item)
        {
            properties.Remove("platform");
            properties.Add("platform", metadata.productInfo.missionInfo.mission.ToLower());

            properties.Remove("constellation");
            properties.Add("constellation", metadata.productInfo.missionInfo.mission.ToLower());

            properties.Remove("mission");
            properties.Add("mission", metadata.productInfo.missionInfo.mission.ToLower());

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { "x-sar" });

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

            properties.Remove("gsd");
            properties.Add("gsd", GetGroundSampleDistance(metadata, item));
        }

        private void FillDateTimeProperties(level1Product metadata, Dictionary<string, object> properties)
        {
            var format = "yyyy-MM-ddTHH:mm:ss.ffffffZ";
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = startDate;
            DateTime.TryParse(metadata.productInfo.sceneInfo.start.timeUTC.ToUniversalTime().ToString(format), null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime.TryParse(metadata.productInfo.sceneInfo.stop.timeUTC.ToUniversalTime().ToString(format), null, DateTimeStyles.AssumeUniversal, out endDate);




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
                properties.Add("datetime", dateInterval.Start.ToUniversalTime().ToString(format));
            }
            else
            {
                properties.Add("datetime", dateInterval.Start.ToUniversalTime().ToString(format));
                properties.Add("start_datetime", dateInterval.Start.ToUniversalTime().ToString(format));
                properties.Add("end_datetime", dateInterval.End.ToUniversalTime().ToString(format));
            }

            DateTime createdDate = metadata.generalHeader.generationTime;
            DateTime.TryParse(createdDate.ToString(), null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate.ToUniversalTime().ToString(format));
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow.ToString(format));
        }

        private void FillBasicsProperties(level1Product metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3} {4} {5} {6}",
                                                  StylePlatform(properties.GetProperty<string>("platform").ToUpper()),
                                                  metadata.productInfo.acquisitionInfo.sensor,              //SAR
                                                  GetProductType(metadata),
                                                  metadata.setup.orderInfo.imagingMode,                     //SM
                                                  metadata.productInfo.missionInfo.relOrbit, //164
                                                  string.Join("/", GetPolarizations(metadata)),
                                                  metadata.productInfo.sceneInfo.start.timeUTC.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")));

            //TSX-1 SAR L1B SM 164 HH 2018-09-29T21:46:04.680000
        }



        private XmlDocument ReadGeometryFile(IItem item)
        {
            var coordinatesDocument = new XmlDocument();
            var fileAsset = FindFirstAssetFromFileNameRegex(item, @".*_iif.*\.xml");
            if (fileAsset == null)
                throw new FileNotFoundException(string.Format("Coordinates file not found "));

            coordinatesDocument.Load(fileAsset.GetStreamable().GetStreamAsync().GetAwaiter().GetResult());


            return coordinatesDocument;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(level1Product metadata, IItem item)
        {
            // Summary:
            //     Return coordinates of Polygon 
            //
            // Parameters:
            //   metadata:
            //     Metadata of given XML.

            var nodeList = ReadGeometryFile(item).SelectNodes("/IIF/spatialCoverage/boundingPolygon/point");
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{

                    new GeoJSON.Net.Geometry.Position(double.Parse(nodeList[0].SelectSingleNode("latitude").InnerText),
                                                        double.Parse(nodeList[0].SelectSingleNode("longitude").InnerText)),

                    new GeoJSON.Net.Geometry.Position(double.Parse(nodeList[1].SelectSingleNode("latitude").InnerText),
                                                        double.Parse(nodeList[1].SelectSingleNode("longitude").InnerText)),

                    new GeoJSON.Net.Geometry.Position(double.Parse(nodeList[2].SelectSingleNode("latitude").InnerText),
                                                        double.Parse(nodeList[2].SelectSingleNode("longitude").InnerText)),

                    new GeoJSON.Net.Geometry.Position(double.Parse(nodeList[3].SelectSingleNode("latitude").InnerText),
                                                        double.Parse(nodeList[3].SelectSingleNode("longitude").InnerText)),

                    new GeoJSON.Net.Geometry.Position(double.Parse(nodeList[0].SelectSingleNode("latitude").InnerText),
                                                        double.Parse(nodeList[0].SelectSingleNode("longitude").InnerText))
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        protected void AddAssets(StacItem stacItem, IItem item, level1Product metadata)
        {
            string tifFileName = metadata.productComponents.imageData[0].file.location.filename;
            IAsset amplitudeAsset = FindFirstAssetFromFileNameRegex(item, tifFileName + "$");

            if (amplitudeAsset == null)
                throw new FileNotFoundException(string.Format("No product found '{0}'", tifFileName));
            var bandStacAsset = AddBandAsset(stacItem, amplitudeAsset, metadata, item); //amplitude-hh

            var metadataAsset = FindFirstAssetFromFileNameRegex(item, @".*L1B.*\.xml"); //metadata
            if (metadataAsset != null)
            {
                stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString())), "Metadata file"));
                stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);
            }
            else
                throw new FileNotFoundException(string.Format("L1B metadata file not found "));


            var metadataProductAsset = GetMetadataAsset(item);      //product-metadata
            if (metadataProductAsset != null){
                stacItem.Assets.Add("product-metadata", StacAsset.CreateMetadataAsset(stacItem, metadataProductAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(metadataProductAsset.Uri.ToString())), "Metadata file"));
                stacItem.Assets["product-metadata"].Properties.AddRange(metadataProductAsset.Properties);
            }
            else
                throw new FileNotFoundException(string.Format("TSX-1 product-metadata file not found "));


            IAsset overviewAsset = FindFirstAssetFromFileNameRegex(item, @".*COMPOSITE.*\.tif"); //overwiew
            if (overviewAsset == null)
                throw new FileNotFoundException(string.Format("No product found '{0}'", ""));

            stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri, new ContentType(MimeTypes.GetMimeType(overviewAsset.Uri.ToString()))));
            stacItem.Assets["overview"].Properties.AddRange(overviewAsset.Properties);

        }


        private string AddBandAsset(StacItem stacItem, IAsset bandAsset, level1Product metadata, IItem item)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri, new System.Net.Mime.ContentType("image/x.geotiff"));
            stacAsset.Properties.AddRange(bandAsset.Properties);
            stacAsset.SetProperty("gsd", GetGroundSampleDistance(metadata, item));
            stacAsset.Properties.Add("sar:polarizations", GetPolarizations(metadata));
            if (GetPixelValueLabel(metadata) != null)
                stacAsset.Roles.Add(GetPixelValueLabel(metadata).ToLower());

            stacItem.Assets.Add(GetProductKey(metadata), stacAsset);
            return GetProductKey(metadata);
        }

        private string GetPixelValueLabel(level1Product metadata)
        {
            switch (metadata.productInfo.productVariantInfo.productVariant)
            {
                case level1ProductProductInfoProductVariantInfoProductVariant.EEC:
                case level1ProductProductInfoProductVariantInfoProductVariant.GEC:
                case level1ProductProductInfoProductVariantInfoProductVariant.MGD:
                    return "amplitude";
                case level1ProductProductInfoProductVariantInfoProductVariant.SSC:
                    return "magnitude";
            }
            return null;
        }

        private string GetProductKey(level1Product metadata)
        {
            if (metadata.instrument.settings[0].polLayer.ToString() != null)
                return "amplitude-" + metadata.instrument.settings[0].polLayer.ToString().ToLower();

            return "unknown";
        }

        private string[] GetPolarizations(level1Product metadata)
        {
            var polarization_list = new List<string>();
            foreach (var setting in metadata.instrument.settings)
            {
                if (!polarization_list.Contains(setting.polLayer.ToString().ToUpper()))
                    polarization_list.Add(setting.polLayer.ToString().ToUpper());
            }
            return polarization_list.ToArray();
        }

        private double GetGroundSampleDistance(level1Product metadata, IItem item)
        {
            XmlNodeList resolution = ReadGeometryFile(item).SelectNodes("/IIF/feature[@key='resolution']");
            return double.Parse(resolution[0].InnerText);
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"T(S|D)X1_SAR.*\.xml$");
            if (manifestAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }

        public virtual async Task<level1Product> ReadMetadata(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await manifestAsset.GetStreamable().GetStreamAsync())
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);
                return (level1Product)metadataSerializer.Deserialize(reader);
            }
        }


        protected KeyValuePair<string, StacAsset> CreateManifestAsset(IStacObject stacObject, IAsset asset)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacObject, asset.Uri, new ContentType("text/xml"), "SAFE Manifest");
            stacAsset.Properties.AddRange(asset.Properties);
            return new KeyValuePair<string, StacAsset>("manifest", stacAsset);
        }
    }
}