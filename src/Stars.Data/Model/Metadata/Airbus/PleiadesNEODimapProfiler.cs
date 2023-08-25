using System;
using System.Collections.Generic;
using System.Linq;
using Stac;
using Stac.Extensions.Eo;
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
            return GetConstellation();
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get

            //TODO PAN assets ? 
            {   //MS-FS assets
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                bandOrders.Add(EoBandCommonName.blue, 0);
                bandOrders.Add(EoBandCommonName.green, 1);
                bandOrders.Add(EoBandCommonName.red, 2);
                bandOrders.Add(EoBandCommonName.coastal, 3);
                bandOrders.Add(EoBandCommonName.rededge, 4);
                bandOrders.Add(EoBandCommonName.nir, 5);
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
            return new string[1] { "pneo-imager"};
        }
        
        
        
        public override double GetResolution()
        {
            try {
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

        
        
        
        protected override List<EoBandObject> GetEoBandObjects(Schemas.Band_Measurement_List spectralBandInfos, Radiometric_Settings radiometric_Settings)
        {
            List<EoBandObject> eoBandObjects = new List<EoBandObject>();
            for (int i = 0; i < spectralBandInfos.Band_Radiance.Count(); i++)
            {
                var bandInfo = spectralBandInfos.Band_Radiance[i];
                var bandSolarIrradiance = spectralBandInfos.Band_Solar_Irradiance[i];
                

                var fwhm = spectralBandInfos.Band_Spectral_Range.FirstOrDefault(b => b.BAND_ID.Equals(bandInfo.BAND_ID)).FWHM;
                // center_wavelength
                double centerWavelength  = (fwhm.FWHM_MIN + fwhm.FWHM_MAX) / 2/1000;
                // full_width_half_max
                double fullWidthHalfMax = (fwhm.FWHM_MAX - fwhm.FWHM_MIN)/1000;

                EoBandObject eoBandObject = GetEoBandRadianceObject(bandInfo, bandSolarIrradiance);
                eoBandObject.Properties.Add("full_width_half_max", fullWidthHalfMax);
                eoBandObject.CenterWavelength = centerWavelength;
                
                eoBandObjects.Add(eoBandObject);
            }
            return eoBandObjects.OrderBy(eob => BandOrders[eob.CommonName]).ToList();
        }

        // private List<RasterBand> GetRasterBandObjects(Schemas.Band_Measurement_List spectralBandInfos, Radiometric_Settings radiometric_Settings)
        // {
        //     List<RasterBand> rasterBandObjects = new List<RasterBand>();
        //     for (int i = 0; i < spectralBandInfos.Band_Radiance.Count(); i++)
        //     {
        //         var bandInfo = spectralBandInfos.Band_Radiance[i];
        //         var bandSolarIrradiance = spectralBandInfos.Band_Solar_Irradiance[i];
        //         rasterBandObjects.Add(GetRasterBandObject(bandInfo, bandSolarIrradiance, radiometric_Settings));
        //     }
        //     rasterBandObjects = rasterBandObjects.OrderBy(rb => BandOrders[rb.GetProperty<EoBandCommonName>("bcn")]).ToList();
        //     foreach (var rb in rasterBandObjects)
        //         rb.RemoveProperty("bcn");
        //     return rasterBandObjects;
        // }
        
        
    }
}