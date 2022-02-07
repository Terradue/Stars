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

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        private static string[] supportedTypes = new string[] { "application/atom+xml", "application/xml", "text/xml" };

        private ICredentials credentials;

        public AtomRouter(ICredentials credentials)
        {
            this.credentials = credentials;
        }

        public int Priority { get; set; }
        public string Key { get => "Atom"; set { } }

        public string Label => "Atom Native Router";

        public bool CanRoute(IResource node)
        {
            if ( !supportedTypes.Contains(node.ContentType.MediaType) )
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                feedFormatter.ReadFrom(XmlReader.Create(FetchResource(node).GetStreamAsync().Result));
                return true;
            }
            catch { }
            try
            {
                Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                itemFormatter.ReadFrom(XmlReader.Create(FetchResource(node).GetStreamAsync().Result));
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
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create(FetchResource(node).GetStreamAsync().Result)));
                return new AtomFeedCatalog(new AtomFeed(feedFormatter.Feed), node.Uri, credentials);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create(FetchResource(node).GetStreamAsync().Result)));
                    return new AtomItemNode(new AtomItem(itemFormatter.Item), node.Uri, credentials);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public IStreamable FetchResource(IResource node)
        {
            if (node is WebRoute && node.Uri.Query.Contains("format=json"))
            {
                return (node as WebRoute).CloneRoute(new Uri (node.Uri.ToString().Replace("format=json", "format=atom")));
            }

            if (node is IStreamable) return node as IStreamable;

            return WebRoute.Create(node.Uri);
        }

    }
}
