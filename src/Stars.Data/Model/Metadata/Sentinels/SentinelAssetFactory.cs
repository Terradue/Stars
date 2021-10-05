using System;
using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels
{
    public abstract class SentinelAssetFactory
    {
        public abstract double GetGroundSampleDistance();
        public abstract string GetId();

        public abstract StacAsset CreateDataAsset(IStacObject stacObject);

        public abstract string GetAnnotationId();

        public abstract StacAsset CreateMetadataAsset(IStacObject stacObject);
    }
}