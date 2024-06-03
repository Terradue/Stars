using System;
using System.IO;
using System.Net.Mime;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    internal class StacResourceLink : IResourceLink
    {
        private StacLink stacLink;

        public StacResourceLink(StacLink l)
        {
            stacLink = l;
        }

        public Uri Uri => stacLink.Uri;

        public string Relationship => stacLink.RelationshipType;

        public ContentType ContentType => stacLink.ContentType;

        public string Title => stacLink.Title;

        public ulong ContentLength => stacLink.Length;

        public ResourceType ResourceType
        {
            get
            {
                if (stacLink.RelationshipType == "root" || stacLink.RelationshipType == "catalog")
                    return ResourceType.Catalog;
                if (stacLink.RelationshipType == "collection")
                    return ResourceType.Collection;
                if (stacLink.RelationshipType == "item" || stacLink.RelationshipType == "child")
                    return ResourceType.Item;
                return ResourceType.Unknown;
            }
        }

        public ContentDisposition ContentDisposition => new ContentDisposition(Path.GetFileName(Uri.AbsoluteUri.ToString()));
    }
}
