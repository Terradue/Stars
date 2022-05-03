using System;
using System.Collections.Generic;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels
{
    public abstract class SentinelAssetFactory
    {
        protected IResourceServiceProvider resourceServiceProvider;

        protected SentinelAssetFactory(IResourceServiceProvider resourceServiceProvider)
        {
            this.resourceServiceProvider = resourceServiceProvider;
        }

        public abstract double GetGroundSampleDistance();
        public abstract string GetId();

        public abstract StacAsset CreateDataAsset(IStacObject stacObject);

        public abstract string GetAnnotationId();

        public abstract StacAsset CreateMetadataAsset(IStacObject stacObject);
    }
}