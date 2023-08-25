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
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;
using Terradue.Stars.Data.Utils;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Services;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    public class AirbusMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer AirbusDimapSerializer = new XmlSerializer(typeof(Schemas.Dimap_Document));

        public override string Label => "Airbus missions (Pleiades, SPOT, VOL) DIMAP based product metadata extractor";

        public AirbusMetadataExtractor(ILogger<AirbusMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IDictionary<string, IAsset> metadataAssets = GetMetadataAssets(item);
                Schemas.Dimap_Document metadata = ReadMetadata(metadataAssets.First().Value).GetAwaiter().GetResult();
                var dimapProfiler = GetProfiler(metadata);
                return dimapProfiler != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal AirbusProfiler GetProfiler(Dimap_Document dimap)
        {
            switch (dimap.Metadata_Identification.METADATA_PROFILE)
            {
                case "VOLUME":
                    return new VolumeDimapProfiler(dimap, this);
                case "PHR_ORTHO":
                case "PHR_SENSOR":
                    return new PleiadesDimapProfiler(dimap);
                case "PNEO_ORTHO":
                    return new PleiadesNEODimapProfiler(dimap);
                case "S6_ORTHO":
                case "S7_ORTHO":
                    return new SpotDimapProfiler(dimap);
            }
            return null;
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            var metadataAssets = GetMetadataAssets(item);
            List<StacItemNode> stacItemNodes = new List<StacItemNode>();

            foreach (var metadataAsset in metadataAssets)
            {
                Dimap_Document metadata = await ReadMetadata(metadataAsset.Value);
                AirbusProfiler dimapProfiler = GetProfiler(metadata);

                if (dimapProfiler is VolumeDimapProfiler)
                {
                    stacItemNodes.AddRange(await (dimapProfiler as VolumeDimapProfiler).GetDatasets(item, suffix));
                }
                else
                {
                    stacItemNodes.Add(ExtractMetadata(item, dimapProfiler, metadataAsset.Value, suffix));
                }
            }

            if (stacItemNodes.Count() == 1 && stacItemNodes.First() is StacItemNode)
            {
                return stacItemNodes.First() as StacItemNode;
            }

            // Merge MS (multispectral) assets into P (pan-chromatic) STAC item
            StacItemNode baseNode = stacItemNodes.FirstOrDefault(n => n.StacItem.Assets.ContainsKey("P-metadata"));
            if (baseNode == null) baseNode = stacItemNodes[0];
            //StacItem mergedStacItem = baseNode.StacItem;

            foreach (StacItemNode n in stacItemNodes.FindAll(n => n != baseNode))
            {
                baseNode.StacItem.Assets.AddRange(n.StacItem.Assets);
            }

            return baseNode;

            /*
            // Below is complicated code to deal with more than one item (involving a StacCatalog) which does not work

            var mergedItems = stacItemNodes.OfType<StacItemNode>().GroupBy<StacItemNode, string, StacItemNode>(item => item.Id,
                (id, items) => new StacItemNode(
                                        StacItemMerger.Merge(items), items.First().Uri
                                        ))
                                        .ToList();

            StacCatalog catalog = new StacCatalog(item.Id, null);

            catalog.Links.Clear();
            catalog.Links.AddRange(new System.Collections.ObjectModel.Collection<StacLink>(
               stacItemNodes.OfType<StacCatalogNode>().Select(catNode =>
               {
                   StacLink link = StacObjectLink.CreateObjectLink(catNode.StacObject, catNode.Uri);
                   link.RelationshipType = "child";
                   return link;
               }).Concat(
               mergedItems.Select(i =>
               {
                   StacLink link = StacObjectLink.CreateObjectLink(i.StacItem, i.Uri);
                   link.RelationshipType = "item";
                   return link;
               }).ToList()).ToList()));

            return StacCatalogNode.Create(catalog, item.Uri);*/
        }

        internal StacItemNode ExtractMetadata(IItem item,
                                                      AirbusProfiler dimapProfiler,
                                                      IAsset metadataAsset,
                                                      string suffix)
        {
            StacItem stacItem = CreateStacItem(dimapProfiler);

            AddAssets(stacItem, item, metadataAsset, dimapProfiler);

            // AddEoBandPropertyInItem(stacItem);

            return StacItemNode.Create(stacItem, item.Uri) as StacItemNode;
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        internal virtual StacItem CreateStacItem(AirbusProfiler dimapProfiler)
        {
            StacItem stacItem = new StacItem(dimapProfiler.Dimap.Dataset_Identification.DATASET_NAME.Text,
                                             GetGeometry(dimapProfiler),
                                             GetCommonMetadata(dimapProfiler));
            AddSatStacExtension(dimapProfiler, stacItem);
            AddProjStacExtension(dimapProfiler, stacItem);
            AddViewStacExtension(dimapProfiler, stacItem);
            AddProcessingStacExtension(dimapProfiler, stacItem);
            AddEoStacExtension(dimapProfiler, stacItem);
            FillBasicsProperties(dimapProfiler, stacItem.Properties);
            AddOtherProperties(dimapProfiler, stacItem.Properties);
            return stacItem;
        }

        private void AddEoStacExtension(AirbusProfiler dimapProfiler, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }

        private void AddProcessingStacExtension(AirbusProfiler dimapProfiler, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            if (!string.IsNullOrEmpty(dimapProfiler.GetProcessingLevel()))
                proc.Level = dimapProfiler.GetProcessingLevel();
            dimapProfiler.AddProcessingSoftware(proc.Software);
        }

        private void AddProjStacExtension(AirbusProfiler dimapProfiler, StacItem stacItem)
        {
            var epsg = dimapProfiler.GetEpsgProjectionCode();
            if (epsg == 0) return;
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = epsg;
            proj.Shape = dimapProfiler.GetShape();
        }

        private void AddViewStacExtension(AirbusProfiler dimapProfiler, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.Azimuth = dimapProfiler.GetViewingAngle();
            view.SunAzimuth = dimapProfiler.GetSunAngle();
            view.SunElevation = dimapProfiler.GetSunElevation();
            view.IncidenceAngle = dimapProfiler.GetIndidenceAngle();
        }

        private void AddSatStacExtension(AirbusProfiler dimapProfiler, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            if (dimapProfiler.GetAbsoluteOrbit().HasValue)
                sat.AbsoluteOrbit = dimapProfiler.GetAbsoluteOrbit().Value;
            if (dimapProfiler.GetRelativeOrbit().HasValue)
                sat.RelativeOrbit = dimapProfiler.GetRelativeOrbit().Value;
            if (dimapProfiler.GetPlatformInternationalDesignator() != null)
                sat.PlatformInternationalDesignator = dimapProfiler.GetPlatformInternationalDesignator();
        }

        private IDictionary<string, object> GetCommonMetadata(AirbusProfiler dimapProfiler)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(dimapProfiler, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(dimapProfiler, properties);
            FillBasicsProperties(dimapProfiler, properties);

            return properties;
        }

        private void FillInstrument(AirbusProfiler dimapProfiler, Dictionary<string, object> properties)
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

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "optical");

            properties.Remove("gsd");
            properties.Add("gsd", dimapProfiler.GetResolution());
        }

        private void FillDateTimeProperties(AirbusProfiler dimapProfiler, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var startDate = dimapProfiler.GetAcquisitionTime();
            var endDate = startDate;

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate);

            // remove previous values
            properties.Remove("datetime");

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

        private void FillBasicsProperties(AirbusProfiler dimapProfiler, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", dimapProfiler.GetTitle(properties));
        }

        private void AddOtherProperties(AirbusProfiler dimapProfiler, IDictionary<string, object> properties)
        {
            if (IncludeProviderProperty)
            {
                StacProvider[] providers = dimapProfiler.GetStacProviders();
                if (!providers.IsNullOrEmpty()) properties.Add("providers", providers);
            }
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(AirbusProfiler dimapProfiler)
        {
            List<GeoJSON.Net.Geometry.Position> positions = new List<Position>();
            foreach (var vertex in dimapProfiler.Dimap.Dataset_Content.Dataset_Extent.Vertex)
            {
                positions.Add(new GeoJSON.Net.Geometry.Position(
                    vertex.LAT, vertex.LON
                )
                );
            }
            positions.Add(positions.First());

            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                positions.ToArray()
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
        }

        protected void AddAssets(StacItem stacItem, IItem item, IAsset metadataAsset, AirbusProfiler dimapProfiler) {

            foreach (var dataFile in dimapProfiler.Dimap.Raster_Data.Data_Access.Data_Files.Data_File) {
                IAsset productAsset = FindFirstAssetFromFileNameRegex(item, dataFile.DATA_FILE_PATH.Href + "$");
                if (productAsset == null)
                    throw new FileNotFoundException(string.Format("No product found '{0}'",
                        dataFile.DATA_FILE_PATH.Href));
                var bandStacAsset = GetBandAsset(productAsset, dimapProfiler, dataFile, stacItem);
                if (Path.GetExtension(bandStacAsset.Value.Uri.ToString())
                    .Equals(".jp2", StringComparison.InvariantCultureIgnoreCase))
                    bandStacAsset.Value.MediaType = new ContentType("image/jp2");
                dimapProfiler.CompleteAsset(bandStacAsset.Value, stacItem);
                stacItem.Assets.Add(bandStacAsset.Key, bandStacAsset.Value);
                var productWorldFileAsset =
                    FindFirstAssetFromFileNameRegex(item, dataFile.DATA_FILE_PATH.Href.Replace("JP2", "J2W") + "$");
                if (productWorldFileAsset != null) {
                    var dataAsset = StacAsset.CreateDataAsset(stacItem, productWorldFileAsset.Uri,
                        new ContentType(MimeTypes.GetMimeType(Path.GetFileName(productWorldFileAsset.Uri.ToString()))));
                    dataAsset.Properties.AddRange(productWorldFileAsset.Properties);
                    stacItem.Assets.Add(bandStacAsset.Key + "-wf", dataAsset);
                    stacItem.Assets[bandStacAsset.Key + "-wf"].Roles.Add("world-file");
                }
            }

            stacItem.Assets.Add(
                "metadata-" + dimapProfiler.Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING,
                StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                    new ContentType("application/xml"), "Metadata file"));
            stacItem.Assets[
                    "metadata-" + dimapProfiler.Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING]
                .Properties.AddRange(metadataAsset.Properties);
            var overviewAsset = FindFirstAssetFromFilePathRegex(item,
                @".*" + dimapProfiler.Dimap.Dataset_Identification.DATASET_QL_PATH?.Href);
            if (overviewAsset != null) {
                stacItem.Assets.Add(
                    "overview-" + dimapProfiler.Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING,
                    StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri,
                        new ContentType(MimeTypes.GetMimeType(Path.GetFileName(overviewAsset.Uri.ToString())))));
                stacItem.Assets[
                        "overview-" + dimapProfiler.Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING]
                    .Properties.AddRange(overviewAsset.Properties);
            }

            if (dimapProfiler.Dimap.Dataset_Identification.DATASET_TN_PATH != null) {
                var thumbnailAsset = FindFirstAssetFromFilePathRegex(item,
                    @".*" + dimapProfiler.Dimap.Dataset_Identification.DATASET_TN_PATH?.Href);
                if (thumbnailAsset != null) {
                    stacItem.Assets.Add(
                        "thumbnail-" + dimapProfiler.Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING,
                        StacAsset.CreateThumbnailAsset(stacItem, thumbnailAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(Path.GetFileName(thumbnailAsset.Uri.ToString())))));
                    stacItem.Assets[
                            "thumbnail-" + dimapProfiler.Dimap.Processing_Information.Product_Settings
                                .SPECTRAL_PROCESSING]
                        .Properties.AddRange(thumbnailAsset.Properties);
                }

            }
        }

        private KeyValuePair<string, StacAsset> GetBandAsset(IAsset bandAsset, AirbusProfiler dimapProfiler, Data_File dataFile, StacItem stacItem)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri, new ContentType(MimeTypes.GetMimeType(Path.GetFileName(bandAsset.Uri.ToString()))));
            stacAsset.Title = dimapProfiler.GetAssetTitle(bandAsset, dataFile);
            stacAsset.Properties.AddRange(bandAsset.Properties);
            return new KeyValuePair<string, StacAsset>(dimapProfiler.GetAssetKey(bandAsset, dataFile), stacAsset);
        }

        protected virtual IDictionary<string, IAsset> GetMetadataAssets(IItem item, bool volume = true)
        {
            IDictionary<string, IAsset> manifestAsset = FindAllAssetsFromFileNameRegex(item, @"VOL_PHR.XML$");
            if (manifestAsset == null || manifestAsset.Count() == 0)
            {
                manifestAsset = FindAllAssetsFromFileNameRegex(item, @"SPOT_VOL.XML$");
            }
            if (manifestAsset == null || manifestAsset.Count() == 0 || !volume)
            {
                manifestAsset = FindAllAssetsFromFileNameRegex(item, @"^DIM.*\.XML$");
                if (manifestAsset == null)
                    throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }

        public virtual async Task<Schemas.Dimap_Document> ReadMetadata(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(manifestAsset, System.Threading.CancellationToken.None))
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (Schemas.Dimap_Document)AirbusDimapSerializer.Deserialize(reader);
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