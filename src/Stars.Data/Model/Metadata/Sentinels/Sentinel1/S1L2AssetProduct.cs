// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S1L2AssetProduct.cs

using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S1.Level2.Product;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1
{
    public class S1L2AssetProduct : SentinelAssetFactory
    {

        public static XmlSerializer s1L2ProductSerializer = new XmlSerializer(typeof(level2ProductType));
        private readonly level2ProductType l2ProductType;
        private readonly string type;

        public S1L2AssetProduct(level2ProductType l2ProductType, string type, IResourceServiceProvider resourceServiceProvider) : base(resourceServiceProvider)
        {
            this.l2ProductType = l2ProductType;
            this.type = type;
        }

        public static async Task<S1L2AssetProduct> CreateData(IAsset annotationAsset, IResourceServiceProvider resourceServiceProvider)
        {
            return new S1L2AssetProduct((level2ProductType)s1L2ProductSerializer.Deserialize(await resourceServiceProvider.GetAssetStreamAsync(annotationAsset, System.Threading.CancellationToken.None)), "data", resourceServiceProvider);
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
