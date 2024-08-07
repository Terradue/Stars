﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PleiadesNEODimapProfiler.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class PleiadesNEODimapProfiler : AirbusProfiler
    {
        public PleiadesNEODimapProfiler(Dimap_Document dimap) : base(dimap)
        {
        }

        internal override string GetConstellation()
        {
            return "pleiades-neo";
        }

        internal override string GetMission()
        {
            return "pleiades-neo";
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get
            {
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                if (Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING == "PMS-X")
                {
                    bandOrders.Add(EoBandCommonName.green, 0);
                    bandOrders.Add(EoBandCommonName.red, 1);
                    bandOrders.Add(EoBandCommonName.nir, 2);
                }
                else
                {
                    bandOrders.Add(EoBandCommonName.red, 0);
                    bandOrders.Add(EoBandCommonName.green, 1);
                    bandOrders.Add(EoBandCommonName.blue, 2);
                    bandOrders.Add(EoBandCommonName.nir, 3);
                    bandOrders.Add(EoBandCommonName.rededge, 4);
                    bandOrders.Add(EoBandCommonName.coastal, 5);
                    bandOrders.Add(EoBandCommonName.pan, 6);
                }

                return bandOrders;
            }
        }

        public override string GetPlatformInternationalDesignator()
        {
            //string mission = GetMission().ToLower();
            //switch (mission)
            //{
            //TODO check rational to distinguish the mission vs designator
            //case "pleiades-3":
            //    return "2021-034A";
            //case "pleiades-4":
            return "2021-073E";
            //}
        }

        internal override string GetPlatform()
        {
            return Dimap.Dataset_Sources.Source_Identification.First().Strip_Source.MISSION
                   + Dimap.Dataset_Sources.Source_Identification.First().Strip_Source.MISSION_INDEX;
        }

        internal override StacProvider[] GetStacProviders()
        {
            StacProvider provider1 = new StacProvider("Airbus", new[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider1.Description = "Pléiades Neo is the most advanced optical constellation of Airbus, with two identical 30 cm resolution satellites and optimum reactivity. Pleiades Neo allows users to unleash the potential of geospatial applications and analytics.";
            provider1.Uri = new Uri("https://www.intelligence-airbusds.com/imagery/constellation/pleiades-neo/");

            StacProvider provider2 = new StacProvider("CNES", new[] { StacProviderRole.licensor });

            return new[] { provider1, provider2 };
        }

        public override string[] GetInstruments()
        {
            return new string[1] { "pneo-imager" };
        }



        public override double GetResolution()
        {
            try
            {
                var value1 = double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                        StringComparison.InvariantCultureIgnoreCase))
                    .Ground_Sample_Distance.GSD_ACROSS_TRACK);

                var value2 = double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                        StringComparison.InvariantCultureIgnoreCase))
                    .Ground_Sample_Distance.GSD_ALONG_TRACK);

                // make mean of the two values and round to 2 decimals
                return Math.Round((value1 + value2) / 2, 2);
            }
            catch
            {
                return 0;
            }
        }


        internal override string GetProcessingLevel()
        {
            return Dimap.Processing_Information.Product_Settings.PROCESSING_LEVEL;
        }



        internal override void CompleteAsset(StacAsset stacAsset, StacItem stacItem)
        {
            // FOR MS-FS and PMS-FS spectalProcess 

            // if the TIF is a NED
            // return only the bands NIR, REDEDGE, DEEP BLUE

            // if the TIF is a RGB
            // return only the bands R, G, B
            Data_Files dataFiles = null;
            List<Raster_Index> filterBands = new List<Raster_Index>();
            if (Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING == "PMS-FS" || Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING == "MS-FS")
            {

                // check to what dataFiles the TIF belongs ( NED or RGB )
                dataFiles = Dimap.Raster_Data.Data_Access.Data_Files.FirstOrDefault(dfs => dfs.Data_File.FirstOrDefault(df => df.DATA_FILE_PATH.Href.Equals(System.IO.Path.GetFileName(stacAsset.Uri.ToString()),
                        StringComparison.InvariantCultureIgnoreCase)) != null);

                if (dataFiles != null)
                {
                    filterBands = dataFiles.Raster_Display.Raster_Index_List.Raster_Index;
                }
            }

            List<EoBandObject> eoBandObjects = GetEoBandObjects(stacAsset, Dimap.Radiometric_Data.Radiometric_Calibration.Instrument_Calibration.Band_Measurement_List,
                Dimap.Processing_Information.Product_Settings.Radiometric_Settings, filterBands);
            if (eoBandObjects.Count > 0)
            {
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
            }
            List<RasterBand> rasterBandObjects = GetRasterBandObjects(Dimap.Radiometric_Data.Radiometric_Calibration.Instrument_Calibration.Band_Measurement_List,
                Dimap.Processing_Information.Product_Settings.Radiometric_Settings, filterBands);

            if (rasterBandObjects.Count > 0)
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
            stacAsset.Properties.Add("product_type", Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING);
            switch (Dimap.Processing_Information.Product_Settings.Radiometric_Settings.RADIOMETRIC_PROCESSING)
            {
                case "BASIC":
                case "LINEAR_STRETCH":
                    stacAsset.Roles.Add("dn");
                    break;
                case "REFLECTANCE":
                    stacAsset.Roles.Add("reflectance");
                    break;
                case "DISPLAY":
                case "SEAMLESS":
                    stacAsset.Roles.Add("visual");
                    break;
            }

            try
            {
                stacAsset.ProjectionExtension().Shape = new int[] { int.Parse(Dimap.Raster_Data.Raster_Dimensions.NCOLS), int.Parse(Dimap.Raster_Data.Raster_Dimensions.NROWS) };
            }
            catch
            {
            }
        }




        protected override List<EoBandObject> GetEoBandObjects(StacAsset stacAsset,
            Band_Measurement_List spectralBandInfos, Radiometric_Settings radiometricSettings,
            List<Raster_Index> filterBands = null)
        {

            // FOR MS-FS spectalProcess 

            // if the TIF is a NED
            // return only the bands NIR, REDEDGE, DEEP BLUE

            // if the TIF is a RGB
            // return only the bands R, G, B

            List<EoBandObject> eoBandObjects = new List<EoBandObject>();
            for (int i = 0; i < spectralBandInfos.Band_Radiance.Count(); i++)
            {

                var bandInfo = spectralBandInfos.Band_Radiance[i];
                // check if bandInfo is in filterBands
                if (filterBands.Any() && !filterBands.Any(b => b.BAND_ID.Equals(bandInfo.BAND_ID)))
                    continue;

                var bandSolarIrradiance = spectralBandInfos.Band_Solar_Irradiance[i];

                var fwhm = spectralBandInfos.Band_Spectral_Range.FirstOrDefault(b => b.BAND_ID.Equals(bandInfo.BAND_ID)).FWHM;
                // center_wavelength
                double centerWavelength = (fwhm.FWHM_MIN + fwhm.FWHM_MAX) / 2 / 1000;
                // full_width_half_max
                double fullWidthHalfMax = (fwhm.FWHM_MAX - fwhm.FWHM_MIN) / 1000;

                EoBandObject eoBandObject = GetEoBandRadianceObject(spectralBandInfos, i);
                eoBandObject.Properties.Add("full_width_half_max", fullWidthHalfMax);
                eoBandObject.CenterWavelength = centerWavelength;

                string commonName = eoBandObject.CommonName.ToString();
                // commonName with first letter to upper case
                commonName = eoBandObject.Name = $"{commonName.Substring(0, 1).ToUpper()}{commonName.Substring(1)}";

                string bandNumber = "";
                if (filterBands.Any())
                {
                    bandNumber = $"-B{filterBands.FirstOrDefault(b => b.BAND_ID.Equals(bandInfo.BAND_ID)).BAND_INDEX}";
                }

                // type
                string type = "";
                if (stacAsset.Uri.ToString().Contains("NED"))
                {
                    type = "NED";
                }
                else if (stacAsset.Uri.ToString().Contains("RGB"))
                {
                    type = "RGB";
                }

                eoBandObject.Name = $"{type}{bandNumber} {commonName}";
                eoBandObjects.Add(eoBandObject);
            }
            return eoBandObjects.OrderBy(eob => BandOrders[eob.CommonName]).ToList();
        }


        internal override string GetId()
        {
            List<string> bandModes =
                Dimap.Dataset_Sources.Source_Identification.Select(identification => identification.Strip_Source.BAND_MODE).ToList();
            var bandModesString = string.Join("_", bandModes);
            CultureInfo culture = new CultureInfo("fr-FR");
            var datetime = GetAcquisitionTime().ToUniversalTime().ToString("yyyyMMddHHmmss", culture); ;
            return $"{GetPlatform()}_{datetime}_{bandModesString}_{GetProcessingLevel()}_{Dimap.Product_Information.Delivery_Identification.JOB_ID}_{Dimap.Product_Information.Delivery_Identification.PRODUCT_CODE}";
        }


        public override string GetTitle(IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            return string.Format("{0} {1} {2} {3}",
                GetPlatform().ToUpper(),
                Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING,
                GetProcessingLevel(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture));
        }

    }
}
