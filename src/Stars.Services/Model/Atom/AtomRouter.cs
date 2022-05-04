using System;
using System.Threading.Tasks;
using System.Xml;
using Terradue.Stars.Interface.Router;
using Terradue.ServiceModel.Syndication;
using System.Net;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Services.Router;
using System.Linq;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        private static string[] supportedTypes = new string[] { "application/atom+xml", "application/xml", "text/xml" };

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
            if (!supportedTypes.Contains(node.ContentType.MediaType)) return false;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                feedFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(node).Result.GetStreamAsync().Result));
                return true;
            }
            catch { }
            try
            {
                Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                itemFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(node).Result.GetStreamAsync().Result));
                return true;
            }
            catch { }

            return false;
        }

        public async Task<IResource> Route(IResource node)
        {
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(node).Result.GetStreamAsync().Result)));
                return new AtomFeedCatalog(new AtomFeed(feedFormatter.Feed), node.Uri);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create(FetchResourceAsync(node).Result.GetStreamAsync().Result)));
                    return new AtomItemNode(new AtomItem(itemFormatter.Item), node.Uri);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<IStreamResource> FetchResourceAsync(IResource node)
        {
            if (node is HttpResource && node.Uri.Query.Contains("format=json"))
            {
                return await resourceServiceProvider.CreateStreamResourceAsync(new Uri(node.Uri.ToString().Replace("format=json", "format=atom")));
            }

            if (node is IStreamResource) return node as IStreamResource;

            return await resourceServiceProvider.CreateStreamResourceAsync(node);
        }

    }
}
