// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AtomRouter.cs

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        private static string[] supportedTypes = new string[] { "application/atom+xml", "application/xml", "text/xml" };

        private static string opensearchDescriptionType = "application/opensearchdescription+xml";

        private readonly IResourceServiceProvider resourceServiceProvider;

        public AtomRouter(IResourceServiceProvider resourceServiceProvider)
        {
            this.resourceServiceProvider = resourceServiceProvider;
        }

        public int Priority { get; set; }
        public string Key { get => "Atom"; set { } }

        public string Label => "Atom Native Router";

        public bool CanRoute(IResource node)
        {
            var affinedRoute = AffineRouteAsync(node, CancellationToken.None).Result;
            if (!supportedTypes.Contains(affinedRoute.ContentType.MediaType)) return false;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                feedFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(affinedRoute, CancellationToken.None).Result.GetStreamAsync(CancellationToken.None).Result));
                return true;
            }
            catch { }
            try
            {
                Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                itemFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(affinedRoute, CancellationToken.None).Result.GetStreamAsync(CancellationToken.None).Result));
                return true;
            }
            catch { }

            return false;
        }

        private async Task<IResource> AffineRouteAsync(IResource route, CancellationToken ct)
        {
            if (supportedTypes.Contains(route.ContentType.MediaType)
                && route is IStreamResource)
            {
                return route;
            }
            if (route.ContentType.MediaType == opensearchDescriptionType && route is IStreamResource streamResource)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(OpenSearchDescription));
                var openSearchDescription = (OpenSearchDescription)xmlSerializer.Deserialize(XmlReader.Create(await streamResource.GetStreamAsync(ct)));
                var url = openSearchDescription.Url.FirstOrDefault(u => supportedTypes.Contains(u.Type));
                if (url != null)
                {
                    OpenSearchUrl searchUri = OpenSearchFactory.BuildRequestUrlFromTemplate(url, new System.Collections.Specialized.NameValueCollection(), new QuerySettings(url.Type, null));
                    return await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(searchUri), ct);
                }
            }
            IResource newRoute = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(route.Uri.ToString())), ct);
            return newRoute;
        }

        public async Task<IResource> RouteAsync(IResource node, CancellationToken ct)
        {
            var affinedRoute = AffineRouteAsync(node, ct).Result;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(affinedRoute, ct).Result.GetStreamAsync(ct).Result)));
                return new AtomFeedCatalog(new AtomFeed(feedFormatter.Feed), affinedRoute.Uri);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(affinedRoute, ct).Result.GetStreamAsync(ct).Result)));
                    return new AtomItemNode(new AtomItem(itemFormatter.Item), affinedRoute.Uri);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<IStreamResource> FetchResourceAsync(IResource node, CancellationToken ct)
        {
            if (node is HttpResource && node.Uri.Query.Contains("format=json"))
            {
                return await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(node.Uri.ToString().Replace("format=json", "format=atom"))), ct);
            }

            if (node is IStreamResource) return node as IStreamResource;

            return await resourceServiceProvider.GetStreamResourceAsync(node, ct);
        }

        public async Task<IResource> RouteLinkAsync(IResource resource, IResourceLink childLink, CancellationToken ct)
        {
            if (!(resource is AtomFeedCatalog)
                && !(resource is AtomItemNode))
            {
                throw new Exception("Cannot route link from non-atom resource");
            }
            var link = resourceServiceProvider.ComposeLinkUri(childLink, resource);
            return await RouteAsync(new GenericResource(link), ct);
        }

    }
}
