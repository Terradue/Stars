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
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Services.Router;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomFeedRoutable : IRoutable, IStreamable
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

        public string Id => string.IsNullOrEmpty(feed.Id)? "feed" : feed.Id.CleanIdentifier();

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsString().Result).Length);

        public bool IsCatalog => true;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public IList<IRoute> GetRoutes()
        {
            return feed.Items.Select(item => new AtomItemRoute(item, feed)).Cast<IRoute>().ToList();
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

        public IStacObject ReadAsStacObject()
        {
            throw new NotImplementedException();
        }

        public Task<INode> GoToNode()
        {
            return Task.FromResult((INode)this);
        }
    }
}