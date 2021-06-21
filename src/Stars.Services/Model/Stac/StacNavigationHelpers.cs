using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Services.Exceptions;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
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
                Uri linkUri = childLink.Uri;
                if (!linkUri.IsAbsoluteUri && baseUri.IsAbsoluteUri)
                    linkUri = new Uri(baseUri, childLink.Uri);
                children.Add(linkUri, await childLink.CreateStacObject(baseUri, stacRouter.Credentials) as IStacCatalog);
            }
            return children;
        }

        private static async Task<IStacObject> CreateStacObject(this StacLink stacLink, Uri baseUri = null, ICredentials credentials = null)
        {
            if (stacLink is StacObjectLink)
                return (stacLink as StacObjectLink).StacObject;
            Uri linkUri = stacLink.Uri;
            if (!linkUri.IsAbsoluteUri)
            {
                if (baseUri == null)
                    throw new RoutingException(string.Format("relative route without base Url : {0}", linkUri));
                linkUri = new Uri(baseUri, linkUri);
            }
            var webRoute = WebRoute.Create(linkUri, stacLink.Length, credentials);
            return StacConvert.Deserialize<IStacObject>(await webRoute.GetStreamAsync());
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
                Uri linkUri = itemLink.Uri;
                if (!linkUri.IsAbsoluteUri && baseUri.IsAbsoluteUri)
                    linkUri = new Uri(baseUri, itemLink.Uri);
                items.Add(linkUri, await itemLink.CreateStacObject(baseUri, stacRouter.Credentials) as StacItem);
            }
            return items;
        }

    }
}
