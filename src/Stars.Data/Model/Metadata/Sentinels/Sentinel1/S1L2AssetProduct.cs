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
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S1.Level2.Product;
using Terradue.Stars.Geometry.Gml321;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1
{
    public class S1L2AssetProduct : SentinelAssetFactory
    {

        public static XmlSerializer s1L2ProductSerializer = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S1.Level2.Product.level2ProductType));
        private readonly level2ProductType l2ProductType;
        private readonly string type;

        public S1L2AssetProduct(level2ProductType l2ProductType, string type) 
        {
            this.l2ProductType = l2ProductType;
            this.type = type;
        }

        public async static Task<S1L2AssetProduct> CreateData(IAsset annotationAsset)
        {
            var streamable = annotationAsset.GetStreamable();
            if ( streamable == null )
                throw new InvalidDataException("Cannot stream data from " + annotationAsset.Uri);
            return new S1L2AssetProduct((level2ProductType)s1L2ProductSerializer.Deserialize(await streamable.GetStreamAsync()), "data");
        }



        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            throw new NotImplementedException();
        }

        public override StacAsset CreateMetadataAsset(IStacObject stacObject)
        {
            throw new NotImplementedException();
        }

        public override string GetAnnotationId()
        {
            throw new NotImplementedException();
        }

        public override double GetGroundSampleDistance()
        {
            throw new NotImplementedException();
        }

        public override string GetId()
        {
            throw new NotImplementedException();
        }
    }
}