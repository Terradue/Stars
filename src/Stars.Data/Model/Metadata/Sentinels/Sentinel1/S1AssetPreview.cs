using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1.Preview
{
    public class S1AssetPreview : SentinelAssetFactory
    {

        private readonly IAsset previewAsset;

        public S1AssetPreview(IAsset previewAsset, IResourceServiceProvider resourceServiceProvider) : base(resourceServiceProvider)
        {
            this.previewAsset = previewAsset;
        }

        public static S1AssetPreview CreateMetadata(IAsset supportAsset, IResourceServiceProvider resourceServiceProvider)
        {
            return new S1AssetPreview(supportAsset, resourceServiceProvider);
        }

        public override StacAsset CreateDataAsset(IStacObject stacObject)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacObject, previewAsset.Uri, GetContentType(), GetTitle());
            stacAsset.Properties.AddRange(previewAsset.Properties);
            stacAsset.Roles.Clear();
            stacAsset.Roles.Add("data");

            var filename = Path.GetFileName(previewAsset.Uri.ToString());

            if ( filename == "quick-look.png")
                    stacAsset.Roles.Add("thumbnail");
            if ( filename == "map-overlay.kml")
                    stacAsset.Roles.Add("kml");
            if ( filename == "logo.png")
                    stacAsset.Roles.Add("logo");

            return stacAsset;
        }

        private ContentType GetContentType()
        {
            var filename = Path.GetFileName(previewAsset.Uri.ToString());
            if (filename == "quick-look.png")
                return new ContentType("image/png");
            if (filename == "map-overlay.kml")
                return new ContentType("application/vnd.google-earth.kml+xml");
            if (filename == "logo.png")
                return new ContentType("image/png");
            return new ContentType("application/octet-stream");
        }

        private string GetTitle()
        {
            return string.Format("Preview file {0}", GetId());

        }

        public override double GetGroundSampleDistance()
        {
            return 0;
        }

        public override string GetId()
        {
            return "preview-" + Path.GetFileNameWithoutExtension(previewAsset.Uri.ToString()).ToLower();
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