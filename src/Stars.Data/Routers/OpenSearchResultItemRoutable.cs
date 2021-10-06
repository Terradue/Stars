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

namespace Terradue.Stars.Data.Routers
{
    public class OpenSearchResultItemRoutable : IResource, IAssetsContainer, IStreamable
    {
        protected IOpenSearchResultItem osItem;

        private Uri sourceUri;
        protected readonly ILogger logger;

        public OpenSearchResultItemRoutable(IOpenSearchResultItem item, Uri sourceUri, ILogger logger)
        {
            this.osItem = item;
            this.sourceUri = sourceUri;
            this.logger = logger;
        }

        public OpenSearchResultItemRoutable()
        {
        }

        public IOpenSearchResultItem OpenSearchResultItem => osItem;

        public string Label => osItem.Title == null ? osItem.Identifier : osItem.Title.Text;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => SelfLink == null ? sourceUri : SelfLink.Uri;

        public SyndicationLink SelfLink => osItem.Links.FirstOrDefault(l => l.RelationshipType == "self");

        public ResourceType ResourceType => ResourceType.Item;

        public string Id => osItem.Identifier;

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(ReadAsString()).Length);

        public bool IsCatalog => false;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public bool CanBeRanged => false;

        public string ReadAsString()
        {
            StreamReader sr = new StreamReader(GetStreamAsync().Result);
            return sr.ReadToEnd();
        }

        public async Task<Stream> GetStreamAsync()
        {
            return await Task<Stream>.Run(() =>
            {
                var atomItem = AtomItem.FromOpenSearchResultItem(osItem);
                MemoryStream ms = new MemoryStream();
                var sw = XmlWriter.Create(ms);
                Atom10ItemFormatter atomFormatter = new Atom10ItemFormatter(atomItem);
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
                foreach (var link in osItem.Links.Where(link => new string[] { "enclosure", "icon" }.Contains(link.RelationshipType)))
                {
                    string key = link.RelationshipType;
                    int i = 1;
                    while (assets.ContainsKey(key))
                    {
                        key = link.RelationshipType + i;
                        i++;
                    }
                    assets.Add(key, new AtomLinkAsset(link, AtomItem.FromOpenSearchResultItem(osItem)));
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