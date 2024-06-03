using System;
using System.IO;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1.Support
{
    public class S1AssetSupport : SentinelAssetFactory
    {

        private readonly IAsset supportAsset;

        public S1AssetSupport(IAsset supportAsset, IResourceServiceProvider resourceServiceProvider) : base(resourceServiceProvider)
        {
            this.supportAsset = supportAsset;
        }

        public static S1AssetSupport CreateMetadata(IAsset supportAsset, IResourceServiceProvider resourceServiceProvider)
        {
            return new S1AssetSupport(supportAsset, resourceServiceProvider);
        }

        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            throw new NotImplementedException();
        }

        private string GetTitle()
        {
            return string.Format("Support file {0}", GetId());

        }

        public override double GetGroundSampleDistance()
        {
            return 0;
        }

        public override string GetId()
        {
            return "support-" + Path.GetFileNameWithoutExtension(supportAsset.Uri.ToString()).ToLower();
        }

        public override string GetAnnotationId()
        {
            throw new NotImplementedException();
        }

        public override StacAsset CreateMetadataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacObject, supportAsset.Uri, new System.Net.Mime.ContentType("text/xml"), GetTitle());
            stacAsset.Properties.AddRange(supportAsset.Properties);
            stacAsset.Roles.Clear();
            stacAsset.Roles.Add("metadata");
            stacAsset.Roles.Add("support");

            return stacAsset;
        }
    }
}
