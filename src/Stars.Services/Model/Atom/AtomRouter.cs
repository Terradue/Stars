using System;
using System.Threading.Tasks;
using System.Xml;
using Terradue.Stars.Interface.Router;
using Terradue.ServiceModel.Syndication;
using System.Net;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;
using Terradue.OpenSearch.Result;

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {
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
            if (!(node is IStreamable)) return false;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                feedFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result));
                return true;
            }
            catch { }
            try
            {
                Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                itemFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result));
                return true;
            }
            catch { }

            return false;
        }
        
        public async Task<IResource> Route(IResource node)
        {
            if (!(node is IStreamable)) return null;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                return new AtomFeedCatalog(new AtomFeed(feedFormatter.Feed), node.Uri, credentials);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                    return new AtomItemNode(new AtomItem(itemFormatter.Item), node.Uri, credentials);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

    }
}
