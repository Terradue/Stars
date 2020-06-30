using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stac.Item;

namespace Stars.Router
{
    internal class StacLinkRoute : IRoute
    {
        private StacLink link;
        private readonly IStacObject stacObject;

        public StacLinkRoute(StacLink link, IStacObject stacObject)
        {
            this.link = link;
            this.stacObject = stacObject;
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

        public async Task<IResource> GetResource()
        {
            switch (link.RelationshipType)
            {
                case "self":
                    return StacRoutable.Create(stacObject) as IResource;
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