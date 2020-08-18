using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Router;
using Stars.Supplier.Asset;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    internal class AtomLinkAsset : IAsset
    {
        private SyndicationLink link;
        private SyndicationItem item;

        public AtomLinkAsset(SyndicationLink link, SyndicationItem item)
        {
            this.link = link;
            this.item = item;
        }

        public Uri Uri => link.Uri;

        public ContentType ContentType
        {
            get
            {
                try
                {
                    return new ContentType(link.MediaType);
                }
                catch
                {
                    return null;
                }
            }
        }

        public long ContentLength => link.Length;

        public string Label => string.Format("[{0}] {1}", string.Join(",", link.RelationshipType), string.IsNullOrEmpty(link.Title) ? Path.GetFileName(link.Uri.AbsolutePath) : link.Title);
    }
}