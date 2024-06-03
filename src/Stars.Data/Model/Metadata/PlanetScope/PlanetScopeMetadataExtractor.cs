using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.PlanetScope
{
    public class PlanetScopeMetadataExtractor : MetadataExtraction
    {
        public override string Label => "Planet Imaging constellation product metadata extractor";

        public PlanetScopeMetadataExtractor(ILogger<PlanetScopeMetadataExtractor> logger,
            IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                PlanetScopeMetadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
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
            PlanetScopeMetadata metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata, item);
            AddAssets(stacItem, item, metadata);

            return StacItemNode.Create(stacItem, item.Uri);
            ;
        }


        internal virtual StacItem CreateStacItem(PlanetScopeMetadata metadata, IItem item)
        {
            string identifier = FindFirstAssetFromFileNameRegex(item, @".*\.tif").Uri.ToString();
            string filename = identifier.Substring(identifier.LastIndexOf('/') + 1).Split('.')[0];

            StacItem stacItem = new StacItem(filename, GetGeometry(metadata), GetCommonMetadata(metadata));

            //stacItem.Properties.Add("iden", filename);
            AddSatStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        private void FillBasicsProperties(PlanetScopeMetadata metadata, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture))
            );
        }

        private void AddSatStacExtension(PlanetScopeMetadata metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.OrbitState = metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/eop:orbitDirection",
                    metadata.nsmgr).Value.ToLower();
            sat.AbsoluteOrbit = int.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:metaDataProperty/ps:EarthObservationMetaData/eop:archivedIn/eop:ArchivingInformation/eop:archivingIdentifier",
                    metadata.nsmgr).Value);
        }


        private void AddViewStacExtension(PlanetScopeMetadata metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.SunElevation = double.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/opt:illuminationElevationAngle",
                    metadata.nsmgr).Value);
            view.SunAzimuth = double.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/opt:illuminationAzimuthAngle",
                    metadata.nsmgr).Value);
            view.IncidenceAngle = double.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/eop:incidenceAngle",
                    metadata.nsmgr).Value);
            view.OffNadir = double.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/ps:spaceCraftViewAngle",
                    metadata.nsmgr).Value);
        }


        private void AddProjStacExtension(PlanetScopeMetadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = long.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:resultOf/ps:EarthObservationResult/eop:product/ps:ProductInformation/ps:spatialReferenceSystem/ps:epsgCode",
                    metadata.nsmgr).Value);
        }

        private void AddProcessingStacExtension(PlanetScopeMetadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:metaDataProperty/ps:EarthObservationMetaData/eop:productType",
                    metadata.nsmgr).Value.ToString();
        }

        private IDictionary<string, object> GetCommonMetadata(PlanetScopeMetadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillPlatformDefinition(metadata, properties);

            return properties;
        }


        private void FillDateTimeProperties(PlanetScopeMetadata metadata, Dictionary<string, object> properties)
        {
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");


            properties.Add("datetime", metadata.nav.SelectSingleNode(
                "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/ps:acquisitionDateTime",
                metadata.nsmgr).Value);


            properties.Add("start_datetime", metadata.nav.SelectSingleNode(
                "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/ps:acquisitionDateTime",
                metadata.nsmgr).Value);

            properties.Add("end_datetime", metadata.nav.SelectSingleNode(
                "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:acquisitionParameters/ps:Acquisition/ps:acquisitionDateTime",
                metadata.nsmgr).Value);
        }

        private void FillPlatformDefinition(PlanetScopeMetadata metadata, Dictionary<string, object> properties)
        {
            properties.Remove("mission");
            properties.Add("mission",
                metadata.nav
                    .SelectSingleNode(
                        "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:Platform/eop:shortName",
                        metadata.nsmgr).Value.ToLower());

            properties.Remove("platform");
            properties.Add("platform",
                metadata.nav
                    .SelectSingleNode(
                        "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:Platform/eop:shortName",
                        metadata.nsmgr).Value.ToLower());

            properties.Remove("instruments");
            properties.Add("instruments",
                new string[]
                {
                    metadata.nav
                        .SelectSingleNode(
                            "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:instrument/eop:Instrument/eop:shortName",
                            metadata.nsmgr).Value.ToLower()
                });

            properties["sensor_type"] = "optical";

            properties.Remove("gsd");
            properties.Add("gsd",
                double.Parse(metadata.nav
                    .SelectSingleNode(
                        "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:sensor/ps:Sensor/eop:resolution",
                        metadata.nsmgr).Value));
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(PlanetScopeMetadata metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]
                {
                    new GeoJSON.Net.Geometry.Position(
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topLeft/ps:latitude",
                                metadata.nsmgr).Value),
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topLeft/ps:longitude",
                                metadata.nsmgr).Value)),

                    new GeoJSON.Net.Geometry.Position(
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topRight/ps:latitude",
                                metadata.nsmgr).Value),
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topRight/ps:longitude",
                                metadata.nsmgr).Value)),

                    new GeoJSON.Net.Geometry.Position(
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomRight/ps:latitude",
                                metadata.nsmgr).Value),
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomRight/ps:longitude",
                                metadata.nsmgr).Value)),

                    new GeoJSON.Net.Geometry.Position(
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomLeft/ps:latitude",
                                metadata.nsmgr).Value),
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomLeft/ps:longitude",
                                metadata.nsmgr).Value)),

                    new GeoJSON.Net.Geometry.Position(
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topLeft/ps:latitude",
                                metadata.nsmgr).Value),
                        double.Parse(metadata.nav
                            .SelectSingleNode(
                                "/ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topLeft/ps:longitude",
                                metadata.nsmgr).Value))
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString })
                .NormalizePolygon();
        }


        private void AddBandAsset(StacItem stacItem, PlanetScopeMetadata metadata, IAsset asset)
        {
            JObject planetScopeBandAux = null;
            using (StreamReader r = new StreamReader("Model/Metadata/PlanetScope/Planetscope_bands.json"))
            {
                string json = r.ReadToEnd();
                planetScopeBandAux = JObject.Parse(json);
            }

            string sensor = metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:instrument/eop:Instrument/eop:shortName",
                    metadata.nsmgr).Value;
            var bandSpecificMetadata =
                metadata.nav.Select(
                    "/ps:EarthObservation/gml:resultOf/ps:EarthObservationResult/ps:bandSpecificMetadata",
                    metadata.nsmgr);
            var numberOfBands = bandSpecificMetadata.Count;

            EoBandCommonName[] bandCommonNames;
            EoBandObject[] bands;
            if (numberOfBands == 3)
            {
                bandCommonNames = new[]
                    { EoBandCommonName.red, EoBandCommonName.green, EoBandCommonName.blue };
                bands = new EoBandObject[numberOfBands];
            }
            else if (numberOfBands == 4)
            {
                bandCommonNames = new[]
                    { EoBandCommonName.blue, EoBandCommonName.green, EoBandCommonName.red, EoBandCommonName.nir };
                bands = new EoBandObject[numberOfBands];
            }
            else if (numberOfBands == 8)
            {
                // we are changing the number of bands to 7 because we are not using the green 1 band
                numberOfBands = 7;
                bandCommonNames = new[] {
                    EoBandCommonName.coastal, EoBandCommonName.blue, EoBandCommonName.green, EoBandCommonName.yellow,
                    EoBandCommonName.red, EoBandCommonName.rededge, EoBandCommonName.nir
                };
                bands = new EoBandObject[numberOfBands];
            }
            else
            {
                throw new InvalidDataException("Sensor not found or not recognized");
            }

            for (int i = 0; i < numberOfBands; i++)
            {
                bands[i] = new EoBandObject(bandCommonNames[i].ToString(), bandCommonNames[i]);
                double centerWaveLength =
                    planetScopeBandAux["planetscope"][sensor][i]["center_wavelength"].Value<double>() / 1000;
                double esun = planetScopeBandAux["planetscope"][sensor][i]["ESUN"].Value<double>();
                bands[i].Description =
                    string.Format("{0} {1}nm TOA", bandCommonNames[i], Math.Round(centerWaveLength * 1000));
                bands[i].CenterWavelength = centerWaveLength;
                bands[i].SolarIllumination = esun;
            }

            StacAsset stacAsset =
                StacAsset.CreateDataAsset(stacItem, asset.Uri, new ContentType("image/tiff; application=geotiff"));
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add("dn");
            List<RasterBand> rasterbands = new List<RasterBand>();
            while (bandSpecificMetadata.MoveNext())
            {
                // band Green I from sensor PSB.SD has to be skipped
                if (sensor == "PSB.SD" &&
                    numberOfBands == 7 &&
                    bandSpecificMetadata.Current.SelectSingleNode("ps:bandNumber", metadata.nsmgr).Value == "3")
                {
                    bandSpecificMetadata.MoveNext();
                }

                RasterBand rasterband = new RasterBand();
                rasterband.Scale = double.Parse(bandSpecificMetadata.Current
                    .SelectSingleNode("ps:reflectanceCoefficient", metadata.nsmgr).Value);
                rasterband.Properties.Add("radiometric_scale",
                    double.Parse(bandSpecificMetadata.Current
                        .SelectSingleNode("ps:radiometricScaleFactor", metadata.nsmgr).Value));
                rasterbands.Add(rasterband);
            }

            var numRows = int.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:resultOf/ps:EarthObservationResult/eop:product/ps:ProductInformation/ps:numRows",
                    metadata.nsmgr).Value.ToString());
            var numColumns = int.Parse(metadata.nav
                .SelectSingleNode(
                    "/ps:EarthObservation/gml:resultOf/ps:EarthObservationResult/eop:product/ps:ProductInformation/ps:numColumns",
                    metadata.nsmgr).Value.ToString());
            stacAsset.ProjectionExtension().Shape = new int[2] { numColumns, numRows };
            stacAsset.RasterExtension().Bands = rasterbands.ToArray();
            stacAsset.EoExtension().Bands = bands;
            stacItem.Assets.Add("data", stacAsset);
        }

        protected void AddAssets(StacItem stacItem, IItem item, PlanetScopeMetadata metadata)
        {
            IAsset previewAsset = FindFirstAssetFromFileNameRegex(item, @".*PT.*\.jpg");
            if (previewAsset != null)
            {
                if (stacItem.Assets.TryAdd("overview",
                        StacAsset.CreateOverviewAsset(stacItem, previewAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(previewAsset.Uri.ToString())))))
                {
                    stacItem.Assets["overview"].Properties.AddRange(previewAsset.Properties);
                }
            }


            IAsset dataAsset = FindFirstAssetFromFileNameRegex(item, @".*\.tif");
            if (dataAsset != null)
            {
                AddBandAsset(stacItem, metadata, dataAsset);
            }

            IAsset metadataAsset = GetMetadataAsset(item);

            if (metadataAsset != null)
            {
                stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                    new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString()))));
                stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);
            }
            else
                throw new FileNotFoundException(string.Format("PlanetScope metadata file not found "));
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @".*Analytic(MS)?.*metadata.*\.xml") ?? throw new FileNotFoundException(string.Format("Unable to find the summary file asset"));
            return manifestAsset;
        }


        public virtual async Task<PlanetScopeMetadata> ReadMetadata(IAsset manifestAsset)
        {
            PlanetScopeMetadata metadata = new PlanetScopeMetadata(manifestAsset);

            await metadata.ReadMetadata(resourceServiceProvider);

            return metadata;
        }


        public class PlanetScopeMetadata
        {
            private IAsset summaryAsset;
            public XmlNamespaceManager nsmgr { get; set; }

            public XPathNavigator nav { get; set; }

            public PlanetScopeMetadata(IAsset summaryAsset)
            {
                this.summaryAsset = summaryAsset;
            }

            public async Task ReadMetadata(IResourceServiceProvider resourceServiceProvider)
            {
                using (var stream =
                       await resourceServiceProvider.GetAssetStreamAsync(summaryAsset,
                           System.Threading.CancellationToken.None))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        XPathDocument doc = new XPathDocument(reader);
                        nav = doc.CreateNavigator();
                        nsmgr = new XmlNamespaceManager(nav.NameTable);
                        nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                        nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                        nsmgr.AddNamespace("opt", "http://earth.esa.int/opt");
                        nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
                        nsmgr.AddNamespace("eop", "http://earth.esa.int/eop");
                        nsmgr.AddNamespace("ps",
                            "http://schemas.planet.com/ps/v1/planet_product_metadata_geocorrected_level");
                    }
                }
            }
        }
    }
}
