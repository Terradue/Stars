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
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Router;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Services;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Geometry.Atom;
using System.Net;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomItemNode : IItem, IAssetsContainer, IStreamable
    {
        private SyndicationItem item;
        private readonly Uri sourceUri;
        private readonly ICredentials credentials;

        public AtomItemNode(SyndicationItem item, Uri sourceUri, System.Net.ICredentials credentials = null)
        {
            this.item = item;
            this.sourceUri = sourceUri;
            this.credentials = credentials;
        }

        public SyndicationItem AtomItem => item;

        public string Label => item.Title != null ? item.Title.Text : item.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => sourceUri ?? item.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Item;

        public string Id => Identifier ?? item.Id.CleanIdentifier();

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsString().Result).Length);

        public bool IsCatalog => false;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public IReadOnlyList<IResource> GetRoutes()
        {
            return new List<IResource>();
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



        public string Identifier
        {
            get
            {
                var identifier = item.ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? null : identifier[0];
            }
        }

        public IGeometryObject Geometry => item.FindGeometry();

        public IDictionary<string, object> Properties => item.GetCommonMetadata();

        public bool CanBeRanged => false;

        public IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                foreach (var link in item.Links.Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType)))
                {
                    string key = link.RelationshipType;
                    int i = 1;
                    while (assets.ContainsKey(key))
                    {
                        key = link.RelationshipType + i;
                        i++;
                    }
                    assets.Add(key, new AtomLinkAsset(link, item, credentials));
                }

                return assets;
            }
        }
        
        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}