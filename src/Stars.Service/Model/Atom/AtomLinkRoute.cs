using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Service.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Service.Model.Atom
{
    internal class AtomLinkRoute : IRoute
    {
        private SyndicationLink link;
        private SyndicationItem item;

        public AtomLinkRoute(SyndicationLink link, SyndicationItem item)
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

        public bool CanRead => false;

        public ResourceType ResourceType {
            get {
                switch (link.RelationshipType){
                    case "enclosure":
                    case "icon":
                        return ResourceType.Asset;
                    default:
                        return ResourceType.Unknown;
                }
            }
        }

        public ulong ContentLength => Convert.ToUInt64(link.Length);

        public Task<INode> GoToNode()
        {
            switch (link.RelationshipType)
            {
                case "self":
                    return Task<INode>.FromResult((INode)new AtomItemRoutable(item));
                case "enclosure":
                case "icon":
                default:
                    return WebRoute.Create(link.Uri).GoToNode();
            }
        }
    }
}