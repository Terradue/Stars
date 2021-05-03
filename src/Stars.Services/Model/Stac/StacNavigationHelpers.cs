using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Stac;
using Stars.Services.Exceptions;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;

namespace Stars.Services.Model.Stac
{
    public static class StacNavigationHelpers
    {

        public static IDictionary<Uri, IStacCatalog> GetChildren(this IStacObject stacObject, Uri baseUri, StacRouter stacRouter)
        {
            return GetChildrenAsync(stacObject, baseUri, stacRouter).GetAwaiter().GetResult();
        }

        public static async Task<IDictionary<Uri, IStacCatalog>> GetChildrenAsync(this IStacObject stacObject, Uri baseUri, StacRouter stacRouter)
        {
            Dictionary<Uri, IStacCatalog> children = new Dictionary<Uri, IStacCatalog>();
            foreach (var childLink in stacObject.Links.Where(l => !string.IsNullOrEmpty(l.RelationshipType) && l.RelationshipType == "child"))
            {
                WebRoute childRoute = childLink.CreateRoute(stacObject, baseUri, stacRouter.Credentials);
                children.Add(childRoute.Uri, StacConvert.Deserialize<IStacCatalog>(await childRoute.GetStreamAsync()));
            }
            return children;
        }

        private static WebRoute CreateRoute(this StacLink stacLink, IStacObject stacObject, Uri baseUri = null, ICredentials credentials = null)
        {
            Uri linkUri = stacLink.Uri;
            if (!linkUri.IsAbsoluteUri)
            {
                if (baseUri == null)
                    throw new RoutingException(string.Format("relative route without base Url : {0}", linkUri));
                linkUri = new Uri(baseUri, linkUri);
            }
            return WebRoute.Create(linkUri, stacLink.Length, credentials);
        }

        public static IDictionary<Uri, StacItem> GetItems(this IStacObject stacObject, Uri baseUri, StacRouter stacRouter)
        {
            return GetItemsAsync(stacObject, baseUri, stacRouter).GetAwaiter().GetResult();
        }

        public static async Task<IDictionary<Uri, StacItem>> GetItemsAsync(this IStacObject stacObject, Uri baseUri, StacRouter stacRouter)
        {
            Dictionary<Uri, StacItem> items = new Dictionary<Uri, StacItem>();
            foreach (var itemLink in stacObject.Links.Where(l => !string.IsNullOrEmpty(l.RelationshipType) && l.RelationshipType == "item"))
            {
                WebRoute itemRoute = itemLink.CreateRoute(stacObject, baseUri, stacRouter.Credentials);
                items.Add(itemRoute.Uri, StacConvert.Deserialize<StacItem>(await itemRoute.GetStreamAsync()));
            }
            return items;
        }

    }
}
