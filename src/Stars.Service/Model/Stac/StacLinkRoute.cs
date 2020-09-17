using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Interface.Router;
using Stars.Service.Router;

namespace Stars.Service.Model.Stac
{
    internal class StacLinkRoute : IRoute
    {
        private StacLink link;
        private readonly IStacObject stacParentObject;

        public StacLinkRoute(StacLink link, IStacObject stacParentObject)
        {
            this.link = link;
            this.stacParentObject = stacParentObject;
        }

        public Uri Uri => link.Uri;

        public ContentType ContentType
        {
            get
            {
                if (string.IsNullOrEmpty(link.MediaType))
                    return null;
                return new ContentType(link.MediaType);
            }
        }

        public bool CanRead
        {
            get
            {
                return new string[] { "self", "root", "parent", "child", "item" }.Contains(link.RelationshipType);
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                switch (link.RelationshipType)
                {
                    case "self":
                        if (stacParentObject is IStacItem)
                            return ResourceType.Item;
                        if (stacParentObject is IStacCatalog)
                            return ResourceType.Catalog;
                        break;
                    case "root":
                    case "parent":
                        return ResourceType.Catalog;
                    case "child":
                        return ResourceType.Catalog;
                    case "item":
                        return ResourceType.Item;
                    default:
                        return ResourceType.Unknown;
                }
                return ResourceType.Unknown;
            }
        }

        public ulong ContentLength => link.Length;

        public async Task<INode> GoToNode()
        {
            switch (link.RelationshipType)
            {
                case "self":
                    return StacNode.Create(stacParentObject) as INode;
                case "root":
                case "parent":
                    throw new NotImplementedException();
                case "child":
                    return StacNode.Create(await (StacCatalog.LoadStacLink(link)));
                case "item":
                    return StacNode.Create(await (StacItem.LoadStacLink(link)));
                default:
                    return null;
            }
        }
    }
}