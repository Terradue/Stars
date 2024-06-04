// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: BlackSkyGlobalMetadataExtractor.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.BlackSkyGlobal
{
    public class BlackSkyGlobalMetadataExtractor : MetadataExtraction
    {
        public static Dictionary<int, string> PlatformDesignators = new Dictionary<int, string> {
            { 1, "2018-096M" },
            { 2, "2018-099BG" },
            { 3, "2019-037C" },
            { 4, "2019-054E" },
            { 7, "2020-055BP" },
            { 8, "2020-055BQ" },
            { 9, "2021-023G" },
            { 12, "2021-115BA" },
            { 13, "2021-115BB" },
            { 14, "2021-106A" },
            { 15, "2021-106B" },
            { 16, "2021-120B" },
        };

        // Possible identifiers:
        // CSKS4_SCS_B_HI_16_HH_RA_FF_20211016045150_20211016045156
        private readonly Regex _identifierRegex = new Regex(@"(?'id'CSKS(?'i'\d)_(?'pt'RAW_B|SCS_B|SCS_U|DGM_B|GEC_B|GTC_B)_(?'mode'HI|PP|WR|HR|S2)_(?'swath'..)_(?'pol'HH|VV|HV|VH|CO|CH|CV)_(?'look'L|R)(?'dir'A|D)_.._\d{14}_\d{14})");
        private readonly Regex _coordinateRegex = new Regex(@"(?'lat'[^ ]+) (?'lon'[^ ]+)");
        private static readonly Regex H5dumpValueRegex = new Regex(@".*\(0\): *(?'value'.*)");
        private static readonly Regex SatNumberRegex = new Regex(@".+?(?'n'\d+)$");

        public static XmlSerializer MetadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));

        public override string Label => "COSMO SkyMed (ASI) mission product metadata extractor";

        public BlackSkyGlobalMetadataExtractor(ILogger<BlackSkyGlobalMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
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
            AddProcessingStacExtension(stacItem, metadata, item);
            AddEoStacExtension(stacItem, metadata);
            AddOtherProperties(stacItem, metadata);

            return StacNode.Create(stacItem, item.Uri);
        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"BSG.*\.json$") ?? FindFirstAssetFromFileNameRegex(item, @"BS.*\.txt$");
            if (metadataAsset == null)
            {
                throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            }
            return metadataAsset;
        }


        public virtual async Task<Schemas.Metadata> ReadMetadata(IAsset metadataAsset)
        {
            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    if (metadataAsset.Uri.AbsolutePath.EndsWith(".json"))
                    {
                        logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);
                        string metadataStr = reader.ReadToEnd();
                        Schemas.Metadata metadata = JsonConvert.DeserializeObject<Schemas.Metadata>(metadataStr);
                        return metadata;
                    }
                    else   // .txt file
                    {
                        Schemas.Metadata metadata = Schemas.Metadata.FromTextFile(reader);
                        return metadata;
                    }
                }
            }
        }


        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata, IItem item)
        {
            string suffix = string.Empty;
            if (!metadata.id.Contains("ortho"))
            {
                suffix = GetProcessingLevel(metadata, item);
                if (suffix == "ORTHO") suffix = "_ortho";
                else suffix = "_non-ortho";
            }

            string identifier = string.Format("{0}{1}", metadata.id, suffix);
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties, metadata);
            FillInstrument(properties, metadata, item);
            FillBasicsProperties(properties, metadata, item);

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
            IAsset browseAsset = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_browse\.png$", metadata.id));
            if (browseAsset != null)
            {
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, browseAsset.Uri, new ContentType("image/png"), "Browse image"));
                stacItem.Assets["overview"].Properties.AddRange(browseAsset.Properties);
            }

            // RGB TIFF (ortho)
            IAsset imageAsset = metadata.IsFromText && metadata.id.EndsWith("_ortho") ? FindFirstAssetFromFileNameRegex(item, @".*\.tif") : FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_ortho\.tif$", metadata.id));

            if (imageAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"), "RGB image");
                stacItem.Assets.Add("ORTHO_RGB", stacAsset);
                stacAsset.Properties.AddRange(imageAsset.Properties);
                stacAsset.Properties["gsd"] = metadata.gsd;
                stacAsset.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("B1-RED", EoBandCommonName.red) { CenterWavelength = 0.645, FullWidthHalfMax = 0.11 },
                    new EoBandObject("B2-GREEN", EoBandCommonName.green) { CenterWavelength = 0.545, FullWidthHalfMax = 0.09 },
                    new EoBandObject("B3-BLUE", EoBandCommonName.blue) { CenterWavelength = 0.485, FullWidthHalfMax = 0.07 },
                };
                stacAsset.RasterExtension().Bands = new RasterBand[] {
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                };
            }

            // PAN TIFF (ortho)
            IAsset imageAssetPan = metadata.IsFromText && metadata.id.EndsWith("_ortho-pan") ? FindFirstAssetFromFileNameRegex(item, @".*\.tif") : FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_ortho-pan\.tif$", metadata.id));
            if (imageAssetPan != null)
            {
                StacAsset stacAssetPan = StacAsset.CreateDataAsset(stacItem, imageAssetPan.Uri, new ContentType("image/tiff; application=geotiff"), "PAN image");
                stacItem.Assets.Add("ORTHO_PAN", stacAssetPan);
                stacAssetPan.Properties.AddRange(imageAssetPan.Properties);
                stacAssetPan.Properties["gsd"] = metadata.gsd;
                stacAssetPan.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("pan", EoBandCommonName.pan) { CenterWavelength = 0.575, FullWidthHalfMax = 0.25 },
                };
                stacAssetPan.RasterExtension().Bands = new RasterBand[] {
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                };
            }

            // RGB TIFF (non-ortho)
            imageAsset = metadata.IsFromText && metadata.id.EndsWith("_georeferenced") ? FindFirstAssetFromFileNameRegex(item, @".*\.tif") : FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced\.tif$", metadata.id));
            if (imageAsset != null)
            {
                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"), "RGB image");
                stacItem.Assets.Add("GEO_RGB", stacAsset);
                stacAsset.Properties.AddRange(imageAsset.Properties);
                stacAsset.Properties["gsd"] = metadata.gsd;
                stacAsset.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("B1-RED", EoBandCommonName.red) { CenterWavelength = 0.645, FullWidthHalfMax = 0.11 },
                    new EoBandObject("B2-GREEN", EoBandCommonName.green) { CenterWavelength = 0.545, FullWidthHalfMax = 0.09 },
                    new EoBandObject("B3-BLUE", EoBandCommonName.blue) { CenterWavelength = 0.485, FullWidthHalfMax = 0.07 },
                };
                stacAsset.RasterExtension().Bands = new RasterBand[] {
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                };
            }

            // PAN TIFF (non-ortho)
            imageAssetPan = metadata.IsFromText && metadata.id.EndsWith("_georeferenced-pan") ? FindFirstAssetFromFileNameRegex(item, @".*\.tif") : FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced-pan\.tif$", metadata.id));
            if (imageAssetPan != null)
            {
                StacAsset stacAssetPan = StacAsset.CreateDataAsset(stacItem, imageAssetPan.Uri, new ContentType("image/tiff; application=geotiff"), "PAN image");
                stacItem.Assets.Add("GEO_PAN", stacAssetPan);
                stacAssetPan.Properties.AddRange(imageAssetPan.Properties);
                stacAssetPan.Properties["gsd"] = metadata.gsd;
                stacAssetPan.EoExtension().Bands = new EoBandObject[] {
                    new EoBandObject("pan", EoBandCommonName.pan) { CenterWavelength = 0.575, FullWidthHalfMax = 0.25 },
                };
                stacAssetPan.RasterExtension().Bands = new RasterBand[] {
                    new RasterBand() { DataType = Stac.Common.DataType.uint16, BitsPerSample = 12 },
                };
            }

            // Mask TIFF (non-ortho)
            IAsset imageAssetMask = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_mask\.tif$", metadata.id));
            if (imageAssetMask != null)
            {
                StacAsset stacAssetMask = StacAsset.CreateDataAsset(stacItem, imageAssetMask.Uri, new ContentType("image/tiff; application=geotiff"), "Mask file");
                stacItem.Assets.Add("mask", stacAssetMask);
                stacAssetMask.Properties.AddRange(imageAssetMask.Properties);
                stacAssetMask.Properties["gsd"] = metadata.gsd;
            }

            // RGB RPC (non-ortho)
            IAsset rpcAsset = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced_rpc\.txt$", metadata.id));
            if (rpcAsset != null)
            {
                StacAsset stacAssetRpc = StacAsset.CreateMetadataAsset(stacItem, rpcAsset.Uri, new ContentType("text/plain"), "RPC (RGB)");
                stacItem.Assets.Add("georefrenced-rgb-rpc", stacAssetRpc);
                stacAssetRpc.Properties.AddRange(rpcAsset.Properties);
            }

            // PAN RPC (non-ortho)
            rpcAsset = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced-pan_rpc\.txt$", metadata.id));
            if (rpcAsset != null)
            {
                StacAsset stacAssetRpc = StacAsset.CreateMetadataAsset(stacItem, rpcAsset.Uri, new ContentType("text/plain"), "RPC (PAN)");
                stacItem.Assets.Add("georefrenced-pan-rpc", stacAssetRpc);
                stacAssetRpc.Properties.AddRange(rpcAsset.Properties);
            }

            // Mask RPC (non-ortho)
            rpcAsset = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_mask_rpc\.txt$", metadata.id));
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


        private void FillInstrument(Dictionary<string, object> properties, Schemas.Metadata metadata, IItem item)
        {
            // platform & constellation

            properties["agency"] = "BlackSky";
            properties["platform"] = metadata.sensorName.ToLower();
            properties["constellation"] = "blacksky-global";
            properties["mission"] = "blacksky-global";
            properties["instruments"] = new string[] { "spaceview-24" };
            properties["sensor_type"] = "optical";
            properties["spectral_mode"] = GetSpectralMode(metadata, item);

            properties["gsd"] = metadata.gsd;

        }

        private string GetSpectralMode(Schemas.Metadata metadata, IItem item)
        {
            if (metadata.spectralMode != null)
            {
                if (metadata.spectralMode == "PN") return "PAN";
                if (metadata.spectralMode == "MS") return "MS";
                if (metadata.spectralMode.Contains("MS") && metadata.spectralMode.Contains("MS")) return "PAN/MS";
            }

            bool pan = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_ortho-pan\.tif$", metadata.id)) != null || FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced-pan\.tif$", metadata.id)) != null;
            bool ms = FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_ortho\.tif$", metadata.id)) != null || FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced\.tif$", metadata.id)) != null;

            return string.Format("{0}{1}{2}", pan ? "PAN" : string.Empty, pan && ms ? "/" : string.Empty, ms ? "MS" : string.Empty);

        }

        private void FillBasicsProperties(IDictionary<string, object> properties, Schemas.Metadata metadata, IItem item)
        {
            DateTime? acquisitionDate = GetAcquisitionDateTime(metadata);
            string dateStr = (acquisitionDate != null ? string.Format(" {0:yyyy-MM-dd HH:mm:ss}", acquisitionDate.Value.ToUniversalTime()) : string.Empty);
            CultureInfo culture = new CultureInfo("fr-FR");
            string processingLevel = GetProcessingLevel(metadata, item);
            if (processingLevel == null) processingLevel = string.Empty;
            else processingLevel = string.Format(" {0}", processingLevel);
            string spectralMode = GetSpectralMode(metadata, item);
            if (spectralMode == null) spectralMode = string.Empty;
            else spectralMode = string.Format(" {0}", spectralMode.Replace("/", " "));
            properties["title"] = string.Format("BLACKSY {0}{1}{2}",
                metadata.sensorName.ToUpper(),
                processingLevel,
                spectralMode,
                dateStr
            );
        }

        private void AddOtherProperties(StacItem stacItem, Schemas.Metadata metadata)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "BlackSky",
                    "BlackSky Constellation is a commercially owned and operated constellation of 60 high resolution imaging microsatellites developed by BlackSky Global. The constellation aims to provide higher temporal resolution and lower cost Earth imaging, and currently contains 17 operational microsatellites, each with an expected lifetime of 4 years.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.blacksky.com")
                );
            }
        }


        private void AddSatStacExtension(StacItem stacItem, Schemas.Metadata metadata)
        {
            var sat = new SatStacExtension(stacItem);
            if (!string.IsNullOrEmpty(""))
            {
                Match match = SatNumberRegex.Match("");
                if (match != null && int.TryParse(match.Groups["n"].Value, out int n) && PlatformDesignators.ContainsKey(n))
                {
                    sat.PlatformInternationalDesignator = PlatformDesignators[n];
                }
            }

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
            if (metadata.offNadirAngle.HasValue) view.OffNadir = metadata.offNadirAngle.Value;
            if (metadata.sunAzimuth.HasValue) view.SunAzimuth = metadata.sunAzimuth.Value;
            if (metadata.sunElevation.HasValue) view.SunElevation = metadata.sunElevation.Value;
        }


        private void AddProcessingStacExtension(StacItem stacItem, Schemas.Metadata metadata, IItem item)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata, item);
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
            GeoJSON.Net.Geometry.Polygon polygon;
            if (metadata.geometry is GeoJSON.Net.Geometry.Polygon)
            {
                polygon = metadata.geometry as GeoJSON.Net.Geometry.Polygon;
            }
            else
            {
                string s = JsonConvert.SerializeObject(metadata.geometry);
                polygon = JsonConvert.DeserializeObject<GeoJSON.Net.Geometry.Polygon>(s);
            }
            return polygon.NormalizePolygon();
        }

        private string GetProcessingLevel(Schemas.Metadata metadata, IItem item)
        {
            if (metadata.processLevel != null)
            {
                if (metadata.orthorectified.HasValue && metadata.orthorectified.Value) return "ORTHO";
                if (metadata.georeferenced.HasValue && metadata.georeferenced.Value) return "GEO";
                if (metadata.processLevel == "O") return "ORTHO";
                if (metadata.processLevel == "G") return "GEO";
            }
            if (FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_ortho\.tif$", metadata.id)) != null) return "ORTHO";
            if (FindFirstAssetFromFileNameRegex(item, string.Format(@"{0}_georeferenced\.tif$", metadata.id)) != null) return "GEO";
            return null;
        }


    }

}
