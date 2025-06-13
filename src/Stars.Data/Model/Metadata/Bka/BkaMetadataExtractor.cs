// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: BkaMetadataExtractor.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
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
using Stac.Extensions.Raster;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;




namespace Terradue.Stars.Data.Model.Metadata.Bka
{
    public class BkaMetadataExtractor : MetadataExtraction
    {
        public override string Label => "BKA (Belarus) mission product metadata extractor";

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(BkaMetadata));

        private readonly IFileSystem _fileSystem;
        private readonly CarrierManager _carrierManager;

        public BkaMetadataExtractor(ILogger<BkaMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider, IFileSystem fileSystem, CarrierManager carrierManager) : base(logger, resourceServiceProvider)
        {
            _fileSystem = fileSystem;
            _carrierManager = carrierManager;
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset[] metadataAssets = GetMetadataAssets(item);
                if (metadataAssets == null || metadataAssets.Length == 0)
                {
                    IAsset topZipAsset = GetTopZipAsset(item);
                    return (topZipAsset != null);
                }
                else
                {
                    BkaMetadata[] metadata = ReadMetadata(metadataAssets).GetAwaiter().GetResult();
                    return metadata != null && metadata.Length > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset topZipAsset = GetTopZipAsset(item);
            List<IAssetsContainer> innerZipAssetContainers = null;

            if (topZipAsset != null)
            {
                ZipArchiveAsset topZipArchiveAsset = new ZipArchiveAsset(topZipAsset, logger, resourceServiceProvider, _fileSystem);
                var tmpDestination = LocalFileDestination.Create(_fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(topZipArchiveAsset.Uri.AbsolutePath)), item, true);
                IAssetsContainer topZipAssets = await topZipArchiveAsset.ExtractToDestinationAsync(tmpDestination, _carrierManager, System.Threading.CancellationToken.None);

                IAsset productZipAsset = GetProductZipAsset(topZipAsset, topZipAssets);
                if (productZipAsset != null)
                {
                    ZipArchiveAsset productZipArchiveAsset = new ZipArchiveAsset(productZipAsset, logger, resourceServiceProvider, _fileSystem);
                    var tmpDestination2 = LocalFileDestination.Create(_fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(productZipArchiveAsset.Uri.AbsolutePath)), item, true);
                    IAssetsContainer productZipAssets = await productZipArchiveAsset.ExtractToDestinationAsync(tmpDestination2, _carrierManager, System.Threading.CancellationToken.None);
                    if (productZipAssets != null)
                    {
                        IEnumerable<IAsset> innerZipAssets = GetInnerZipAssets(productZipAssets);
                        if (innerZipAssets != null)
                        {
                            foreach (IAsset innerZipAsset in innerZipAssets)
                            {
                                ZipArchiveAsset innerZipArchiveAsset = new ZipArchiveAsset(innerZipAsset, logger, resourceServiceProvider, _fileSystem);
                                var tmpDestination3 = LocalFileDestination.Create(_fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(innerZipArchiveAsset.Uri.AbsolutePath)), item, true);
                                IAssetsContainer innerZipAssetContainer = await innerZipArchiveAsset.ExtractToDestinationAsync(tmpDestination3, _carrierManager, System.Threading.CancellationToken.None);
                                if (innerZipAssetContainers == null) innerZipAssetContainers = new List<IAssetsContainer>();
                                innerZipAssetContainers.Add(innerZipAssetContainer);
                            }
                        }
                    }
                }

            }

            IAsset[] metadataAssets = GetMetadataAssets(item, innerZipAssetContainers) ?? throw new Exception("No metadata assets found");
            BkaMetadata[] metadata = await ReadMetadata(metadataAssets);

            StacItem stacItem = CreateStacItem(metadata);

            AddAssets(stacItem, item, innerZipAssetContainers?.ToArray());

            return StacNode.Create(stacItem, item.Uri); ;
        }


        internal virtual StacItem CreateStacItem(BkaMetadata[] metadata)
        {
            bool multipleProducts = metadata.Length > 1;
            string identifier = metadata[0].Processing.Scene01FileName.Replace(".tif", string.Empty);
            if (multipleProducts)
            {
                identifier = identifier.Replace("-MUL-", "-").Replace("-PAN-", "-");
            }

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

        private void AddEoStacExtension(BkaMetadata[] metadata, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }


        private void AddSatStacExtension(BkaMetadata[] metadata, StacItem stacItem)
        {
            SatStacExtension sat = stacItem.SatExtension();
            sat.PlatformInternationalDesignator = "2012-039B";
        }

        private void AddProjStacExtension(BkaMetadata[] metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = metadata[0].GeoReference?.EpsgCode;
            int height = 0, width = 0;
            foreach (BkaMetadata m in metadata)
            {
                if (m.ImageInfo != null && m.ImageInfo.Height != null && m.ImageInfo.Width != null)
                {
                    if (height == 0 || m.ImageInfo.Height.Value < height) height = m.ImageInfo.Height.Value;
                    if (width == 0 || m.ImageInfo.Width.Value < width) width = m.ImageInfo.Width.Value;
                }
            }
            if (height != 0 && width != 0)
            {
                proj.Shape = new int[2] { height, width };
            }
        }

        private void AddProjViewExtension(BkaMetadata[] metadata, StacItem stacItem)
        {
            ViewStacExtension view = stacItem.ViewExtension();
            int viewingAngleCount = 0, sunAzimuthCount = 0, sunElevationCount = 0;
            double viewingAngleSum = 0, sunAzimuthSum = 0, sunElevationSum = 0;
            foreach (BkaMetadata m in metadata)
            {
                if (m.SatelliteData != null && m.SatelliteData.SceneAcquisition != null)
                {
                    BkaSceneAcquisition sceneAcquisition = m.SatelliteData.SceneAcquisition;
                    if (sceneAcquisition.ViewingAngle != null)
                    {
                        viewingAngleCount++;
                        viewingAngleSum += sceneAcquisition.ViewingAngle.Value;
                    }
                    if (sceneAcquisition.SunAzimuth != null)
                    {
                        sunAzimuthCount++;
                        sunAzimuthSum += sceneAcquisition.SunAzimuth.Value;
                    }
                    if (sceneAcquisition.SunElevation != null)
                    {
                        sunElevationCount++;
                        sunElevationSum += sceneAcquisition.SunElevation.Value;
                    }
                }
            }
            //if (viewingAngleCount != 0) view.IncidenceAngle = viewingAngleSum / viewingAngleCount;
            if (sunAzimuthCount != 0) view.SunAzimuth = sunAzimuthSum / sunAzimuthCount;
            if (sunElevationCount != 0) view.SunElevation = sunElevationSum / sunElevationCount;
        }

        private void AddProcessingStacExtension(BkaMetadata[] metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            string processingLevel = GetProcessingLevel(metadata);
            if (processingLevel != null)
            {
                proc.Level = processingLevel;
            }
        }

        private string GetProcessingLevel(BkaMetadata[] metadata)
        {
            if (metadata[0].Processing?.Level != null)
            {
                return string.Format("L1{0}", metadata[0].Processing.Level);
            }
            return null;
        }

        private string GetInstrument(BkaMetadata[] metadata)
        {
            return "MSS";
        }

        private string GetSpectralMode(BkaMetadata[] metadata)
        {
            string spectralMode = null;
            foreach (BkaMetadata m in metadata)
            {
                if (m.Processing?.Instrument != null)
                {
                    if (spectralMode == null) spectralMode = string.Empty;
                    else spectralMode += "/";
                    spectralMode += m.Processing.Instrument;
                }
            }
            return spectralMode;
        }


        private IDictionary<string, object> GetCommonMetadata(BkaMetadata[] metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillPlatformDefinition(metadata, properties);


            return properties;
        }

        private void FillBasicsProperties(BkaMetadata[] metadata, IDictionary<string, object> properties)
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

        private void AddOtherProperties(BkaMetadata[] metadata, StacItem item)
        {
            IDictionary<string, object> properties = item.Properties;
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
            double gsd = 0;
            foreach (BkaMetadata m in metadata)
            {
                BkaSceneGroundResolution groundResolution = m.GeoReference?.SceneGeoposition?.GroundResolution;
                if (groundResolution != null)
                {
                    if (gsd == 0 || groundResolution.PixelSizeNorthing > gsd) gsd = groundResolution.PixelSizeNorthing;
                }
            }
            if (gsd != 0) item.Gsd = gsd;
        }


        private void FillDateTimeProperties(BkaMetadata[] metadata, Dictionary<string, object> properties)
        {
            var format = "yyyy-MM-ddTHH:mm:ss";
            var format2 = "dd.MM.yyyy HH:mm:ss";

            DateTime datetime = DateTime.MaxValue;
            DateTime created = DateTime.MaxValue;

            foreach (BkaMetadata m in metadata)
            {

                if (DateTime.TryParseExact(m.SatelliteData.SceneAcquisition.AcquisitionTime, format, null, DateTimeStyles.AssumeUniversal, out DateTime dt))
                {
                    if (dt.ToUniversalTime() < datetime) datetime = dt.ToUniversalTime();
                    properties["datetime"] = datetime.ToString("O");
                }
                if (DateTime.TryParseExact(m.Production?.CreationDate, format, null, DateTimeStyles.AssumeUniversal, out dt))
                {
                    if (dt.ToUniversalTime() < created)
                    {
                        created = dt.ToUniversalTime();
                        properties["created"] = created.ToString("O");
                    }
                }
                else if (DateTime.TryParseExact(m.Production?.CreationDate, format2, null, DateTimeStyles.AssumeUniversal, out dt))
                {
                    if (dt.ToUniversalTime() < created)
                    {
                        created = dt.ToUniversalTime();
                        properties["created"] = created.ToString("O");
                    }
                }
            }
        }

        private void FillPlatformDefinition(BkaMetadata[] metadata, Dictionary<string, object> properties)
        {
            string platform = "bka";
            if (metadata[0].Processing?.Satellite != null) platform = metadata[0].Processing.Satellite.ToLower();
            string mission = "bka";
            if (metadata[0].Processing?.Mission != null) mission = metadata[0].Processing.Mission.ToLower();
            properties["platform"] = platform;
            properties["constellation"] = platform;
            properties["mission"] = mission;
            properties["instruments"] = new string[] { GetInstrument(metadata).ToLower() };
            properties["sensor_type"] = "optical";
            string spectralMode = GetSpectralMode(metadata);
            if (spectralMode != null) properties["spectral_mode"] = spectralMode;
        }


        private IGeometryObject GetGeometry(BkaMetadata[] metadata)
        {
            BkaGeodeticCoordinates baseCoordinates = metadata[0].GeoReference.SceneGeoposition.GeodeticCoordinates;
            bool falling = baseCoordinates.Corner2NWLat < baseCoordinates.Corner3NELat;   // orbit goes NW -> SE or SE -> NW

            double swLon = 180, swLat = 0, seLon = -180, seLat = 0, neLon = 0, neLat = 0, nwLon = 0, nwLat = 0;
            foreach (BkaMetadata m in metadata)
            {
                if (m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner1SWLon < swLon)
                {
                    swLon = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner1SWLon;
                    swLat = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner1SWLat;
                    nwLon = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner2NWLon;
                    nwLat = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner2NWLat;
                }
                if (m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner4SELon > seLon)
                {
                    seLon = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner4SELon;
                    seLat = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner4SELat;
                    neLon = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner3NELon;
                    neLat = m.GeoReference.SceneGeoposition.GeodeticCoordinates.Corner3NELat;
                }
            }


            List<Position> positions = new List<Position>
            {
                new Position(swLat, swLon),
                new Position(seLat, seLon),
                new Position(neLat, neLon),
                new Position(nwLat, nwLon),
                new Position(swLat, swLon)
            };
            LineString lineString = new LineString(positions.ToArray());
            return new Polygon(new LineString[] { lineString }).NormalizePolygon();
        }


        protected void AddAssets(StacItem stacItem, IItem item, IAssetsContainer[] assetContainers)
        {
            if (assetContainers == null || assetContainers.Length == 0)
            {
                assetContainers = new IAssetsContainer[] { item };
            }
            foreach (IAssetsContainer container in assetContainers)
            {
                IAsset[] metadataAssets = GetMetadataAssets(container, null);
                BkaMetadata[] metadata = ReadMetadata(metadataAssets).GetAwaiter().GetResult();

                if (metadata.Length != 1)
                {
                    throw new Exception("Missing/invalid metadata");
                }
                IAsset metadataAsset = metadataAssets[0];
                BkaMetadata subProductMetadata = metadata[0];

                IAsset imageAsset = FindFirstAssetFromFileNameRegex(container, @".*\.tif");
                if (imageAsset != null)
                {
                    StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("image/tiff; application=geotiff"));
                    stacAsset.Roles.Add("dn");

                    string key = string.Format("{0}_{1}",
                        GetProcessingLevel(metadata),
                        subProductMetadata.Processing?.Instrument
                    );
                    stacAsset.Properties["title"] = key.Replace("_", " ");
                    stacAsset.Properties.AddRange(imageAsset.Properties);

                    BkaSpectral spectral = subProductMetadata.Radiometry.Spectral;
                    if (spectral.BandMS1 != null) AddBandAsset(spectral.BandMS1, stacAsset);
                    if (spectral.BandMS2 != null) AddBandAsset(spectral.BandMS2, stacAsset);
                    if (spectral.BandMS3 != null) AddBandAsset(spectral.BandMS3, stacAsset);
                    if (spectral.BandMS4 != null) AddBandAsset(spectral.BandMS4, stacAsset);
                    if (spectral.BandPAN != null) AddBandAsset(spectral.BandPAN, stacAsset);

                    stacItem.Assets.Add(key, stacAsset);
                }

                string suffix = (assetContainers.Length == 1) ? string.Empty : string.Format("_{0}", subProductMetadata.Processing?.Instrument);
                if (metadataAsset != null)
                {
                    StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString())));
                    stacItem.Assets.Add(string.Format("metadata{0}", suffix), stacAsset);
                    stacAsset.Properties.AddRange(metadataAsset.Properties);
                }

                IAsset overviewAsset = FindFirstAssetFromFileNameRegex(container, @".*_preview\.jpg");
                if (overviewAsset != null)
                {
                    StacAsset stacAsset = StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri, new ContentType(MimeTypes.GetMimeType(overviewAsset.Uri.ToString())));
                    stacAsset.Properties.AddRange(overviewAsset.Properties);
                    stacItem.Assets.Add(string.Format("overview{0}", suffix), stacAsset);
                }
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
                case "PAN":
                    waveLength = 0.66;
                    fullWidthHalfMax = 0.208;
                    commonName = EoBandCommonName.pan;
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


        protected virtual IAsset[] GetMetadataAssets(IAssetsContainer container, IEnumerable<IAssetsContainer> innerAssetContainers = null)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(container, @"^.*_pasp-en\.xml");
            if (metadataAsset != null) return new IAsset[] { metadataAsset };

            if (innerAssetContainers != null)
            {
                List<IAsset> metadataAssets = new List<IAsset>();
                foreach (IAssetsContainer innerAssetContainer in innerAssetContainers)
                {
                    metadataAsset = FindFirstAssetFromFileNameRegex(innerAssetContainer, @"^.*_pasp-en\.xml");
                    if (metadataAsset != null) metadataAssets.Add(metadataAsset);
                }
                if (metadataAssets.Count != 0) return metadataAssets.ToArray();
            }

            return null;
        }


        protected virtual IAsset GetTopZipAsset(IItem item)
        {
            IAsset zipAsset = FindFirstAssetFromFileNameRegex(item, @"^(?!.*S\d[A-Z]_).*\.zip$");
            return zipAsset;
        }

        protected virtual IAsset GetProductZipAsset(IAsset topAsset, IAssetsContainer container)
        {
            if (topAsset.Uri.AbsolutePath.EndsWith("PRODUCT.zip"))
            {
                return topAsset;
            }

            IAsset zipAsset = FindFirstAssetFromFileNameRegex(container, @"PRODUCT\.zip");
            return zipAsset;
        }

        protected virtual IEnumerable<IAsset> GetInnerZipAssets(IAssetsContainer container)
        {
            IEnumerable<IAsset> zipAssets = FindAssetsFromFileNameRegex(container, @"^(?!.*S\d[A-Z]).*\.zip$");
            return zipAssets;
        }

        public virtual async Task<BkaMetadata[]> ReadMetadata(IEnumerable<IAsset> metadataAssets)
        {
            List<BkaMetadata> metadata = new List<BkaMetadata>();

            foreach (IAsset metadataAsset in metadataAssets)
            {
                using (var stream = await resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None))
                {
                    XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };
                    var reader = XmlReader.Create(stream, settings);

                    logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                    metadata.Add((BkaMetadata)metadataSerializer.Deserialize(reader));
                }
            }

            return metadata.ToArray();
        }
    }
}
