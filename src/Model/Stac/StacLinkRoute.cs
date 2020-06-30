using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal class StacLinkRoute : IRoute
    {
        private StacLink link;
        private readonly IStacObject stacParentObject;

        public StacLinkRoute(StacLink link, IStacObject stacObject)
        {
            this.link = link;
            this.stacParentObject = stacObject;
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

        public bool CanGetResource
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


        public async Task<IResource> GetResource()
        {
            switch (link.RelationshipType)
            {
                case "self":
                    return StacRoutable.Create(stacParentObject) as IResource;
                case "root":
                case "parent":
                    throw new NotImplementedException();
                case "child":
                    return StacRoutable.Create(await (StacCatalog.LoadStacLink(link)));
                case "item":
                    return StacRoutable.Create(await (StacItem.LoadStacLink(link)));
                default:
                    return null;
            }
        }
    }
}