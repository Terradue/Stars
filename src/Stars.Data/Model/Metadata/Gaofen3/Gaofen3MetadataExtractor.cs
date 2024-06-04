// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Gaofen3MetadataExtractor.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Gaofen3
{
    public class Gaofen3MetadataExtractor : MetadataExtraction
    {
        //private const string GAOFEN3_PLATFORM_NAME = "gf-3";
        private const string GAOFEN3_PLATFORM_NAME = "GAOFEN-3";

        private const string GAOFEN3_DESCENDING_ORBIT_STATE = "descending";

        public override string Label => "Gaofen-3 SAR Satellite (CNSA) mission product metadata extractor";

        public Gaofen3MetadataExtractor(ILogger<Gaofen3MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider) { }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            IAsset metadataFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.meta\\.xml)$");

            if (metadataFile == null)
            {
                return false;
            }

            return true;
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata files in the product package");
            IAsset metadataFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.meta\\.xml)$") ?? throw new FileNotFoundException("Unable to find the metadata file asset");
            logger.LogDebug("Metadata file is {0}", metadataFile.Uri);

            IStreamResource metadataFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(metadataFile, System.Threading.CancellationToken.None);
            if (metadataFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }


            logger.LogDebug("Deserializing metadata files");
            ProductMetadata metadata = await DeserializeProductMetadata(metadataFileStreamable);
            logger.LogDebug("Metadata files deserialized. Starting metadata generation");

            string[] polarizations = GetPolarizations(metadata);

            // retrieving GeometryObject from metadata
            var geometryObject = GetGeometryObjectFromProductMetadata(metadata);

            // retrieving the common metadata properties (i.e. time and instruments)
            var commonMetadata = GetCommonMetadata(metadata, metadata);

            // id
            string stacItemId = Path.GetFileNameWithoutExtension(metadataFile.Uri.OriginalString).Replace(".meta", "");

            // initializing the stac item object
            StacItem stacItem = new StacItem(stacItemId, geometryObject, commonMetadata);

            AddSarStacExtension(metadata, stacItem);
            AddSatStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);

            AddAssets(stacItem, metadata, item);

            FillBasicsProperties(metadata, stacItem.Properties);
            AddOtherProperties(metadata, stacItem);

            return StacNode.Create(stacItem, item.Uri); ;
        }

        private void AddProjStacExtension(ProductMetadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.SetCoordinateSystem(ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84);
            proj.Shape = new int[2] { metadata.Imageinfo.Width, metadata.Imageinfo.Height };
        }

        private void AddSarStacExtension(ProductMetadata metadata, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();
            sar.Required(GetInstrumentMode(metadata),
                GetFrequencyBand(metadata),
                GetPolarizations(metadata),
                GetProductType(metadata)
            );
            sar.ObservationDirection = ParseObservationDirection(metadata.Sensor.LookDirection);
            sar.PixelSpacingRange = double.Parse(metadata.Imageinfo.Widthspace);
            sar.PixelSpacingAzimuth = double.Parse(metadata.Imageinfo.Heightspace);
            sar.LooksRange = double.Parse(metadata.Processinfo.MultilookRange);
            sar.LooksAzimuth = double.Parse(metadata.Processinfo.MultilookAzimuth);
        }

        private string GetProductType(ProductMetadata metadata)
        {
            return metadata.Productinfo.ProductType;
        }

        private string[] GetPolarizations(ProductMetadata metadata)
        {
            switch (metadata.Productinfo.ProductPolar)
            {
                // single polarization
                case "HH":
                case "DH":
                    return new[] { "HH" };
                case "HV":
                    return new[] { "HV" };
                case "VV":
                case "DV":
                    return new[] { "VV" };
                case "VH":
                    return new[] { "VH" };

                //dual polarization
                case "HHVV":
                    return new[] { "HH", "VV" };
                case "VVHH":
                    return new[] { "VV", "HH" };
                case "VVVH":
                    return new[] { "VV", "VH" };
                case "VHVV":
                    return new[] { "VV", "VH" };
                case "HHHV":
                    return new[] { "HH", "HV" };
                case "HVHH":
                    return new[] { "HH", "HV" };

                // quad polarization
                case "HHHVVHVV":
                    return new[] { "HH", "HV", "VH", "VV" };
                case "AHV":
                    return new[] { "HH", "HV", "VH", "VV" };

                default: return new string[] { };
            }
        }

        private SarCommonFrequencyBandName GetFrequencyBand(ProductMetadata metadata)
        {
            return SarCommonFrequencyBandName.C;
        }

        private string GetInstrumentMode(ProductMetadata metadata)
        {
            return metadata.Sensor.ImagingMode;
        }

        private void FillBasicsProperties(ProductMetadata productMetadata,
            IDictionary<string, object> properties)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)));
        }


        private void AddAssets(StacItem stacItem, ProductMetadata productMetadata,
            IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values.OrderBy(a => a.Uri.ToString()))
            {
                AddAsset(stacItem, asset, productMetadata);
            }
        }


        private void AddAsset(StacItem stacItem, IAsset asset,
            ProductMetadata productMetadata)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());


            // metadata 
            if (filename.EndsWith(".meta.xml", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("metadata",
                    GetGenericAsset(stacItem, asset.Uri, new[] { "metadata" }));
                stacItem.Assets["metadata"].Properties.AddRange(asset.Properties);
                return;
            }


            // thumbnail
            Regex rgx = new Regex("[0-9a-zA-Z_-]*(HH|HV|VH|VV|DH|DV)[0-9a-zA-Z_-]*(\\.thumb\\.jpg)$");
            Match match = rgx.Match(filename);
            if (match.Success)
            {
                string polarization;
                if (match.Groups[1].Value.ToUpper().Equals("DH"))
                {
                    polarization = "HH";
                }
                else if (match.Groups[1].Value.ToUpper().Equals("DV"))
                {
                    polarization = "VV";
                }
                else
                {
                    polarization = match.Groups[1].Value;
                }

                var thumbnailAsset = GetGenericAsset(stacItem, asset.Uri, new[] { "thumbnail" });
                thumbnailAsset.SetProperty("sar:polarizations", new[] { polarization });
                stacItem.Assets.Add($"thumbnail-{polarization.ToLower()}", thumbnailAsset);
                stacItem.Assets[$"thumbnail-{polarization.ToLower()}"].Properties.AddRange(asset.Properties);
                return;
            }


            // overview
            rgx = new Regex("[0-9a-zA-Z_-]*(HH|HV|VH|VV|DH|DV)[0-9a-zA-Z_-]*(\\.jpg)$");
            match = rgx.Match(filename);
            if (match.Success)
            {
                string polarization;
                if (match.Groups[1].Value.ToUpper().Equals("DH"))
                {
                    polarization = "HH";
                }
                else if (match.Groups[1].Value.ToUpper().Equals("DV"))
                {
                    polarization = "VV";
                }
                else
                {
                    polarization = match.Groups[1].Value;
                }

                var overviewAsset = GetGenericAsset(stacItem, asset.Uri, new[] { "overview" });
                overviewAsset.SetProperty("sar:polarizations", new[] { polarization });
                if (stacItem.Assets.TryAdd($"overview-{polarization.ToLower()}", overviewAsset))
                {
                    stacItem.Assets[$"overview-{polarization.ToLower()}"].Properties.AddRange(asset.Properties);
                }
                return;
            }

            // Tiff
            rgx = new Regex("[0-9a-zA-Z_-]*(HH|HV|VH|VV|DH|DV)[0-9a-zA-Z_-]*(\\.tiff)$");
            match = rgx.Match(filename);
            if (match.Success)
            {
                string polarization;
                if (match.Groups[1].Value.ToUpper().Equals("DH"))
                {
                    polarization = "HH";
                }
                else if (match.Groups[1].Value.ToUpper().Equals("DV"))
                {
                    polarization = "VV";
                }
                else
                {
                    polarization = match.Groups[1].Value;
                }
                var tiffAsset = GetGenericAsset(stacItem, asset.Uri, new[] { "amplitude", "data" });
                tiffAsset.SetProperty("sar:polarizations", new[] { polarization });
                tiffAsset.SetProperty($"{polarization}:qualify", GetPolarizationQualifyValue(polarization, productMetadata));
                tiffAsset.SetProperty($"{polarization}:calibration_constant", GetPolarizationCalibrationConstant(polarization, productMetadata));
                stacItem.Assets.Add($"amplitude-{polarization.ToLower()}", tiffAsset);
                stacItem.Assets[$"amplitude-{polarization.ToLower()}"].Properties.AddRange(asset.Properties);
                return;
            }

        }

        private double GetPolarizationCalibrationConstant(string polarization, ProductMetadata productMetadata)
        {
            double polarizationCalibrationConstantValue = polarization switch
            {
                "HH" => double.Parse(productMetadata.Processinfo.CalibrationConst.HH),
                "DH" => double.Parse(productMetadata.Processinfo.CalibrationConst.HH),
                "HV" => double.Parse(productMetadata.Processinfo.CalibrationConst.HV),
                "VH" => double.Parse(productMetadata.Processinfo.CalibrationConst.VH),
                "VV" => double.Parse(productMetadata.Processinfo.CalibrationConst.VV),
                "DV" => double.Parse(productMetadata.Processinfo.CalibrationConst.VV),
                _ => 0
            };

            return polarizationCalibrationConstantValue;
        }


        private double GetPolarizationQualifyValue(string polarization, ProductMetadata productMetadata)
        {
            double polarizationQualifyValue = polarization switch
            {
                "HH" => double.Parse(productMetadata.Imageinfo.QualifyValue.HH),
                "DH" => double.Parse(productMetadata.Imageinfo.QualifyValue.HH),
                "HV" => double.Parse(productMetadata.Imageinfo.QualifyValue.HV),
                "VH" => double.Parse(productMetadata.Imageinfo.QualifyValue.VH),
                "VV" => double.Parse(productMetadata.Imageinfo.QualifyValue.VV),
                "DV" => double.Parse(productMetadata.Imageinfo.QualifyValue.VV),
                _ => 0
            };

            return polarizationQualifyValue;
        }
        private StacAsset GetGenericAsset(StacItem stacItem, Uri uri, string[] roles)
        {
            StacAsset stacAsset = new StacAsset(stacItem, uri, roles, null, null);
            stacAsset.MediaType =
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(uri.ToString())));
            return stacAsset;
        }

        private void AddOtherProperties(ProductMetadata productMetadata, StacItem stacItem)
        {
            stacItem.Properties.Add("product_type", productMetadata.Productinfo.ProductType);
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "CNSA",
                    "The main goal of the CHEOS (China High-Resolution Earth Observation System) series is to provide NRT (Near-Real-Time) observations for disaster prevention and relief, climate change monitoring, geographical mapping, environment and resource surveying, and precision agricultural support.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("http://www.cnsa.gov.cn/english/n6465715/n6465716/c6840350/content.html")
                );
            }

        }

        private void AddProcessingStacExtension(ProductMetadata productMetadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = $"L{productMetadata.Productinfo.ProductLevel}";
        }

        private void AddViewStacExtension(ProductMetadata productMetadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = double.Parse(productMetadata.Sensor.WaveParams.Wave.CenterLookAngle);

            double incidenceAngleCenterPixel = (double.Parse(productMetadata.Processinfo.IncidenceAngleNearRange) +
                                                double.Parse(productMetadata.Processinfo.IncidenceAngleNearRange)) /
                                               2.0;
            view.IncidenceAngle = incidenceAngleCenterPixel;
        }

        private void AddSatStacExtension(ProductMetadata productMetadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = int.Parse(productMetadata.OrbitID);
            sat.OrbitState = GAOFEN3_DESCENDING_ORBIT_STATE;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometryObjectFromProductMetadata(
            ProductMetadata productMetadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5] {
                    new GeoJSON.Net.Geometry.Position(productMetadata.Imageinfo.Corner.BottomLeft.Latitude,
                        productMetadata.Imageinfo.Corner.BottomLeft.Longitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.Imageinfo.Corner.BottomRight.Latitude,
                        productMetadata.Imageinfo.Corner.BottomRight.Longitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.Imageinfo.Corner.TopRight.Latitude,
                        productMetadata.Imageinfo.Corner.TopRight.Longitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.Imageinfo.Corner.TopLeft.Latitude,
                        productMetadata.Imageinfo.Corner.TopLeft.Longitude),
                    new GeoJSON.Net.Geometry.Position(productMetadata.Imageinfo.Corner.BottomLeft.Latitude,
                        productMetadata.Imageinfo.Corner.BottomLeft.Longitude)
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new[] { lineString }).NormalizePolygon();
        }

        private IDictionary<string, object> GetCommonMetadata(ProductMetadata productMetadata,
            ProductMetadata pan1ProductMetadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            FillDateTimeProperties(productMetadata, properties);
            FillInstrument(productMetadata, properties);

            return properties;
        }


        public static async Task<ProductMetadata> DeserializeProductMetadata(IStreamResource productMetadataFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(ProductMetadata));
            ProductMetadata auxiliary;
            using (var stream = await productMetadataFile.GetStreamAsync(System.Threading.CancellationToken.None))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (ProductMetadata)ser.Deserialize(reader);
                }
            }

            return auxiliary;
        }


        private void FillDateTimeProperties(ProductMetadata productMetadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;

            string format = "yyyy-MM-dd HH:mm:ss.ffffff";
            DateTime.TryParseExact(productMetadata.Imageinfo.ImagingTime.Start, format, provider,
                DateTimeStyles.AssumeUniversal,
                out var startDate);
            DateTime.TryParseExact(productMetadata.Imageinfo.ImagingTime.End, format, provider,
                DateTimeStyles.AssumeUniversal,
                out var endDate);

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
                properties.Add("datetime", dateInterval.Start.ToUniversalTime());
            }
            else
            {
                properties.Add("datetime", dateInterval.Start.ToUniversalTime());
                properties.Add("start_datetime", dateInterval.Start.ToUniversalTime());
                properties.Add("end_datetime", dateInterval.End.ToUniversalTime());
            }

            format = "yyyy-MM-dd HH:mm:ss";
            DateTime.TryParseExact(productMetadata.Productinfo.ProductGentime, format, provider,
                DateTimeStyles.AssumeUniversal,
                out var createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate.ToUniversalTime());
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillInstrument(ProductMetadata productMetadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            var platformName = GAOFEN3_PLATFORM_NAME.ToLower();
            if (!string.IsNullOrEmpty(platformName))
            {
                properties.Remove("platform");
                properties.Add("platform", platformName);

                properties.Remove("constellation");
                properties.Add("constellation", platformName);

                properties.Remove("mission");
                properties.Add("mission", platformName);
            }

            // instruments
            var instrumentName = productMetadata.Sensor.SensorID.ToUpper();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

            //var gsd1 = double.Parse(productMetadata.ImageGSD);
        }
    }
}
