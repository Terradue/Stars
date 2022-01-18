using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1.Noise
{
    public class S1L1AssetNoise : SentinelAssetFactory
    {

        public static XmlSerializer s1L1NoiseSerializer = new XmlSerializer(typeof(l1NoiseVectorType));
        private readonly l1NoiseVectorType l1Noise;

        private readonly IAsset noiseAsset;

        public S1L1AssetNoise(l1NoiseVectorType l1Calibration, IAsset calibrationAsset)
        {
            this.l1Noise = l1Calibration;
            this.noiseAsset = calibrationAsset;
        }

        public async static Task<S1L1AssetNoise> Create(IAsset calibrationAsset)
        {
            var streamable = calibrationAsset.GetStreamable();
            if (streamable == null)
                throw new InvalidDataException("Cannot stream data from " + calibrationAsset.Uri);
            return new S1L1AssetNoise((l1NoiseVectorType)s1L1NoiseSerializer.Deserialize(await streamable.GetStreamAsync()), calibrationAsset);
        }

        public static Task<S1L1AssetProduct> CreateData(IAsset annotationAsset)
        {
            throw new NotImplementedException();
        }

        protected string GetPolarization()
        {
            return l1Noise.adsHeader.polarisation.ToString();
        }

        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacObject, noiseAsset.Uri, new System.Net.Mime.ContentType("text/xml"), GetTitle());
            stacAsset.Properties.AddRange(noiseAsset.Properties);
            stacAsset.Properties.Add("sar:polarizations", new string[1] { GetPolarization() });
            stacAsset.Roles.Add("noise");

            return stacAsset;
        }

        private string GetTitle()
        {
            return string.Format("Noise {0} {1}",
                GetPolarization(), GetSwath());

        }

        public override double GetGroundSampleDistance()
        {
            return 0;
        }

        public override string GetId()
        {
            return String.Format("noise-{0}-{1}-{2}", GetPolarization(), GetSwath(), GetStripe()).ToLower();
        }

        private object GetStripe()
        {
            return l1Noise.adsHeader.imageNumber;
        }

        private string GetSwath()
        {
            return l1Noise.adsHeader.swath.ToString();
        }

        public override string GetAnnotationId()
        {
            throw new NotImplementedException();
        }

        public override StacAsset CreateMetadataAsset(IStacObject stacObject)
        {
            throw new NotImplementedException();
        }
    }
}