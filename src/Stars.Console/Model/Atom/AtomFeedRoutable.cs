using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Collection;
using Stars.Interface.Router;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    internal class AtomFeedRoutable : IRoutable
    {
        private readonly SyndicationFeed feed;

        public AtomFeedRoutable(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        public SyndicationFeed AtomFeed => feed;

        public string Label => feed.Title != null ? feed.Title.Text : feed.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => feed.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Catalog;

        public string Id => feed.Id.CleanIdentifier();

        public string Filename => "atomfeed.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(ReadAsString()).Length);

        public IEnumerable<IRoute> GetRoutes()
        {
            return feed.Items.Select(item => new AtomItemRoute(item, feed));
        }

       public string ReadAsString()
        {
            StreamReader sr = new StreamReader(GetAsStream());
            return sr.ReadToEnd();
        }

        public Stream GetAsStream() {
            MemoryStream ms = new MemoryStream();
            var sw = XmlWriter.Create(ms);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public IStacObject ReadAsStacObject()
        {
            throw new NotImplementedException();
        }

        public Task<IResource> GotoResource()
        {
            return Task.FromResult((IResource)this);
        }
    }
}