using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Iceye
{
    public class IceyeMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));

        public override string Label => "ICEYE SAR X-band microsatellites constellation product metadata extractor";

        public IceyeMetadataExtractor(ILogger<IceyeMetadataExtractor> logger) : base(logger)
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
            catch
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

            return StacItemNode.CreateUnlocatedNode(stacItem);
        }

        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata)
        {
            StacItem stacItem = new StacItem(metadata.product_name, GetGeometry(metadata), GetCommonMetadata(metadata));
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddSarStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        private void AddSarStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();
            sar.Required(metadata.product_type,
                SarCommonFrequencyBandName.X,
                GetPolarizations(metadata),
                metadata.product_level
            );

            sar.ObservationDirection = ParseObservationDirection(metadata.look_side);
        }

        private void AddProcessingStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
            proc.Software.Add("ICEYE", metadata.processor_version);
        }

        private string GetProcessingLevel(Schemas.Metadata metadata)
        {
            if (metadata.product_level == "GRD") return "L1C";
            return null;
        }

        private void AddProjStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = null;
            proj.Wkt2 = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84.WKT;
        }

        private void AddViewStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = metadata.satellite_look_angle;
            view.IncidenceAngle = metadata.incidence_center;
        }

        private void AddSatStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = metadata.orbit_absolute_number;
            sat.RelativeOrbit = metadata.orbit_relative_number;
            sat.OrbitState = metadata.orbit_direction.ToLower();
            sat.PlatformInternationalDesignator = "2019-038D";
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

        private void FillInstrument(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties.Remove("platform");
            properties.Add("platform", metadata.satellite_name.ToLower());

            properties.Remove("constellation");
            properties.Add("constellation", "iceye");

            properties.Remove("mission");
            properties.Add("mission", "iceye");

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { "x-sar" });

            properties.Remove("gsd");
            properties.Add("gsd", 3.0);
        }

        private void FillDateTimeProperties(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = DateTime.MinValue;
            DateTime.TryParse(metadata.acquisition_start_utc, null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            DateTime.TryParse(metadata.acquisition_end_utc, null, DateTimeStyles.AssumeUniversal, out endDate);

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
                properties.Add("datetime", dateInterval.Start);
            }
            else
            {
                properties.Add("datetime", dateInterval.Start);
                properties.Add("start_datetime", dateInterval.Start);
                properties.Add("end_datetime", dateInterval.End);
            }

            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParse(metadata.processing_time, null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate);
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillBasicsProperties(Schemas.Metadata metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3} {4}",
                                                  StylePlatform(properties.GetProperty<string>("platform")),
                                                  metadata.product_type,
                                                  metadata.product_level,
                                                  string.Join("/", GetPolarizations(metadata)),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)));
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.coord_first_near.Split(' ')[2]),
                                                        double.Parse(metadata.coord_first_near.Split(' ')[3])),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.coord_first_far.Split(' ')[2]),
                                                        double.Parse(metadata.coord_first_far.Split(' ')[3])),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.coord_last_far.Split(' ')[2]),
                                                        double.Parse(metadata.coord_last_far.Split(' ')[3])),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.coord_last_near.Split(' ')[2]),
                                                        double.Parse(metadata.coord_last_near.Split(' ')[3])),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.coord_first_near.Split(' ')[2]),
                                                        double.Parse(metadata.coord_first_near.Split(' ')[3])),
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }

        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata)
        {
            IAsset productAsset = FindFirstAssetFromFileNameRegex(item, metadata.product_file + "$");
            if (string.IsNullOrEmpty(metadata.product_file) || productAsset == null)
                productAsset = FindFirstAssetFromFileNameRegex(item, metadata.product_name + @"\.h5$");
            if (productAsset == null)
                throw new FileNotFoundException(string.Format("No product found '{0}'", metadata.product_file));
            AddBandAsset(stacItem, productAsset, metadata);
            var metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                        new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString())), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);
            var overviewAsset = FindFirstAssetFromFileNameRegex(item, @".*QUICKLOOK.*\.png");
            if (overviewAsset != null)
            {
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(overviewAsset.Uri.ToString()))));
                stacItem.Assets["overview"].Properties.AddRange(overviewAsset.Properties);
            }
        }

        private void AddBandAsset(StacItem stacItem, IAsset bandAsset, Schemas.Metadata metadata)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri, new System.Net.Mime.ContentType("image/x.geotiff"));
            stacAsset.Properties.AddRange(bandAsset.Properties);
            stacAsset.SetProperty("gsd", GetGroundSampleDistance(metadata));
            stacAsset.SarExtension().Polarizations = GetPolarizations(metadata);
            if (GetPixelValueLabel(metadata) != null)
                stacAsset.Roles.Add(GetPixelValueLabel(metadata).ToLower());

            stacItem.Assets.Add(GetProductKey(metadata), stacAsset);
        }

        private string GetPixelValueLabel(Schemas.Metadata metadata)
        {
            switch (metadata.product_level)
            {
                case "GRD":
                    return "amplitude";
                case "SLC":
                    return "magnitude";
            }
            return null;
        }

        private string GetProductKey(Schemas.Metadata metadata)
        {
            if (metadata.product_level == "GRD") return "amplitude-" + metadata.polarization.ToLower();
            return "unknown";
        }

        private string[] GetPolarizations(Schemas.Metadata metadata)
        {
            return new string[] { metadata.polarization.ToUpper() };
        }

        private double GetGroundSampleDistance(Schemas.Metadata metadata)
        {
            return GetGroundSampleDistance(metadata.acquisition_mode);
        }

        private double GetGroundSampleDistance(string mode)
        {
            if (mode == "stripmap") return 2.5;
            if (mode == "spotlight") return 0.5;

            return 0;
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"ICEYE.*\.xml$");
            if (manifestAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }

        public virtual async Task<Schemas.Metadata> ReadMetadata(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await manifestAsset.GetStreamable().GetStreamAsync())
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (Schemas.Metadata)metadataSerializer.Deserialize(reader);
            }
        }

        protected void AddManifestAsset(StacItem stacItem, IAsset asset)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacItem, asset.Uri, new ContentType("text/xml"), "SAFE Manifest");
            stacAsset.Properties.AddRange(asset.Properties);
            stacItem.Assets.Add("manifest", stacAsset);
        }
    }
}
