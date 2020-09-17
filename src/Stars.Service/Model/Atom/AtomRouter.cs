using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Service.Router;
using Stars.Service.Supply;
using Terradue.ServiceModel.Syndication;

namespace Stars.Service.Model.Atom
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
                return null;
            }
        }

    }
}
