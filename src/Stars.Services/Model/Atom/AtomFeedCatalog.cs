﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AtomFeedCatalog.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomFeedCatalog : ICatalog, IStreamResource
    {
        private readonly AtomFeed feed;
        private readonly Uri sourceUri;

        public AtomFeedCatalog(AtomFeed feed, Uri sourceUri)
        {
            this.feed = feed;
            this.sourceUri = sourceUri;
        }

        public AtomFeed AtomFeed => feed;

        public string Title => feed.Title != null ? feed.Title.Text : feed.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => sourceUri ?? feed.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Catalog;

        public string Id => string.IsNullOrEmpty(feed.Id) ? "feed" : feed.Id.CleanIdentifier();

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsStringAsync(CancellationToken.None).Result).Length);

        public bool IsCatalog => true;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public bool CanBeRanged => false;

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return feed.Links.Select(l => new AtomResourceLink(l)).ToList();
        }

        public IReadOnlyList<IResource> GetRoutes(IRouter router)
        {
            return feed.Items.Cast<AtomItem>().Select(item => new AtomItemNode(item, new Uri(Uri, item.Id))).Cast<IResource>().ToList();
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            MemoryStream ms = new MemoryStream();
            return await Task.Run(() =>
            {
                var sw = XmlWriter.Create(ms);
                Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
                atomFormatter.WriteTo(sw);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            }, ct);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

    }
}
