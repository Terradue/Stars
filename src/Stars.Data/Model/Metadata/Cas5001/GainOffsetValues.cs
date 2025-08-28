using Stac.Extensions.Eo;

namespace Terradue.Stars.Data.Model.Metadata.Cas5001
{
    public class GainOffsetValues
    {
        private int year;
        private Cas5001Season season;
        private EoBandCommonName bandName;
        private double gain;
        private double offset;

        public int Year
        {
            get { return year; }
        }
        public Cas5001Season Season
        {
            get { return season; }
        }

        public EoBandCommonName BandName
        {
            get { return bandName; }
        }
        public double Gain
        {
            get { return gain; }
        }
        public double Offset
        {
            get { return offset; }
        }

        public GainOffsetValues(int year, Cas5001Season season, EoBandCommonName bandName, double gain, double offset)
        {
            this.year = year;
            this.season = season;
            this.bandName = bandName;
            this.gain = gain;
            this.offset = offset;
        }
    }

    public enum Cas5001Season
    {
        Summer,
        Winter
    }
}
