using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terradue.ServiceModel.Syndication;
using System.Net;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomFeedCatalog : ICatalog, IStreamable
    {
        private readonly SyndicationFeed feed;
        private readonly Uri sourceUri;
        private readonly ICredentials credentials;

        public AtomFeedCatalog(SyndicationFeed feed, Uri sourceUri, System.Net.ICredentials credentials = null)
        {
            this.feed = feed;
            this.sourceUri = sourceUri;
            this.credentials = credentials;
        }

        public SyndicationFeed AtomFeed => feed;

        public string Label => feed.Title != null ? feed.Title.Text : feed.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => sourceUri ?? feed.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Catalog;

        public string Id => string.IsNullOrEmpty(feed.Id)? "feed" : feed.Id.CleanIdentifier();

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsString().Result).Length);

        public bool IsCatalog => true;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public bool CanBeRanged => false;

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return feed.Links.Select(l => new AtomResourceLink(l)).ToList();
        }

        public IReadOnlyList<IResource> GetRoutes(ICredentials credentials)
        {
            return feed.Items.Select(item => new AtomItemNode(item, new Uri(Uri, item.Id), credentials)).Cast<IResource>().ToList();
        }

        public async Task<Stream> GetStreamAsync()
        {
            MemoryStream ms = new MemoryStream();
            return await Task<Stream>.Run(() =>
            {
                var sw = XmlWriter.Create(ms);
                Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
                atomFormatter.WriteTo(sw);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            });
        }

        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }

    }
}