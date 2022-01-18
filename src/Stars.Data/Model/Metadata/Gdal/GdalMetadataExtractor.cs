using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using MaxRev.Gdal.Core;
using Microsoft.Extensions.Logging;
using OSGeo.GDAL;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Gdal
{
    [PluginPriority(10000)]
    public class GdalMetadataExtractor : MetadataExtraction
    {
        private readonly string GDALFILE_REGEX = @".*\.(tif|tiff)$";

        public override string Label => "GDAL metadata extractor";

        public GdalMetadataExtractor(ILogger<GdalMetadataExtractor> logger) : base(logger)
        {
            GdalBase.ConfigureAll();
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                var gdalAsset = GetGdalAsset(item);
                OSGeo.GDAL.Dataset dataset = LoadGdalAsset(gdalAsset).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            KeyValuePair<string, IAsset> gdalAsset = GetGdalAsset(item);
            Dataset dataset = await LoadGdalAsset(gdalAsset);

            StacItem stacItem = CreateStacItem(gdalAsset, dataset, item);

            AddAssets(stacItem, gdalAsset, dataset);

            return StacItemNode.Create(stacItem, item.Uri); ;
        }

        internal virtual StacItem CreateStacItem(KeyValuePair<string, IAsset> gdalAsset, Dataset dataset, IItem item)
        {
            StacItem stacItem = new StacItem(item.Id, GetGeometry(dataset));
            FillCommonMetadata(stacItem, dataset, item);
            // AddSatStacExtension(metadata, stacItem);
            // AddSarStacExtension(metadata, stacItem);
            // AddProjStacExtension(metadata, stacItem);
            // AddViewStacExtension(metadata, stacItem);
            // AddProcessingStacExtension(metadata, stacItem);
            //FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        // private void AddSatStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var sat = new SatStacExtension(stacItem);
        //     sat.AbsoluteOrbit = metadata.GetInt32("Ext_OrbitNumber");
        //     sat.RelativeOrbit = sat.AbsoluteOrbit;   // TODO remove?
        //     sat.OrbitState = metadata.GetString("Ext_OrbitDirection").ToLower();
        // }

        // private void AddSarStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     SarStacExtension sar = stacItem.SarExtension();
        //     sar.Required(metadata.GetString("Ext_ObservationMode"),
        //         SarCommonFrequencyBandName.L,
        //         metadata.GetString("Ext_Polarizations").Split('/'),
        //         metadata.GetString("Ext_ObservationMode")
        //     );

        //     sar.ObservationDirection = ParseObservationDirection(metadata.GetString("Ext_ObservationDirection"));
        // }

        // private void AddProjStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     ProjectionStacExtension proj = stacItem.ProjectionExtension();
        //     try
        //     {
        //         int utmZone = metadata.GetInt32("Pds_UTM_ZoneNo");
        //         bool north = metadata.GetString("Pds_MapDirection").Contains("MapNorth");
        //         ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, north);
        //         proj.SetCoordinateSystem(utm);
        //     }
        //     catch { }
        //     try
        //     {
        //         stacItem.ProjectionExtension().Shape = new int[2] { metadata.GetInt32("Pdi_NoOfPixels_0"), metadata.GetInt32("Pdi_NoOfLines_0") };
        //     }
        //     catch { }
        // }

        // private void AddViewStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var view = new ViewStacExtension(stacItem);
        //     view.OffNadir = metadata.GetDouble("Img_OffNadirAngle");
        // }

        // private void AddProcessingStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var proc = stacItem.ProcessingExtension();
        //     // proc.Level = GetProcessingLevel(metadata);
        // }

        private void FillCommonMetadata(StacItem stacItem, Dataset dataset, IItem item)
        {
            FillDateTimeProperties(stacItem, dataset, item);
            // TODO Licensing
            // TODO Provider
            FillInstrument(stacItem, dataset, item);
        }

        private void FillDateTimeProperties(StacItem stacItem, Dataset dataset, IItem item)
        {
            stacItem.DateTime = GetDateTime(dataset, item);

            DateTime? createdDate = GetCreatedDateTime(dataset, item);
            if (createdDate.HasValue)
                stacItem.Created = createdDate.Value;
        }

        private Itenso.TimePeriod.ITimePeriod GetDateTime(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("DateTimeOriginal", "EXIF")))
            {
                try
                {
                    return new Itenso.TimePeriod.TimeInterval(DateTime.Parse(dataset.GetMetadataItem("DateTimeOriginal", "EXIF"), null, DateTimeStyles.AssumeUniversal));
                }
                catch { }
            }
            // if (item is StacItem)
                return item.DateTime;

            // return new Itenso.TimePeriod.TimeInterval(FileInfo(dataset.GetFileList());
        }

        private DateTime? GetCreatedDateTime(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("DateTime", "EXIF")))
            {
                try
                {
                    return DateTime.Parse(dataset.GetMetadataItem("DateTime", "EXIF"), null, DateTimeStyles.AssumeUniversal);
                }
                catch { }
            }
            if (item is StacItem && (item as StacItem).Created.Ticks > 0)
                return (item as StacItem).Created;

            return null;
        }

        private void FillInstrument(StacItem stacItem, Dataset dataset, IItem item)
        {
            // platform & constellation
            var ins = GetInstrument(dataset, item);
            if (ins != null)
                stacItem.Instruments = ins;
        }

        private IEnumerable<string> GetInstrument(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("CameraLabel", "EXIF")))
            {
                try
                {
                    return new string[1] { dataset.GetMetadataItem("CameraLabel", "EXIF") };
                }
                catch { }
            }
            if (item is StacItem && (item as StacItem).Instruments != null)
                return (item as StacItem).Instruments;

            return null;
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Dataset dataset)
        {
            var box = GetRasterExtent(dataset);

            if (box != null && !(box.MinX == 0 && box.MinY == 0))
            {
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                    new GeoJSON.Net.Geometry.Position[] {
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MinX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MinY,
                            box.MinX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MinY,
                            box.MaxX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MaxX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MinX
                        )
                    });
                return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
            }
            return null;


        }

        public OSGeo.OGR.Envelope GetRasterExtent(Dataset dataset, string proj4 = "+proj=latlong +datum=WGS84 +no_defs")
        {

            OSGeo.OSR.SpatialReference srcSRS = new OSGeo.OSR.SpatialReference(dataset.GetProjection());
            OSGeo.OGR.Envelope extent;

            if (dataset.RasterCount == 0)
                return null;

            extent = GetBaseRasterExtent(dataset);

            if (string.IsNullOrEmpty(dataset.GetProjection()))
            {
                srcSRS = new OSGeo.OSR.SpatialReference("");
                srcSRS.ImportFromProj4(proj4);
            }

            if (srcSRS.__str__().Contains("AUTHORITY[\"EPSG\",\"3857\"]") || srcSRS.__str__().Contains("LOCAL_CS[\"WGS 84 / Pseudo-Mercator\""))
            {
                srcSRS.ImportFromProj4("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs");
            }

            OSGeo.OSR.SpatialReference dstSRS = new OSGeo.OSR.SpatialReference("");
            dstSRS.ImportFromProj4(proj4);

            if (dstSRS.IsSame(srcSRS, null) == 1)
            {
                return extent;
            }

            OSGeo.OSR.CoordinateTransformation ct;

            ct = new OSGeo.OSR.CoordinateTransformation(srcSRS, dstSRS);

            double[] newXs = new double[] { extent.MaxX, extent.MinX };
            double[] newYs = new double[] { extent.MaxY, extent.MinY };

            ct.TransformPoints(2, newXs, newYs, new double[] { 0, 0 });

            extent.MaxX = newXs[0];
            extent.MinX = newXs[1];
            extent.MaxY = newYs[0];
            extent.MinY = newYs[1];

            return extent;
        }

        public OSGeo.OGR.Envelope GetBaseRasterExtent(Dataset dataset)
        {

            if (dataset.RasterCount > 0)
            {
                OSGeo.OGR.Envelope extent = new OSGeo.OGR.Envelope();
                double[] geoTransform = new double[6];
                dataset.GetGeoTransform(geoTransform);

                extent.MinX = geoTransform[0];
                extent.MinY = geoTransform[3];
                extent.MaxX = geoTransform[0] + (dataset.RasterXSize * geoTransform[1]) + (geoTransform[2] * dataset.RasterYSize);
                extent.MaxY = geoTransform[3] + (dataset.RasterYSize * geoTransform[5]) + (geoTransform[4] * dataset.RasterXSize); ;

                return extent;
            }

            return null;

        }

        protected void AddAssets(StacItem stacItem, KeyValuePair<string, IAsset> gdalAsset, Dataset dataset)
        {
            string key = gdalAsset.Key;
            int i = 0;
            foreach (var file in dataset.GetFileList())
            {
                StacAsset dataStacAsset = StacAsset.CreateDataAsset(stacItem, new Uri(gdalAsset.Value.Uri, file), new ContentType(MimeTypes.GetMimeType(file)));
                string relpath = gdalAsset.Value.Uri.MakeRelativeUri(dataStacAsset.Uri).ToString();
                dataStacAsset.Properties.Add("filename", string.IsNullOrEmpty(relpath) ? Path.GetFileName(file) : relpath);
                if (i > 0)
                    key += "_" + Path.GetExtension(file);
                stacItem.Assets.Add(key, dataStacAsset);
                i++;
            }
        }

        protected virtual KeyValuePair<string, IAsset> GetGdalAsset(IItem item)
        {
            var gdalAsset = FindFirstKeyAssetFromFileNameRegex(item, GDALFILE_REGEX);
            if (gdalAsset.Key == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the summary file asset"));
            }
            return gdalAsset;
        }

        public virtual async Task<OSGeo.GDAL.Dataset> LoadGdalAsset(KeyValuePair<string, IAsset> gdalAsset)
        {
            OSGeo.GDAL.Dataset dataset = OSGeo.GDAL.Gdal.Open(GetGdalPath(gdalAsset.Value), Access.GA_ReadOnly);

            dataset.GetDriver();

            return dataset;
        }

        private static string GetGdalPath(IAsset gdalAsset)
        {
            switch (gdalAsset.Uri.Scheme)
            {
                case "file":
                    return gdalAsset.Uri.LocalPath;
            }

            return gdalAsset.Uri.ToString();
        }
    }

}
