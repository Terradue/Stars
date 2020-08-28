using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Stac;
using Stars.Interface.Router;
using Stars.Router;
using Stars.Supply.Asset;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    internal class AtomItemRoutable : IResource, IRoutable, IAssetsContainer
    {
        private SyndicationItem item;

        public AtomItemRoutable(SyndicationItem item)
        {
            this.item = item;
        }

        public SyndicationItem AtomItem => item;

        public string Label => item.Title != null ? item.Title.Text : item.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => item.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Item;

        public string Id => item.Id.CleanIdentifier();

        public string Filename => Id + ".item.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(ReadAsString()).Length);

        public IEnumerable<IRoute> GetRoutes()
        {
            return new List<IRoute>();
        }

        public string ReadAsString()
        {
            StreamReader sr = new StreamReader(GetAsStream());
            return sr.ReadToEnd();
        }

        public Stream GetAsStream()
        {
            MemoryStream ms = new MemoryStream();
            var sw = XmlWriter.Create(ms);
            Atom10ItemFormatter atomFormatter = new Atom10ItemFormatter(item);
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

        public IEnumerable<IAsset> GetAssets()
        {
            return item.Links
                .Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType))
                .Select(link => new AtomLinkAsset(link, item));
        }
    }
}