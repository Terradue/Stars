using System;
using System.Net.Mime;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Atom
{
    internal class AtomResourceLink : IResourceLink
    {
        private SyndicationLink atomLink;

        public AtomResourceLink(SyndicationLink l)
        {
            this.atomLink = l;
        }

        public Uri Uri => atomLink.Uri;

        public string Relationship => atomLink.RelationshipType;

        public ContentType ContentType => new ContentType(atomLink.MediaType);

        public string Title => atomLink.Title;

        public ulong ContentLength => Convert.ToUInt64(atomLink.Length);
    }
}