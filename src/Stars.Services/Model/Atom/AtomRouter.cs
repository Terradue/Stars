using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supply;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Services.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        public AtomRouter()
        {
        }

        public string Label => "Atom";

        public bool CanRoute(INode node)
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

        public async Task<IRoutable> Route(INode node)
        {
            if (!(node is IStreamable)) return null;
            try
            {
                Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                return new AtomFeedRoutable(feedFormatter.Feed);
            }
            catch (Exception)
            {
                try
                {
                    Atom10ItemFormatter itemFormatter = new Atom10ItemFormatter();
                    await Task.Run(() => itemFormatter.ReadFrom(XmlReader.Create((node as IStreamable).GetStreamAsync().Result)));
                    return new AtomItemRoutable(itemFormatter.Item);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

    }
}
