// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacAlternateAssetAsset.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using Stac;
using Stac.Extensions.Alternate;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacAlternateAssetAsset : IAsset
    {
        private readonly StacAssetAsset parentAsset;
        private readonly AlternateAssetObject alternateAsset;
        private readonly Uri uri;

        public StacAlternateAssetAsset(AlternateAssetObject alternateAsset, StacAssetAsset parentAsset)
        {
            this.alternateAsset = alternateAsset;
            this.parentAsset = parentAsset;
            if (alternateAsset.Uri.IsAbsoluteUri)
                uri = alternateAsset.Uri;
            else
            {
                if (parentAsset?.Parent != null)
                {
                    if (parentAsset.Parent.Uri.IsAbsoluteUri)
                        uri = new Uri(parentAsset.Parent.Uri, alternateAsset.Uri);
                    else
                        uri = new Uri(parentAsset.Parent.Uri.ToString() + "/" + alternateAsset.Uri.ToString(), UriKind.Relative);
                }
                else uri = alternateAsset.Uri;
            }
        }

        public Uri Uri => uri;

        public ContentType ContentType => parentAsset.ContentType;

        public ulong ContentLength
        {
            get
            {
                return parentAsset.ContentLength;
            }
        }
        public string Title => alternateAsset.Title;

        public ResourceType ResourceType => ResourceType.Asset;

        public string Filename
        {
            get
            {
                if (alternateAsset.Properties.ContainsKey("filename"))
                    return alternateAsset.GetProperty<string>("filename");
                return parentAsset?.Filename;
            }
        }

        public IReadOnlyList<string> Roles => parentAsset?.Roles.ToList();

        public StacAsset StacAsset { get => parentAsset.StacAsset; }

        public ContentDisposition ContentDisposition => parentAsset.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(alternateAsset.Properties);

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public override string ToString()
        {
            return uri.ToString();
        }

    }
}
