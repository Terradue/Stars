using System;
using System.Collections.Generic;
using System.Globalization;
using Stac;
using Stac.Extensions.Eo;
using Stac;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;
using Stac.Extensions.Raster;

namespace Terradue.Stars.Data.Model.Metadata.Dimap
{
    internal class GenericDimapProfiler : DimapProfiler
    {

        public GenericDimapProfiler(DimapDocument dimap)
        {
            Dimap = dimap;
        }

        internal override string GetProcessingLevel()
        {
            return Dimap.Production.PRODUCT_TYPE;
        }

        internal override string GetOrbitState()
        {
            return "ascending";
        }

        public override string GetProductKey(IAsset bandAsset, t_Data_File dataFile)
        {
            try
            {
                if (dataFile.DATA_FILE_PATH.href.Contains("pan"))
                    return "pan";
            }
            catch { }
            return "composite";
        }

        public override string GetTitle(IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            return string.Format("{0} {1} {2} {3}",
                                                  GetPlatform(),
                                                  string.Join("/", GetInstruments()),
                                                  GetProcessingLevel(),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture));
        }

        protected override EoBandObject GetEoBandObject(Schemas.t_Spectral_Band_Info bandInfo, string description)
        {
            EoBandObject eoBandObject = new EoBandObject(bandInfo.BAND_DESCRIPTION ?? bandInfo.BAND_INDEX.Value,
                                                (EoBandCommonName)Enum.Parse(typeof(EoBandCommonName), bandInfo.BAND_DESCRIPTION.ToLower()));
            eoBandObject.Description = string.Format("{0} {1} in {2}", bandInfo.BAND_DESCRIPTION, description, bandInfo.PHYSICAL_UNIT);
            return eoBandObject;
        }

        protected override RasterBand GetRasterBandObject(Schemas.t_Spectral_Band_Info bandInfo, Schemas.t_Raster_Encoding rasterEncoding)
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

        internal override string GetSensorMode()
        {
            return null;
        }
    }
}