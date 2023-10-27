using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terradue.Stars.Services.Model.Atom;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Terradue.Stars.Data.Routers
{
    public class OpenSearchResultFeedRoutable : IResource, IAssetsContainer, IStreamResource
    {
        protected IOpenSearchResultCollection osCollection;

        private Uri sourceUri;
        protected readonly ILogger logger;

        public OpenSearchResultFeedRoutable(IOpenSearchResultCollection collection, Uri sourceUri, ILogger logger)
        {
            this.osCollection = collection;
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

        public virtual IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                return assets;
            }
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}