// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AtomResourceLink.cs

using System;
using System.IO;
using System.Net.Mime;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomResourceLink : IResourceLink
    {
        private readonly SyndicationLink atomLink;

        public AtomResourceLink(SyndicationLink l)
        {
            atomLink = l;
        }

        public Uri Uri => atomLink.Uri;

        public string Relationship => atomLink.RelationshipType;

        public ContentType ContentType => string.IsNullOrEmpty(atomLink.MediaType) ? null : new ContentType(atomLink.MediaType);

        public string Title => atomLink.Title;

        public ulong ContentLength => Convert.ToUInt64(atomLink.Length);

        public ResourceType ResourceType => ResourceType.Unknown;

        public ContentDisposition ContentDisposition => new ContentDisposition(Path.GetFileName(Uri.AbsoluteUri.ToString()));
    }
}
