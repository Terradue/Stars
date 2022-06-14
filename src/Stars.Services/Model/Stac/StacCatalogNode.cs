using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Stac;
using Stars.Services.Model.Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacCatalogNode : StacNode, ICatalog
    {
        public StacCatalogNode(IStacCatalog stacCatalog, Uri uri) : base(stacCatalog, uri)
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

        public override IReadOnlyList<IResource> GetRoutes(IRouter router)
        {
            StacRouter stacRouter = router as StacRouter;
            if (stacRouter == null)
                throw new Exception("Router is not a StacRouter");
            return this.GetChildren(stacRouter).Cast<IResource>()
                    .Concat(this.GetItems(stacRouter).Cast<IResource>())
                    .ToList();
        }
    }
}