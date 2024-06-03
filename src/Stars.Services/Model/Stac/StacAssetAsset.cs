// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacAssetAsset.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Stac;
using Stac.Extensions.Alternate;
using Stac.Extensions.File;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacAssetAsset : IAsset
    {
        private readonly StacAsset asset;
        private readonly IAssetsContainer parent;
        private Uri uri;

        public StacAssetAsset(StacAsset asset, IAssetsContainer parent)
        {
            this.asset = asset;
            this.parent = parent;
            if (asset.Uri.IsAbsoluteUri)
                uri = asset.Uri;
            else
            {
                if (parent != null)
                {
                    if (parent.Uri.IsAbsoluteUri)
                        uri = new Uri(parent.Uri, asset.Uri);
                    else
                        uri = new Uri(parent.Uri.ToString() + "/" + asset.Uri.ToString(), UriKind.Relative);
                }
                else uri = asset.Uri;
            }
        }

        public Uri Uri
        {
            get { return uri; }
            private set { uri = value; }
        }

        public ContentType ContentType => asset.MediaType;

        public ulong ContentLength
        {
            get
            {
                if (asset.FileExtension().Size.HasValue) return asset.FileExtension().Size.Value;
                return 0;
            }
        }
        public string Title => asset.Title;

        public ResourceType ResourceType => ResourceType.Asset;

        public string Filename
        {
            get
            {
                if (asset.Properties.ContainsKey("filename"))
                    return asset.GetProperty<string>("filename");
                return Path.GetFileName(Uri.ToString());
            }
        }

        public IReadOnlyList<string> Roles => asset.Roles.ToList();

        public StacAsset StacAsset { get => asset; }

        public ContentDisposition ContentDisposition
        {
            get
            {
                ContentDisposition cd = new ContentDisposition();
                cd.FileName = Filename;
                return cd;
            }
        }

        public IEnumerable<IAsset> Alternates
        {
            get
            {
                var alternateAssets = asset.AlternateExtension().AlternateAssets;
                if (alternateAssets != null)
                    return alternateAssets.Values.Select(a => new StacAlternateAssetAsset(a, this));
                return Enumerable.Empty<IAsset>();
            }
        }

        public IAssetsContainer Parent => parent;

        public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(asset.Properties);

        public override string ToString()
        {
            return uri.ToString();
        }

    }
}
