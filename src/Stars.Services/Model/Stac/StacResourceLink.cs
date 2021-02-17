using System;
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
            this.stacLink = l;
        }

        public Uri Uri => stacLink.Uri;

        public string Relationship => stacLink.RelationshipType;

        public ContentType ContentType => stacLink.MediaType;

        public string Title => stacLink.Title;

        public ulong ContentLength => stacLink.Length;
    }
}