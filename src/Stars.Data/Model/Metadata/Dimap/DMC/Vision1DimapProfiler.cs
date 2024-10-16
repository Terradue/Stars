// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Vision1DimapProfiler.cs

using System;
using System.Collections.Generic;
using System.IO;
using Stac;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Dimap.DMC
{
    internal class Vision1DimapProfiler : DmcDimapProfiler
    {

        public Vision1DimapProfiler(DimapDocument dimap) : base(dimap)
        {
        }

        public Vision1DimapProfiler(IEnumerable<DimapDocument> dimaps) : base(dimaps)
        {
        }

        internal override string GetProcessingLevel()
        {
            string[] identifierParts = Dimap.Dataset_Id.DATASET_NAME.Split('_');
            if (identifierParts.Length >= 4) return identifierParts[3];

            if (Dimap.Production?.PRODUCT_TYPE != null) return Dimap.Production.PRODUCT_TYPE;
            return null;
        }

        protected override EoBandObject GetEoBandObject(t_Spectral_Band_Info bandInfo, string description)
        {
            var eoBandObject = base.GetEoBandObject(bandInfo, description);
            ////
            if (bandInfo.BAND_DESCRIPTION.ToUpper() == "PAN")
            {
                eoBandObject.SolarIllumination = 1830.15;
            }
            if (bandInfo.BAND_DESCRIPTION.ToUpper() == "BLUE")
            {
                eoBandObject.SolarIllumination = 2002.25;
                eoBandObject.CenterWavelength = 0.475;
                eoBandObject.FullWidthHalfMax = 0.07;
            }
            if (bandInfo.BAND_DESCRIPTION.ToUpper() == "GREEN")
            {
                eoBandObject.SolarIllumination = 1822.22;
                eoBandObject.CenterWavelength = 0.55;
                eoBandObject.FullWidthHalfMax = 0.08;
            }
            if (bandInfo.BAND_DESCRIPTION.ToUpper() == "RED")
            {
                eoBandObject.SolarIllumination = 1613.83;
                eoBandObject.CenterWavelength = 0.635;
                eoBandObject.FullWidthHalfMax = 0.07;
            }
            if (bandInfo.BAND_DESCRIPTION.ToUpper() == "NIR")
            {
                eoBandObject.SolarIllumination = 948.98;
                eoBandObject.CenterWavelength = 0.835;
                eoBandObject.FullWidthHalfMax = 0.15;
            }
            ////

            return eoBandObject;
        }

        internal override string GetMission()
        {
            return "Vision-1";
        }

        public override string GetSpectralProcessing(DimapDocument dimap = null)
        {
            List<string> spectralProcessings = new List<string>();
            DimapDocument[] dimaps = dimap == null ? Dimaps : new DimapDocument[] { dimap };
            foreach (DimapDocument d in dimaps)
            {
                string[] identifierParts = d.Dataset_Id.DATASET_NAME.Split('_');
                if (identifierParts.Length >= 2) spectralProcessings.Add(identifierParts[1]);
            }
            spectralProcessings.Sort();
            return string.Join(",", spectralProcessings);
        }


        internal override string GetOrbitState()
        {
            return "ascending";
        }

        public override string GetPlatformInternationalDesignator()
        {
            return "2018-071A";
        }

        public override string GetProductKey(IAsset bandAsset, t_Data_File dataFile)
        {
            string[] nameParts = Path.GetFileNameWithoutExtension(dataFile.DATA_FILE_PATH.href).Split('_');
            if (nameParts.Length < 4)
            {
                return "composite";
            }

            string key = string.Format(
                "{0}-{1}",
                nameParts[1] == "PAN" ? "PAN" : "MS",
                nameParts[3]
            );
            if (nameParts[1] != "PAN") key += string.Format("-{0}", nameParts[1]);
            if (nameParts.Length >= 7) key += string.Format("-{0}", nameParts[nameParts.Length - 1]);

            return key;
        }

        /// <summary>
        /// https://directory.eoportal.org/web/eoportal/satellite-missions/u/uk-dmc-2
        /// </summary>
        /// <returns></returns>
        internal override string GetConstellation()
        {
            return "dmc-3";
        }

        internal override string GetSensorMode()
        {
            return "optical";
        }

        internal override string GetAssetPrefix(DimapDocument dimap, IAsset metadataAsset)
        {
            if (dimap != null)
            {
                return Dimaps.Length == 1 ? string.Empty : string.Format("{0}-", dimap.Dataset_Id.DATASET_NAME.Substring(5, 3));
            }
            if (metadataAsset != null)
            {
                return (Dimaps.Length == 1) ? string.Empty : string.Format("{0}-", Path.GetFileName(metadataAsset.Uri.AbsolutePath).Substring(5, 3));
            }
            return string.Empty;
        }

        internal override StacProvider GetStacProvider()
        {
            StacProvider provider = new StacProvider("Airbus DS", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider.Description = "Vision-1 imagery acquired by the SSTL S1-4 satellite provides 0.9m optical images in the panchromatic band and 3.5m in the multispectral bands (NIR, RGB), with a 20.8km swath width";
            provider.Uri = new Uri("https://intelligence.airbus.com/imagery/our-optical-and-radar-satellite-imagery/vision-1/");
            return provider;
        }
    }
}
