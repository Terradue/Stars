using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Collection;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal class StacCatalogRoutable : StacRoutable, IRoutable
    {
        public StacCatalogRoutable(IStacCatalog stacCatalog) : base(stacCatalog)
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

        public override string Filename {
            get
            {
                if (stacObject is StacCollection)
                    return stacObject.Id.CleanIdentifier() + ".collection.json";
                else
                    return "catalog.json";
            }
        }

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