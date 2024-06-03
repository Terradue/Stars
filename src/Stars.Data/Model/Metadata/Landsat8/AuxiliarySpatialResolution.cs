using Kajabity.Tools.Java;

namespace Terradue.Stars.Data.Model.Metadata.Landsat8
{
    public class AuxiliarySpatialResolution : JavaProperties
    {
        public new string GetProperty(string key)
        {
            var r = base.GetProperty(key);
            if (!string.IsNullOrEmpty(r)) r = r.Trim('"');
            return r;
        }

        // Band
        public string PixelSize_Band1 => GetProperty("BAND01_PIXEL_SIZE");
        public string PixelSize_Band2 => GetProperty("BAND02_PIXEL_SIZE");
        public string PixelSize_Band3 => GetProperty("BAND03_PIXEL_SIZE");
        public string PixelSize_Band4 => GetProperty("BAND04_PIXEL_SIZE");
        public string PixelSize_Band5 => GetProperty("BAND05_PIXEL_SIZE");
        public string PixelSize_Band6 => GetProperty("BAND06_PIXEL_SIZE");
        public string PixelSize_Band7 => GetProperty("BAND07_PIXEL_SIZE");
        public string PixelSize_Band8 => GetProperty("BAND08_PIXEL_SIZE");
        public string PixelSize_Band9 => GetProperty("BAND09_PIXEL_SIZE");
        public string PixelSize_Band10 => GetProperty("BAND10_PIXEL_SIZE");
        public string PixelSize_Band11 => GetProperty("BAND11_PIXEL_SIZE");

        public double GetPixelSizeFromBand(string band)
        {
            return double.Parse(GetProperty($"BAND{$"{band.PadLeft(2, '0')}"}_PIXEL_SIZE"));
        }

    }

}
