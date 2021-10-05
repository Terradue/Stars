using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GeoJSON.Net.Geometry;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S1.Level1.Product;
using Terradue.Stars.Geometry.Gml321;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1.Calibration
{
    public class S1L1AssetCalibration : SentinelAssetFactory
    {

        public static XmlSerializer s1L1CalibrationSerializer = new XmlSerializer(typeof(l1CalibrationType));
        private readonly l1CalibrationType l1Calibration;

        private readonly IAsset calibrationAsset;

        public S1L1AssetCalibration(l1CalibrationType l1Calibration, IAsset calibrationAsset)
        {
            this.l1Calibration = l1Calibration;
            this.calibrationAsset = calibrationAsset;
        }

        public async static Task<S1L1AssetCalibration> Create(IAsset calibrationAsset)
        {
            var streamable = calibrationAsset.GetStreamable();
            if (streamable == null)
                throw new InvalidDataException("Cannot stream data from " + calibrationAsset.Uri);
            return new S1L1AssetCalibration((l1CalibrationType)s1L1CalibrationSerializer.Deserialize(await streamable.GetStreamAsync()), calibrationAsset);
        }

        public static Task<S1L1AssetProduct> CreateData(IAsset annotationAsset)
        {
            throw new NotImplementedException();
        }

        protected string GetPolarization()
        {
            return l1Calibration.adsHeader.polarisation.ToString();
        }

        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacObject, calibrationAsset.Uri, new System.Net.Mime.ContentType("text/xml"), GetTitle());
            stacAsset.Properties.AddRange(calibrationAsset.Properties);
            stacAsset.Properties.Add("sar:polarizations", new string[1] { GetPolarization() });
            stacAsset.Roles.Add("calibration");

            return stacAsset;
        }

        private string GetTitle()
        {
            return string.Format("Calibration {0} {1}",
                GetPolarization(), GetSwath());

        }

        public override double GetGroundSampleDistance()
        {
            return 0;
        }

        public override string GetId()
        {
            return String.Format("calibration-{0}-{1}-{2}", GetPolarization(), GetSwath(), GetStripe()).ToLower();
        }

        private object GetStripe()
        {
            return l1Calibration.adsHeader.imageNumber;
        }

        private string GetSwath()
        {
            return l1Calibration.adsHeader.swath.ToString();
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