using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Collection;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacCatalogNode : StacNode, ICatalog
    {
        public StacCatalogNode(IStacCatalog stacCatalog) : base(stacCatalog)
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

        public override IList<IRoute> GetRoutes()
        {
            return StacCatalog.GetChildren().Values.Select(child => new StacCatalogNode(child)).Concat(
                    StacCatalog.GetItems().Values.Select(item => new StacItemNode(item)).Cast<IRoute>()
                ).Cast<IRoute>().ToList();
        }

    }
}