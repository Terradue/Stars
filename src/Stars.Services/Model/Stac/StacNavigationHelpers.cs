using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stac;
using Stars.Services.Exceptions;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;

namespace Stars.Services.Model.Stac
{
    public static class StacNavigationHelpers
    {

        public static IEnumerable<StacCatalogNode> GetChildren(this StacCatalogNode stacCatalog, StacRouter router)
        {
            return GetChildrenAsync(stacCatalog, router, CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<StacCatalogNode>> GetChildrenAsync(this StacCatalogNode catalog, StacRouter router, CancellationToken ct)
        {
            List<StacCatalogNode> children = new List<StacCatalogNode>();
            foreach (var childLink in catalog.GetLinks().Where(l => !string.IsNullOrEmpty(l.Relationship) && l.Relationship == "child"))
            {
                IResource childRoute = await router.RouteLinkAsync(catalog, childLink, ct);
                children.Add(childRoute as StacCatalogNode);
            }
            return children;
        }

        public static IEnumerable<StacItemNode> GetItems(this StacCatalogNode stacCatalog, StacRouter router)
        {
            return GetItemsAsync(stacCatalog, router, CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<StacItemNode>> GetItemsAsync(this StacCatalogNode stacCatalog, StacRouter router, CancellationToken ct, bool throwOnError = false)
        {
            List<StacItemNode> items = new List<StacItemNode>();
            foreach (var itemLink in stacCatalog.GetLinks().Where(l => !string.IsNullOrEmpty(l.Relationship) && l.Relationship == "item"))
            {
                try
                {
                    IResource itemRoute = await router.RouteLinkAsync(stacCatalog, itemLink, ct);
                    items.Add(itemRoute as StacItemNode);
                }
                catch (Exception)
                {
                    if (throwOnError)
                        throw;
                }
            }
            return items;
        }

    }
}
