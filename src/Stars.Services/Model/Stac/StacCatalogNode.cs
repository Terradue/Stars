using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Stac;
using Stars.Services.Model.Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacCatalogNode : StacNode, ICatalog
    {
        public StacCatalogNode(IStacCatalog stacCatalog, Uri uri, ICredentials credentials = null) : base(stacCatalog, uri, credentials)
        {
        }

        public IStacCatalog StacCatalog => stacObject as IStacCatalog;

        public override ResourceType ResourceType
        {
            get
            {
                if (stacObject is StacCollection)
                    return ResourceType.Collection;
                else
                    return ResourceType.Catalog;
            }
        }

        public override IReadOnlyList<IResource> GetRoutes(ICredentials credentials)
        {
            StacRouter stacRouter = new StacRouter(credentials);
            return StacCatalog.GetChildren(this.Uri, stacRouter).Select(child => new StacCatalogNode(child.Value, child.Key)).Cast<IResource>()
                    .Concat(StacCatalog.GetItems(this.Uri, stacRouter).Select(item => new StacItemNode(item.Value, item.Key)))
                    .ToList();
        }

        public static StacCatalogNode CreateUnlocatedNode(IStacCatalog catalog)
        {
            return new StacCatalogNode(catalog, new Uri(catalog.Id + ".json", UriKind.Relative));
        }
    }
}