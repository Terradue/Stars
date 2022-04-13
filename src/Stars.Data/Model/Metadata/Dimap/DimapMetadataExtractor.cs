using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Geometry.GeoJson;

namespace Terradue.Stars.Data.Model.Metadata.Dimap
{
    public class DimapMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.t_Dimap_Document));

        public override string Label => "Generic DIMAP product metadata extractor";

        public DimapMetadataExtractor(ILogger<DimapMetadataExtractor> logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                Schemas.t_Dimap_Document metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                var dimapProfiler = GetProfiler(metadata);
                return dimapProfiler != null;
            }
            catch
            {
                return false;
            }
        }

        private DimapProfiler GetProfiler(t_Dimap_Document dimap)
        {
            switch (dimap.Metadata_Id.METADATA_PROFILE)
            {
                case "DMCii":
                    return new DMC.DmcDimapProfiler(dimap);
            }
            if (dimap.Dataset_Id != null && dimap.Dataset_Id.DATASET_NAME.StartsWith("vis1", true, CultureInfo.InvariantCulture))
                return new DMC.Vision1DimapProfiler(dimap);
            if (dimap.Dataset_Id != null && dimap.Dataset_Id.DATASET_NAME.StartsWith("ab_", true, CultureInfo.InvariantCulture))
                return new DMC.Alsat1BDimapProfiler(dimap);
            return new GenericDimapProfiler(dimap);
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            Schemas.t_Dimap_Document metadata = await ReadMetadata(metadataAsset);

            DimapProfiler dimapProfiler = GetProfiler(metadata);

            StacItem stacItem = CreateStacItem(dimapProfiler);

            AddAssets(stacItem, item, dimapProfiler);

            // AddEoBandPropertyInItem(stacItem);

            return StacItemNode.Create(stacItem, item.Uri);;
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        internal virtual StacItem CreateStacItem(DimapProfiler dimapProfiler)
        {
            StacItem stacItem = new StacItem(dimapProfiler.Dimap.Dataset_Id.DATASET_NAME, GetGeometry(dimapProfiler), GetCommonMetadata(dimapProfiler));
            AddSatStacExtension(dimapProfiler, stacItem);
            AddProjStacExtension(dimapProfiler, stacItem);
            AddViewStacExtension(dimapProfiler, stacItem);
            AddProcessingStacExtension(dimapProfiler, stacItem);
            AddEoStacExtension(dimapProfiler, stacItem);
            FillBasicsProperties(dimapProfiler, stacItem.Properties);
            return stacItem;
        }

        private void AddEoStacExtension(DimapProfiler dimapProfiler, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }

        private void AddProcessingStacExtension(DimapProfiler dimapProfiler, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            if (!string.IsNullOrEmpty(dimapProfiler.GetProcessingLevel()))
                proc.Level = dimapProfiler.GetProcessingLevel();
            dimapProfiler.AddProcessingSoftware(proc.Software);
        }

        private void AddProjStacExtension(DimapProfiler dimapProfiler, StacItem stacItem)
        {
            var epsg = dimapProfiler.GetProjection();
            if (epsg == 0) return;
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = epsg;
            proj.Shape = dimapProfiler.GetShape();
        }

        private void AddViewStacExtension(DimapProfiler dimapProfiler, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.Azimuth = dimapProfiler.GetViewingAngle();
            view.SunAzimuth = dimapProfiler.GetSunAngle();
            view.SunElevation = dimapProfiler.GetSunElevation();
            view.IncidenceAngle = dimapProfiler.GetIndidenceAngle();
        }

        private void AddSatStacExtension(DimapProfiler dimapProfiler, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            if (dimapProfiler.GetAbsoluteOrbit().HasValue)
                sat.AbsoluteOrbit = dimapProfiler.GetAbsoluteOrbit().Value;
            if (dimapProfiler.GetRelativeOrbit().HasValue)
                sat.RelativeOrbit = dimapProfiler.GetRelativeOrbit().Value;
            sat.OrbitState = dimapProfiler.GetOrbitState();
            if (GetPlatformInternationalDesignator() != null)
                sat.PlatformInternationalDesignator = GetPlatformInternationalDesignator();
        }

        protected virtual string GetPlatformInternationalDesignator()
        {
            return null;
        }

        private IDictionary<string, object> GetCommonMetadata(DimapProfiler dimapProfiler)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(dimapProfiler, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(dimapProfiler, properties);
            FillBasicsProperties(dimapProfiler, properties);

            return properties;
        }

        private void FillInstrument(DimapProfiler dimapProfiler, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties.Remove("platform");
            properties.Add("platform", dimapProfiler.GetPlatform().ToLower());

            properties.Remove("constellation");
            properties.Add("constellation", dimapProfiler.GetConstellation().ToLower());

            properties.Remove("mission");
            properties.Add("mission", dimapProfiler.GetMission().ToLower());

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", dimapProfiler.GetInstruments().Select(i => i.ToLower()).ToArray());

            properties["sensor_type"] = dimapProfiler.GetSensorMode();

            properties.Remove("gsd");
            properties.Add("gsd", dimapProfiler.GetResolution());
        }

        private void FillDateTimeProperties(DimapProfiler dimapProfiler, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = dimapProfiler.GetStartTime();
            DateTime endDate = dimapProfiler.GetEndTime();
            if (startDate.Ticks == 0 && endDate.Ticks == 0)
            {
                startDate = dimapProfiler.GetAcquisitionTime();
                endDate = startDate;
            }

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

            DateTime createdDate = dimapProfiler.GetProcessingTime();

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate);
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillBasicsProperties(DimapProfiler dimapProfiler, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", dimapProfiler.GetTitle(properties));
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(DimapProfiler dimapProfiler)
        {
            List<GeoJSON.Net.Geometry.Position> positions = new List<Position>();
            foreach (var vertex in dimapProfiler.Dimap.Dataset_Frame)
            {
                positions.Add(new GeoJSON.Net.Geometry.Position(
                    vertex.FRAME_LAT.Value, vertex.FRAME_LON.Value
                )
                );
            }
            positions.Add(positions.First());

            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                positions.ToArray()
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
        }

        protected void AddAssets(StacItem stacItem, IItem item, DimapProfiler dimapProfiler)
        {
            foreach (var dataFile in dimapProfiler.Dimap.Data_Access.Data_File)
            {
                IAsset productAsset = FindFirstAssetFromFileNameRegex(item, dataFile.DATA_FILE_PATH.href + "$");
                if (productAsset == null)
                    throw new FileNotFoundException(string.Format("No product found '{0}'", dataFile.DATA_FILE_PATH.href));
                var bandStacAsset = CreateRasterAsset(stacItem, productAsset, dimapProfiler, dataFile);
                if (dimapProfiler.Dimap.Data_Access.DATA_FILE_ORGANISATION == t_DATA_FILE_ORGANISATION.BAND_SEPARATE)
                    dimapProfiler.CompleteAsset(bandStacAsset.Value,
                        new t_Spectral_Band_Info[1] { dimapProfiler.Dimap.Image_Interpretation.FirstOrDefault(sb => sb.BAND_INDEX == dataFile.BAND_INDEX) },
                        dimapProfiler.Dimap.Raster_Encoding);
                else
                    dimapProfiler.CompleteAsset(bandStacAsset.Value, dimapProfiler.Dimap.Image_Interpretation, dimapProfiler.Dimap.Raster_Encoding);
                stacItem.Assets.Add(bandStacAsset.Key, bandStacAsset.Value);
            }

            var metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                        new ContentType("application/xml"), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);
            try
            {
                var overviewAsset = FindFirstAssetFromFileNameRegex(item, dimapProfiler.Dimap.Dataset_Id.DATASET_QL_PATH.href);
                if (overviewAsset != null)
                {
                    if (stacItem.Assets.TryAdd("overview", StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri,
                                new ContentType(MimeTypes.GetMimeType(Path.GetFileName(overviewAsset.Uri.ToString()))))))
                        stacItem.Assets["overview"].Properties.AddRange(overviewAsset.Properties);
                }
            }
            catch { }
            try
            {
                var thumbnailAsset = FindFirstAssetFromFileNameRegex(item, dimapProfiler.Dimap.Dataset_Id.DATASET_TN_PATH.href);
                if (thumbnailAsset != null)
                {
                    stacItem.Assets.Add("thumbnail", StacAsset.CreateThumbnailAsset(stacItem, thumbnailAsset.Uri,
                                new ContentType(MimeTypes.GetMimeType(Path.GetFileName(thumbnailAsset.Uri.ToString())))));
                    stacItem.Assets["thumbnail"].Properties.AddRange(thumbnailAsset.Properties);
                }
            }
            catch{}
        }

        private KeyValuePair<string, StacAsset> CreateRasterAsset(StacItem stacItem, IAsset bandAsset, DimapProfiler dimapProfiler, t_Data_File dataFile)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri, new ContentType(MimeTypes.GetMimeType(Path.GetFileName(bandAsset.Uri.ToString()))));
            stacAsset.Properties.AddRange(bandAsset.Properties);
            stacAsset.Title = dimapProfiler.GetAssetTitle(bandAsset, dataFile);
            return new KeyValuePair<string, StacAsset>(dimapProfiler.GetProductKey(bandAsset, dataFile), stacAsset);
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @".*\.dim$");
            if (manifestAsset == null)
            {
                manifestAsset = FindFirstAssetFromFileNameRegex(item, @"DIM.*\.xml$");
                if (manifestAsset == null)
                    throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }

        public virtual async Task<Schemas.t_Dimap_Document> ReadMetadata(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await manifestAsset.GetStreamable().GetStreamAsync())
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (Schemas.t_Dimap_Document)metadataSerializer.Deserialize(reader);
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
