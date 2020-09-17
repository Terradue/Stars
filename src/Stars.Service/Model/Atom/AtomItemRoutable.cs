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
using Stars.Interface.Supply;
using Stars.Interface.Supply.Asset;
using Stars.Service.Router;
using Terradue.ServiceModel.Syndication;
using Stars.Service;

namespace Stars.Service.Model.Atom
{
    public class AtomItemRoutable : INode, IRoutable, IAssetsContainer, IStreamable
    {
        private SyndicationItem item;
        private readonly ISupplier supplier;

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

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsString()).Length);

        public bool IsCatalog => false;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public IList<IRoute> GetRoutes()
        {
            return new List<IRoute>();
        }

        public async Task<Stream> GetStreamAsync()
        {
            return await Task<Stream>.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                var sw = XmlWriter.Create(ms);
                Atom10ItemFormatter atomFormatter = new Atom10ItemFormatter(item);
                atomFormatter.WriteTo(sw);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            });
        }

        public Task<INode> GoToNode()
        {
            return Task.FromResult((INode)this);
        }

        public IEnumerable<IAsset> GetAssets()
        {
            return item.Links
                .Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType))
                .Select(link => new AtomLinkAsset(link, item));
        }
    }
}