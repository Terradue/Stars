using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Collection;
using Stars.Interface.Router;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal class StacCatalogResource : StacResource, IRoutable
    {
        public StacCatalogResource(IStacCatalog stacCatalog) : base(stacCatalog)
        {
            if (stacCatalog is StacCollection)
                contentType.Parameters.Add("profile", "stac-collection");
            else
                contentType.Parameters.Add("profile", "stac-catalog");
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

        public IEnumerable<IRoute> GetRoutes()
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