using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Processing;
using Terradue.ServiceModel.Syndication;
using Microsoft.Extensions.Configuration;

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        public AtomRouter()
        {
        }

        public string Label => "Atom";

        public bool CanRoute(IRoute node)
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

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {

        }

        public async Task<IRoute> Route(IRoute node)
        {
            if (!(node is IStreamable)) return null;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                return new AtomFeedCatalog(feedFormatter.Feed, node.Uri);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                    return new AtomItemNode(itemFormatter.Item, node.Uri);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

    }
}
