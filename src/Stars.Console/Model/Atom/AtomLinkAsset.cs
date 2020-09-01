using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;
using Stars.Router;
using Stars.Supply.Asset;
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

        public ulong ContentLength => Convert.ToUInt64(link.Length);

        public string Label => string.Format("[{0}] {1}", string.Join(",", link.RelationshipType), string.IsNullOrEmpty(link.Title) ? Path.GetFileName(link.Uri.AbsolutePath) : link.Title);

        public ResourceType ResourceType => ResourceType.Asset;

        public async Task<INode> GoToNode()
        {
            return await WebRoute.Create(Uri).GoToNode();
        }
    }
}