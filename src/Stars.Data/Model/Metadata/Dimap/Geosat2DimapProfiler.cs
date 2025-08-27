// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GenericDimapProfiler.cs

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Raster;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Dimap
{
    internal class Geosat2DimapProfiler : GenericDimapProfiler
    {

        public Geosat2DimapProfiler(DimapDocument dimap) : base(dimap)
        {
            Dimap = dimap;
        }

        public Geosat2DimapProfiler(IEnumerable<DimapDocument> dimaps): base(dimaps)
        {
            Dimaps = dimaps.ToList().ToArray();
        }

        public override string GetProductKey(IAsset bandAsset, t_Data_File dataFile)
        {
            string dataFileSpectralProcessing = dataFile?.DATA_FILE_PATH?.href.Split('_')[1];
            if (Int32.TryParse(Dimap.Raster_Dimensions.NBANDS, out int bands) && bands > 1)
            {
                return String.Format("MS-{0}-{1}", GetProcessingLevel(), dataFileSpectralProcessing);
            }
            else
            {
                return String.Format("{0}-{1}", GetProcessingLevel(), dataFileSpectralProcessing);
            }
        }

        internal override string GetAssetPrefix(Schemas.DimapDocument dimap, IAsset metadataAsset = null)
        {
            if (dimap != null)
            {
                return Dimaps.Length == 1 ? string.Empty : string.Format("{0}-", dimap.Dataset_Id.DATASET_NAME.Substring(5, 3));
            }
            if (metadataAsset != null)
            {
                return (Dimaps.Length == 1) ? string.Empty : string.Format("{0}-", Path.GetFileName(metadataAsset.Uri.AbsolutePath).Split('_')[1]);
            }
            return string.Empty;
        }


        protected override EoBandObject GetEoBandObject(t_Spectral_Band_Info bandInfo, string description)
        {
            string unit = bandInfo.PHYSICAL_UNIT.Replace(" ", "*").Replace("&micro;", "µ");
            unit = Regex.Replace(unit, "<sup>([^<]+)</sup>", "^$1");
            string name = String.Format("BAND-{0} {1} in {2}",
                bandInfo.BAND_INDEX.Value,
                bandInfo.BAND_DESCRIPTION,
                unit
            );
            EoBandObject eoBandObject = new EoBandObject(name,
                                                (EoBandCommonName)Enum.Parse(typeof(EoBandCommonName), bandInfo.BAND_DESCRIPTION.ToLower()));
            string bandDescription = bandInfo.BAND_DESCRIPTION;
            switch (bandDescription.ToLower())
            {
                case "red":
                case "green":
                case "blue":
                    bandDescription = bandDescription.ToLower().Titleize();
                    break;
            }

            double centerWavelength = 0;
            double fullWidthHalfMax = 0;
            switch (bandDescription.ToLower())
            {
                case "nir":
                    centerWavelength = 0.831;
                    fullWidthHalfMax = 0.12;
                    break;
                case "red":
                    centerWavelength = 0.669;
                    fullWidthHalfMax = 0.06;
                    break;
                case "green":
                    centerWavelength = 0.566;
                    fullWidthHalfMax = 0.07;
                    break;
                case "blue":
                    centerWavelength = 0.496;
                    fullWidthHalfMax = 0.06;
                    break;
                case "pan":
                    centerWavelength = 0.730;
                    fullWidthHalfMax = 0.34;
                    break;
            }

            if (centerWavelength != 0)
            {
                eoBandObject.CenterWavelength = centerWavelength;
                eoBandObject.FullWidthHalfMax = fullWidthHalfMax;
            }
            if (bandInfo.ESUN != null)
            {
                eoBandObject.SolarIllumination = bandInfo.ESUN;
            }

            eoBandObject.Description = string.Format("{0} {1} in {2}", bandDescription, description, unit);
            return eoBandObject;

        }

        protected override RasterBand GetRasterBandObject(t_Spectral_Band_Info bandInfo, t_Raster_Encoding rasterEncoding)
        {
            RasterBand rasterBandObject = new RasterBand();
            if (rasterEncoding.DATA_TYPESpecified)
            {
                switch (rasterEncoding.DATA_TYPE)
                {
                    case (t_DATA_TYPE.BYTE):
                        rasterBandObject.DataType = Stac.Common.DataType.uint8;
                        break;
                    case (t_DATA_TYPE.DOUBLE):
                        rasterBandObject.DataType = Stac.Common.DataType.float64;
                        break;
                    case (t_DATA_TYPE.FLOAT):
                        rasterBandObject.DataType = Stac.Common.DataType.float32;
                        break;
                    case (t_DATA_TYPE.LONG):
                        rasterBandObject.DataType = Stac.Common.DataType.uint64;
                        break;
                    case (t_DATA_TYPE.SBYTE):
                        rasterBandObject.DataType = Stac.Common.DataType.int8;
                        break;
                    case (t_DATA_TYPE.SHORT):
                        rasterBandObject.DataType = Stac.Common.DataType.uint16;
                        break;
                    case (t_DATA_TYPE.SLONG):
                        rasterBandObject.DataType = Stac.Common.DataType.int64;
                        break;
                    case (t_DATA_TYPE.SSHORT):
                        rasterBandObject.DataType = Stac.Common.DataType.int16;
                        break;
                }
            }
            else
            {
                rasterBandObject.DataType = Stac.Common.DataType.uint8;
            }
            rasterBandObject.BitsPerSample = rasterEncoding.NBITS;
            if (bandInfo.PHYSICAL_GAINSpecified)
                rasterBandObject.Scale = bandInfo.PHYSICAL_GAIN;
            if (bandInfo.PHYSICAL_BIASSpecified)
                rasterBandObject.Offset = bandInfo.PHYSICAL_BIAS;

            return rasterBandObject;
        }

        internal override StacProvider GetStacProvider()
        {
            DimapDocument dimap = Dimap;
            return new StacProvider(
                dimap.Production.DATASET_PRODUCER_NAME.TrimEnd(new char[] { ' ', '.' }),
                new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor }
            )
            {
                Description = "GEOSAT 2 is a very-high resolution (up to 40cm) multispectral optical satellite with 5 spectral channels, designed for fast off-nadir imaging",
                Uri = new Uri(dimap.Production.DATASET_PRODUCER_URL.href)
            };
        }

        public override string GetPlatformInternationalDesignator()
        {
            return "2014-033D";
        }

        internal override void AddAdditionalProperties(IDictionary<string, object> properties)
        {
            properties.Add("metadata_format", Dimap.Metadata_Id.METADATA_FORMAT?.Value);
            properties.Add("data_file_format", Dimap.Data_Access.DATA_FILE_FORMAT?.Value);
            if (Int32.TryParse(Dimap.Raster_Dimensions.NBANDS, out int bands))
            {
                properties.Add("data_file_bands", bands);
            }
        }


    }
}
