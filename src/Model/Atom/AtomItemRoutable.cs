using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    internal class AtomItemRoutable : IResource, IRoutable
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

        public IEnumerable<IRoute> GetRoutes()
        {
            return item.Links
                .Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType))
                .Select(link => new AtomLinkRoute(link, item));
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
    }
}