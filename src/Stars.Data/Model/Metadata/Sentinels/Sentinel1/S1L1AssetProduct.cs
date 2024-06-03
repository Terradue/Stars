using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Stac;
using Stac.Extensions.Projection;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S1.Level1.Product;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1
{
    public class S1L1AssetProduct : SentinelAssetFactory
    {

        public static XmlSerializer s1L1ProductSerializer = new XmlSerializer(typeof(l1ProductType));
        private readonly l1ProductType l1Product;
        private readonly IAsset annotationAsset;
        private readonly IAsset dataAsset;

        public S1L1AssetProduct(l1ProductType l1ProductType, IAsset annotationAsset, IAsset dataAsset, IResourceServiceProvider resourceServiceProvider) : base(resourceServiceProvider)
        {
            l1Product = l1ProductType;
            this.annotationAsset = annotationAsset;
            this.dataAsset = dataAsset;
        }

        public async static Task<S1L1AssetProduct> Create(IAsset annotationAsset, IAsset dataAsset, IResourceServiceProvider resourceServiceProvider)
        {
            return new S1L1AssetProduct((l1ProductType)s1L1ProductSerializer.Deserialize(await resourceServiceProvider.GetAssetStreamAsync(annotationAsset, System.Threading.CancellationToken.None)), annotationAsset, dataAsset, resourceServiceProvider);
        }

        protected string GetPixelValueLabel()
        {
            switch (l1Product.adsHeader.productType)
            {
                case productType.GRD:
                    return "Amplitude";
                case productType.SLC:
                    return "Magnitude";
                case productType.OCN:
                    return "Ocean";
                default:
                    return null;
            }
        }

        protected string GetPolarization()
        {
            return l1Product.adsHeader.polarisation.ToString();
        }

        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacObject, dataAsset.Uri, new System.Net.Mime.ContentType("image/x.geotiff"), GetTitle());
            stacAsset.Properties.AddRange(dataAsset.Properties);
            if (GetGroundSampleDistance() > 0)
                stacAsset.SetProperty("gsd", GetGroundSampleDistance());
            stacAsset.Properties.Add("sar:polarizations", new string[1] { GetPolarization() });
            if (GetPixelValueLabel() != null)
                stacAsset.Roles.Add(GetPixelValueLabel().ToLower());
            stacAsset.ProjectionExtension().Epsg = null;
            stacAsset.ProjectionExtension().Shape = new int[2] { Convert.ToInt32(l1Product.imageAnnotation.imageInformation.numberOfSamples.Value), Convert.ToInt32(l1Product.imageAnnotation.imageInformation.numberOfLines.Value) };

            return stacAsset;
        }

        private string GetTitle()
        {
            return string.Format("{0} {1} {2} pixel values",
                GetSwath(), GetPolarization(), GetPixelValueLabel());

        }

        public override double GetGroundSampleDistance()
        {
            return 0;
        }

        public override string GetId()
        {
            return string.Format("{0}-{1}-{2}-{3}", GetPixelValueLabel(), GetPolarization(), GetSwath(), GetStripe()).ToLower();
        }

        private object GetStripe()
        {
            return l1Product.adsHeader.imageNumber;
        }

        private string GetSwath()
        {
            return l1Product.adsHeader.swath.ToString();
        }

        public override string GetAnnotationId()
        {
            return string.Format("annotation-{0}-{1}-{2}", GetPolarization(), GetSwath(), GetStripe()).ToLower();
        }

        public string GetAnnotationTitle()
        {
            return string.Format("Annotation {0} {1} {2}", GetPolarization(), GetSwath(), GetStripe());
        }

        public override StacAsset CreateMetadataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacObject, annotationAsset.Uri, new System.Net.Mime.ContentType("text/xml"), GetAnnotationTitle());
            stacAsset.Properties.AddRange(annotationAsset.Properties);
            stacAsset.Properties.Add("sar:polarizations", new string[1] { GetPolarization() });

            return stacAsset;
        }
    }
}
