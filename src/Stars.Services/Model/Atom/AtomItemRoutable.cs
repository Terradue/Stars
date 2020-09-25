using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Stac;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Services.Router;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Services;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomItemRoutable : INode, IRoutable, IAssetsContainer, IStreamable
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

        public string Id => Identifier ?? item.Id.CleanIdentifier();

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

        public string Identifier {
            get {
                var identifier = item.ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? null : identifier[0];
            }
        }

        public IDictionary<string, IAsset> GetAssets()
        {
            Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
            foreach (var link in item.Links.Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType)))
            {
                string key = link.RelationshipType;
                int i = 1;
                while ( assets.ContainsKey(key )){
                    key = link.RelationshipType + i;
                    i++;
                }
                assets.Add(key, new AtomLinkAsset(link, item));
            }

            return assets;
        }
    }
}