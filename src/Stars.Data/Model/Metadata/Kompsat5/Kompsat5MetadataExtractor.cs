// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Kompsat5MetadataExtractor.cs

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
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Kompsat5
{

    [PluginPriority(1000)]
    public class Kompsat5MetadataExtraction : MetadataExtraction
    {

        private const string PLATFORM = "kompsat-5";
        private const string INSTRUMENT = "cosi";
        private const string LEVEL_0 = "L0";
        private const string LEVEL_1A = "L1A";
        private const string LEVEL_1C = "L1C";
        private const string LEVEL_1D = "L1D";

        public override string Label => "Korea Multi-Purpose Satellite-5 (KARI) mission product metadata extractor";

        public Kompsat5MetadataExtraction(ILogger<Kompsat5MetadataExtraction> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(Aux\\.xml)$") ?? throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            logger.LogDebug(string.Format("Metadata file is {0}", auxFile.Uri));

            IStreamResource auxFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(auxFile, System.Threading.CancellationToken.None);
            if (auxFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }

            logger.LogDebug("Deserializing metadata");
            Auxiliary auxiliary = await DeserializeAuxiliary(auxFileStreamable);
            logger.LogDebug("Metadata deserialized. Starting metadata generation");

            var stacIdentifier = string.Format("K5_{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                auxiliary.Root.DownlinkStartUTC.Replace("-", "").Replace(" ", "").Replace(":", "").Replace(".", "_"),
                auxiliary.Root.OrbitNumber,
                auxiliary.Root.OrbitDirection == ORBIT_STATE.ASCENDING ? "A" : "D",
                auxiliary.Root.MultiBeamID.Replace("-", ""),
                GetPolarizations(auxiliary).First(),
                auxiliary.Root.ProductType,
                GetProcessingLevel(auxiliary)
            );
            Match match = Regex.Match(item.Uri.ToString(), @"(.*\/)*(?'identifier'K5_[^\.\/]*)(\.\w+)*(\/.*)*");
            if (match.Success)
                stacIdentifier = match.Groups["identifier"].Value;
            match = Regex.Match(item.Id.ToString(), @"(.*\/)*(?'identifier'K5_[^\.\/]*)(\.\w+)*(\/.*)*");
            if (match.Success)
                stacIdentifier = match.Groups["identifier"].Value;

            StacItem stacItem = new StacItem(stacIdentifier,
                                                GetGeometry(auxiliary),
                                                GetCommonMetadata(auxiliary));

            AddEoStacExtension(auxiliary, stacItem);
            AddSatStacExtension(auxiliary, stacItem);
            AddSarStacExtension(auxiliary, stacItem);
            AddProjStacExtension(auxiliary, stacItem);
            AddViewStacExtension(auxiliary, stacItem);
            AddProcessingStacExtension(auxiliary, stacItem);

            AddOtherProperties(auxiliary, stacItem);

            AddAssets(stacItem, auxiliary, item);

            return StacNode.Create(stacItem, item.Uri); ;

        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();

            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).OrderBy(b => b.Name).ToArray();
        }

        private void AddAssets(StacItem stacItem, Auxiliary auxiliary, IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values)
            {
                AddAsset(stacItem, auxiliary, asset);
            }
        }

        private void AddAsset(StacItem stacItem, Auxiliary auxiliary, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            if (filename.EndsWith("_th.jpg", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("thumbnail", GetGenericAsset(stacItem, asset, "thumbnail"));
            if (filename.EndsWith("_br.jpg", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("browse", GetGenericAsset(stacItem, asset, "overview"));
            if (filename.EndsWith("_br.jgw", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("browse-worldfile", GetGenericAsset(stacItem, asset, "overview-worldfile"));
            if (filename.EndsWith("_Aux.xml", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("metadata", GetGenericAsset(stacItem, asset, "metadata"));
            if (filename.EndsWith("_QL.png", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("overview", GetGenericAsset(stacItem, asset, "overview"));
            if (filename.EndsWith("_QL.pgw", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("overview-worldfile", GetGenericAsset(stacItem, asset, "overview-worldfile"));
            if (filename.EndsWith("_GIM.tif", true, CultureInfo.InvariantCulture))
            {
                stacItem.Assets.Add("gim", GetGenericAsset(stacItem, asset, "data"));
                return;
            }
            if (filename.EndsWith(".tif", true, CultureInfo.InvariantCulture))
            {
                var pols = GetPolarizations(auxiliary);
                var tifkey = "amplitude";
                if (pols != null && pols.Count() > 0) tifkey += "-" + string.Join("-", pols).ToLower();
                var tifasset = GetGenericAsset(stacItem, asset, "data");
                tifasset.Roles.Add("amplitude");
                tifasset.SetProperty("sar:polarizations", pols);
                stacItem.Assets.Add(tifkey, tifasset);
            }

        }

        private StacAsset GetGenericAsset(StacItem stacItem, IAsset asset, string role)
        {
            StacAsset stacAsset = new StacAsset(stacItem, asset.Uri);
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            return stacAsset;
        }

        private void AddOtherProperties(Auxiliary auxiliary, StacItem stacItem)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "KARI",
                    "KOMPSAT-5 has as primary mission objective the development, launch and operation of an Earth observation SAR (Synthetic Aperture Radar) satellite system to provide imagery for geographic information applications and to monitor environmental disasters.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.eoportal.org/satellite-missions/kompsat-5")
                );
            }
        }

        private void AddViewStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);

            var mbi = GetBeamImageObject(auxiliary);
            if (mbi != null)
            {
                if (mbi.NearIncidenceAngle != 0 && mbi.FarIncidenceAngle != 0)
                    view.IncidenceAngle = (mbi.NearIncidenceAngle + mbi.FarIncidenceAngle) / 2;
                else view.IncidenceAngle = mbi.NearIncidenceAngle;
            }

            foreach (var subswath in auxiliary.Root.SubSwaths.SubSwath)
            {
                view.OffNadir += subswath.BeamOffNadirAngle;
            }
            view.OffNadir = view.OffNadir / auxiliary.Root.SubSwaths.SubSwath.Count();
        }

        private void AddSarStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();
            sar.Required(GetInstrumentMode(auxiliary),
                GetFrequencyBand(auxiliary),
                GetPolarizations(auxiliary),
                GetProductType(auxiliary)
            );
            sar.ObservationDirection = ParseObservationDirection(auxiliary.Root.LookSide);

            //Radar Frequency In gigahertz (GHz), for Center Frequency, multiple the value by 1e-9
            if (auxiliary.Root.RadarFrequency != 0) sar.CenterFrequency = auxiliary.Root.RadarFrequency / 1000000000;

            sar.ResolutionRange = auxiliary.Root.GroundRangeGeometricResolution;
            sar.ResolutionAzimuth = auxiliary.Root.AzimuthGeometricResolution;

            switch (auxiliary.Root.AcquisitionMode)
            {
                case ACQUISITION_MODE.HIGH_RESOLUTION:
                case ACQUISITION_MODE.ENHANCED_HIGH_RESOLUTION:
                case ACQUISITION_MODE.ULTRA_HIGH_RESOLUTION:
                    break;
                case ACQUISITION_MODE.STANDARD:
                case ACQUISITION_MODE.ENHANCED_STANDARD:
                    sar.PixelSpacingRange = 1.125;
                    sar.PixelSpacingAzimuth = 1.125;
                    break;
                case ACQUISITION_MODE.WIDE_SWATH:
                    sar.PixelSpacingRange = 12.5;
                    sar.PixelSpacingAzimuth = 12.5;
                    break;
                case ACQUISITION_MODE.ENHANCED_WIDE_SWATH:
                    sar.PixelSpacingRange = 7.5;
                    sar.PixelSpacingAzimuth = 7.5;
                    break;
            }

            sar.LooksRange = auxiliary.Root.RangeProcessingNumberofLooks;
            sar.LooksAzimuth = auxiliary.Root.AzimuthProcessingNumberofLooks;
            var mbi = GetBeamImageObject(auxiliary);
            if (mbi != null)
            {
                sar.LooksEquivalentNumber = mbi.EquivalentNumberofLooks;
            }
        }
        private void AddProcessingStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            ProcessingStacExtension process = stacItem.ProcessingExtension();
            process.Level = GetProcessingLevel(auxiliary);
        }

        private string GetProcessingLevel(Auxiliary auxiliary)
        {
            switch (GetProductType(auxiliary))
            {
                case "SCS":
                    return LEVEL_1A;
                case "GEC":
                case "WEC":
                    return LEVEL_1C;
                case "GTC":
                case "WTC":
                    return LEVEL_1D;
                default:
                    return LEVEL_0;
            }
        }
        private string GetInstrumentMode(Auxiliary auxiliary)
        {
            switch (auxiliary.Root.AcquisitionMode)
            {
                case ACQUISITION_MODE.HIGH_RESOLUTION:
                    return "HR";
                case ACQUISITION_MODE.ENHANCED_HIGH_RESOLUTION:
                    return "EHR";
                case ACQUISITION_MODE.ULTRA_HIGH_RESOLUTION:
                    return "UHR";
                case ACQUISITION_MODE.STANDARD:
                    return "ST";
                case ACQUISITION_MODE.ENHANCED_STANDARD:
                    return "ES";
                case ACQUISITION_MODE.WIDE_SWATH:
                    return "WS";
                case ACQUISITION_MODE.ENHANCED_WIDE_SWATH:
                    return "EWS";
                default:
                    return null;
            }
        }
        private SarCommonFrequencyBandName GetFrequencyBand(Auxiliary auxiliary)
        {
            var wlcm = auxiliary.Root.RadarWavelength * 100; //wavelength in cm       
            if (wlcm > 15 && wlcm <= 30) return SarCommonFrequencyBandName.L;
            if (wlcm > 7.5 && wlcm <= 15) return SarCommonFrequencyBandName.S;
            if (wlcm > 3.8 && wlcm <= 7.5) return SarCommonFrequencyBandName.C;
            if (wlcm > 2.4 && wlcm <= 3.8) return SarCommonFrequencyBandName.X;
            throw new Exception("Invalid RadarWavelength");
        }
        private string[] GetPolarizations(Auxiliary auxiliary)
        {
            var pols = new List<string>();
            foreach (var subswath in auxiliary.Root.SubSwaths.SubSwath)
            {
                if (!pols.Contains(subswath.Polarisation)) pols.Add(subswath.Polarisation);
            }
            return pols.ToArray();
        }
        private string GetProductType(Auxiliary auxiliary)
        {
            return auxiliary.Root.ProductType.Substring(0, auxiliary.Root.ProductType.IndexOf("_"));
        }

        private MBI GetBeamImageObject(Auxiliary auxiliary)
        {
            switch (auxiliary.Root.AcquisitionMode)
            {
                case ACQUISITION_MODE.HIGH_RESOLUTION:
                case ACQUISITION_MODE.ENHANCED_HIGH_RESOLUTION:
                case ACQUISITION_MODE.ULTRA_HIGH_RESOLUTION:
                case ACQUISITION_MODE.STANDARD:
                case ACQUISITION_MODE.ENHANCED_STANDARD:
                    return auxiliary.Root.SubSwaths.SubSwath.First().SBI;
                case ACQUISITION_MODE.WIDE_SWATH:
                case ACQUISITION_MODE.ENHANCED_WIDE_SWATH:
                    return auxiliary.Root.MBI;
                default:
                    return null;
            }
        }

        /*

                "processing:level": "L1D",

        */

        private void AddSatStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = auxiliary.Root.OrbitNumber;

            switch (auxiliary.Root.OrbitDirection)
            {
                case ORBIT_STATE.ASCENDING:
                    sat.OrbitState = "ascending";
                    break;
                case ORBIT_STATE.DESCENDING:
                    sat.OrbitState = "descending";
                    break;
            }
        }

        private void AddEoStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }

        private void AddProjStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();

            switch (auxiliary.Root.ProjectionID)
            {
                case "UNIVERSAL TRANSVERSE MERCATOR":
                    int zone = Math.Abs(auxiliary.Root.MapProjectionZone);
                    bool north = auxiliary.Root.MapProjectionZone < 0;
                    ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
                    proj.SetCoordinateSystem(utm);
                    break;
            }
            proj.Shape = new int[2] { auxiliary.Root.SubSwaths.SubSwath.Sum(s => s.EchoSamplingWindowLength), auxiliary.Root.SubSwaths.SubSwath.Max(s => s.LinesperBurst * s.BurstsperSubswath) };

        }

        private IDictionary<string, object> GetCommonMetadata(Auxiliary auxiliary)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(auxiliary, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(auxiliary, properties);
            FillBasicsProperties(auxiliary, properties);

            return properties;
        }

        private void FillInstrument(Auxiliary auxiliary, Dictionary<string, object> properties)
        {
            // platform
            if (!string.IsNullOrEmpty(PLATFORM))
            {
                properties.Remove("platform");
                properties.Remove("constellation");
                properties.Remove("mission");

                properties.Add("platform", PLATFORM);
                properties.Add("mission", PLATFORM);
            }

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { INSTRUMENT });

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

            double gsd = 0;
            switch (auxiliary.Root.AcquisitionMode)
            {
                case ACQUISITION_MODE.HIGH_RESOLUTION:
                case ACQUISITION_MODE.ENHANCED_HIGH_RESOLUTION:
                    gsd = 1;
                    break;
                case ACQUISITION_MODE.ULTRA_HIGH_RESOLUTION:
                    gsd = 0.85;
                    break;
                case ACQUISITION_MODE.STANDARD:
                    gsd = 3;
                    break;
                case ACQUISITION_MODE.ENHANCED_STANDARD:
                    gsd = 2.5;
                    break;
                case ACQUISITION_MODE.WIDE_SWATH:
                    gsd = 20;
                    break;
                case ACQUISITION_MODE.ENHANCED_WIDE_SWATH:
                    gsd = 5;
                    break;
            }
            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        private void FillDateTimeProperties(Auxiliary auxiliary, Dictionary<string, object> properties)
        {
            var format = "yyyy-MM-ddTHH:mm:ssZ";
            DateTime startDate = DateTime.MinValue;
            DateTime.TryParse(auxiliary.Root.SceneSensingStartUTC, null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            DateTime.TryParse(auxiliary.Root.SceneSensingStopUTC, null, DateTimeStyles.AssumeUniversal, out endDate);

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

            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParse(auxiliary.Root.ProductGenerationUTC, null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate.ToUniversalTime().ToString(format));
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow.ToString(format));
        }

        private void FillBasicsProperties(Auxiliary auxiliary, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3} {4}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                string.Join(",", properties.GetProperty<string[]>("instruments")),
                GetProductType(auxiliary),
                string.Join("/", GetPolarizations(auxiliary)),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture))
            );
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Auxiliary auxiliary)
        {
            var mbi = GetBeamImageObject(auxiliary);
            if (mbi != null)
            {
                var tlgc = mbi.TopLeftGeodeticCoordinates.Split(',');
                var trgc = mbi.TopRightGeodeticCoordinates.Split(',');
                var blgc = mbi.BottomLeftGeodeticCoordinates.Split(',');
                var brgc = mbi.BottomRightGeodeticCoordinates.Split(',');
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                    new GeoJSON.Net.Geometry.Position[5]{
                        new GeoJSON.Net.Geometry.Position(tlgc[0],tlgc[1]),
                        new GeoJSON.Net.Geometry.Position(trgc[0],trgc[1]),
                        new GeoJSON.Net.Geometry.Position(brgc[0],brgc[1]),
                        new GeoJSON.Net.Geometry.Position(blgc[0],blgc[1]),
                        new GeoJSON.Net.Geometry.Position(tlgc[0],tlgc[1])
                    }
                );
                return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
            }

            var p0 = new GeoJSON.Net.Geometry.Position(0, 0);
            return new GeoJSON.Net.Geometry.Polygon(
                new GeoJSON.Net.Geometry.LineString[] {
                    new GeoJSON.Net.Geometry.LineString(
                        new GeoJSON.Net.Geometry.Position[5]{p0,p0,p0,p0,p0}
                    )
                }
            );
        }


        /// <summary>Deserialize Auxiliary from xml to class</summary>
        /// <param name="auxiliaryFile">The <see cref="StreamWrapper"/> instance linked to the metadata file.</param>
        /// <returns>The deserialized metadata object.</returns>
        public static async Task<Auxiliary> DeserializeAuxiliary(IStreamResource auxiliaryFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Auxiliary));
            Auxiliary auxiliary;
            using (var stream = await auxiliaryFile.GetStreamAsync(System.Threading.CancellationToken.None))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (Auxiliary)ser.Deserialize(reader);
                }
            }
            return auxiliary;
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            if (!(route is IItem item)) return false;
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(Aux\\.xml)$");
            try
            {
                var auxiliary = DeserializeAuxiliary(resourceServiceProvider.GetStreamResourceAsync(auxFile, System.Threading.CancellationToken.None).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                return auxiliary.Root.SatelliteID == "KMPS5";
            }
            catch (Exception e)
            {
                return false;
            }
            if (auxFile == null)
            {
                return false;
            }

            return true;
        }


    }

}
