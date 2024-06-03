using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Atom;

namespace Terradue.Stars.Data.Routers
{
    public class OpenSearchResultFeedRoutable : IItemCollection, IStreamResource
    {
        protected IOpenSearchResultCollection osCollection;

        private Uri sourceUri;
        protected readonly ILogger logger;

        public OpenSearchResultFeedRoutable(IOpenSearchResultCollection collection, Uri sourceUri, ILogger logger)
        {
            osCollection = collection;
            this.sourceUri = sourceUri;
            this.logger = logger;
        }

        public OpenSearchResultFeedRoutable()
        {
        }

        public IOpenSearchResultCollection OpenSearchResultCollection => osCollection;

        public string Label => osCollection.Title == null ? osCollection.Identifier : osCollection.Title.Text;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => SelfLink == null ? sourceUri : SelfLink.Uri;

        public SyndicationLink SelfLink => osCollection.Links.FirstOrDefault(l => l.RelationshipType == "self");

        public ResourceType ResourceType => ResourceType.Collection;

        public string Id => osCollection.Identifier;

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(ReadAsStringAsync(System.Threading.CancellationToken.None)).Length);

        public bool IsCatalog => false;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public bool CanBeRanged => false;

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public string ReadAsStringAsync(CancellationToken ct)
        {
            StreamReader sr = new StreamReader(GetStreamAsync(ct).Result);
            return sr.ReadToEnd();
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return await Task<Stream>.Run(() =>
            {
                AtomFeed atomFeed = AtomFeed.CreateFromOpenSearchResultCollection(osCollection) as AtomFeed;
                MemoryStream ms = new MemoryStream();
                var sw = XmlWriter.Create(ms);
                Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(atomFeed);
                atomFormatter.WriteTo(sw);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            });
        }

        public Task<IResource> GoToNode()
        {
            return Task.FromResult((IResource)this);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return osCollection.Links.Select(l => new AtomResourceLink(l)).ToList();
        }

        public void Add(IItem item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IItem item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IItem item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IItem> GetEnumerator()
        {
            return osCollection.Items.Select(i => new AtomItemNode(AtomItem.FromOpenSearchResultItem(i), new Uri(Uri, i.Id))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return osCollection.Items.Select(i => new AtomItemNode(AtomItem.FromOpenSearchResultItem(i), new Uri(Uri, i.Id))).GetEnumerator();
        }
    }
}
