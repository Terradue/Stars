using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Collection;

namespace Stars.Router
{
    internal class StacCatalogRoutable : StacRoutable, IRoutable
    {
        public StacCatalogRoutable(IStacCatalog stacCatalog) : base(stacCatalog)
        {
            if ( stacCatalog is StacCollection )
                contentType.Parameters.Add("profile", "stac-collection");
            else
                contentType.Parameters.Add("profile", "stac-catalog");
        }

        public IStacCatalog StacCatalog => stacObject as IStacCatalog;

        public override IEnumerable<IRoute> GetRoutes()
        {
            return StacCatalog.GetChildrenLinks().Select(link => new StacLinkRoute(link, StacCatalog)).Concat(
                    StacCatalog.GetItemLinks().Select(link => new StacLinkRoute(link, StacCatalog))
                ).Cast<IRoute>();
        }

        public override string ReadAsString()
        {
            return JsonConvert.SerializeObject(StacCatalog);
        }
    }
}