// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PerusatDimapProfiler.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Stac;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class PerusatDimapProfiler : AirbusProfiler
    {
        public PerusatDimapProfiler(Dimap_Document dimap) : base(dimap)
        {
        }

        internal override string GetConstellation()
        {
            return "perusat";
        }

        internal override string GetMission()
        {
            return base.GetMission().Replace("PER", "PeruSAT");
        }

        public override string[] GetSpectralMode()
        {
            string[] possibleModes = new string[] { "P", "MS", "PMS" };
            List<string> presentModes = new List<string>();
            foreach (var dataFiles in Dimap.Raster_Data.Data_Access.Data_Files)
            {
                foreach (var dataFile in dataFiles.Data_File)
                {
                    string[] parts = dataFile.DATA_FILE_PATH.Href.Split(new char[] { '_', '.' });
                    if (parts.Length > 4)
                    {
                        foreach (string m in possibleModes)
                        {
                            if (m == parts[4])
                            {
                                if (!presentModes.Contains(m)) presentModes.Add(m);
                                break;
                            }
                        }
                    }
                }
            }

            return presentModes.ToArray();
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get
            {
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                bandOrders.Add(EoBandCommonName.pan, 0);
                bandOrders.Add(EoBandCommonName.red, 1);
                bandOrders.Add(EoBandCommonName.green, 2);
                bandOrders.Add(EoBandCommonName.blue, 3);
                bandOrders.Add(EoBandCommonName.nir, 4);
                return bandOrders;
            }
        }

        public override string GetPlatformInternationalDesignator()
        {
            string mission = GetMission().ToLower();
            switch (mission)
            {
                case "perusat-1":
                    return "2016-058A";
            }
            return null;
        }

        internal override string GetProductType()
        {
            string pt = GetProcessingLevel();
            string rp = Dimap.Processing_Information.Product_Settings.Radiometric_Settings.RADIOMETRIC_PROCESSING;
            if (!string.IsNullOrEmpty(rp)) pt += string.Format("/{0}", rp);
            return pt;
        }

        internal override string GetProcessingLevel()
        {
            return Dimap.Processing_Information.Product_Settings.PROCESSING_LEVEL;
        }

        public override string GetTitle(IDictionary<string, object> properties)
        {
            string[] spectralMode = GetSpectralMode() ?? (new string[0]);
            CultureInfo culture = new CultureInfo("fr-FR");
            return string.Format("{0} {1} {2} {3}",
                properties.GetProperty<string>("platform").ToUpper(),
                GetProcessingLevel(),
                properties.ContainsKey("spectral_mode") ? string.Join(" ", properties.GetProperty<string[]>("spectral_mode")) : string.Empty,
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)
            );
        }

        public override double GetResolution()
        {
            Located_Geometric_Values gv = Dimap.Geometric_Data.Use_Area.Located_Geometric_Values.FirstOrDefault(l => l.LOCATION_TYPE.Equals("center", StringComparison.InvariantCultureIgnoreCase));
            if (gv == null) return 0;

            if (double.TryParse(gv.Ground_Sample_Distance.GSD_ACROSS_TRACK, out double across) && double.TryParse(gv.Ground_Sample_Distance.GSD_ALONG_TRACK, out double along))
            {
                return (across + along) / 2;
            }
            return 0;
        }


        internal override StacProvider[] GetStacProviders()
        {
            StacProvider provider = new StacProvider("CONIDA, CNOIS, Airbus", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider.Description = "PerúSAT-1 is a very-high-resolution Earth observation satellite system built for the government and Space Agency of Peru. Launched in 2016, this satellite is the first of its kind operated by Peru.";
            provider.Uri = new Uri("https://www.airbus.com/en/space/earth-observation/earth-observation-portfolio/perusat");
            return new[] { provider };
        }


        internal override string GetAssetKey(IAsset bandAsset, Data_File dataFile)
        {
            string key = base.GetAssetKey(bandAsset, dataFile);
            if (key == "P") key = "PAN";
            return key;
        }


        internal override string GetAssetTitle(IAsset bandAsset, Data_File dataFile)
        {
            string title = string.Format("{0} {1}", GetProcessingLevel(), Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING);
            return title;
        }

    }
}
