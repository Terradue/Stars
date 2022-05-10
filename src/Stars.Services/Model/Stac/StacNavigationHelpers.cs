using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Stac;
using Stars.Services.Exceptions;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;

namespace Stars.Services.Model.Stac
{
    public static class StacNavigationHelpers
    {

        public static IDictionary<Uri, IStacCatalog> GetChildren(this IStacObject stacObject, Uri baseUri, IResourceServiceProvider resourceServiceProvider)
        {
            return GetChildrenAsync(stacObject, baseUri, resourceServiceProvider).GetAwaiter().GetResult();
        }

        public static async Task<IDictionary<Uri, IStacCatalog>> GetChildrenAsync(this IStacObject stacObject, Uri baseUri, IResourceServiceProvider resourceServiceProvider)
        {
            Dictionary<Uri, IStacCatalog> children = new Dictionary<Uri, IStacCatalog>();
            foreach (var childLink in stacObject.Links.Where(l => !string.IsNullOrEmpty(l.RelationshipType) && l.RelationshipType == "child"))
            {
                Uri linkUri = childLink.Uri;
                if (!linkUri.IsAbsoluteUri && baseUri.IsAbsoluteUri)
                    linkUri = new Uri(baseUri, childLink.Uri);
                children.Remove(linkUri);
                children.Add(linkUri, await childLink.CreateStacObject(baseUri, resourceServiceProvider) as IStacCatalog);
            }
            return children;
        }

        private static async Task<IStacObject> CreateStacObject(this StacLink stacLink, Uri baseUri = null, IResourceServiceProvider resourceServiceProvider = null, ICredentials credentials = null)
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
            var webRoute = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(linkUri));
            return StacConvert.Deserialize<IStacObject>(await webRoute.GetStreamAsync());
        }

        public static IDictionary<Uri, StacItem> GetItems(this IStacObject stacObject, Uri baseUri, IResourceServiceProvider resourceServiceProvider)
        {
            return GetItemsAsync(stacObject, baseUri, resourceServiceProvider).GetAwaiter().GetResult();
        }

        public static async Task<IDictionary<Uri, StacItem>> GetItemsAsync(this IStacObject stacObject, Uri baseUri, IResourceServiceProvider resourceServiceProvider, bool throwOnError = false)
        {
            Dictionary<Uri, StacItem> items = new Dictionary<Uri, StacItem>();
            foreach (var itemLink in stacObject.Links.Where(l => !string.IsNullOrEmpty(l.RelationshipType) && l.RelationshipType == "item"))
            {
                try
                {
                    Uri linkUri = itemLink.Uri;
                    if (!linkUri.IsAbsoluteUri && baseUri.IsAbsoluteUri)
                        linkUri = new Uri(baseUri, itemLink.Uri);
                    items.Remove(linkUri);
                    items.Add(linkUri, await itemLink.CreateStacObject(baseUri, resourceServiceProvider) as StacItem);
                }
                catch (Exception e)
                {
                    if (throwOnError)
                        throw e;
                }
            }
            return items;
        }

    }
}
