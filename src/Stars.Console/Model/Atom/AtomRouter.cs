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
using Stars.Router;
using Stars.Supply;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    [PluginPriority(10)]
    public class AtomRouter : IRouter
    {

        public AtomRouter()
        {
        }

        public string Label => "Atom";

        public bool CanRoute(INode resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml")
            {
               try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    feedFormatter.ReadFrom(XmlReader.Create(resource.GetStream()));
                    return true;
                }
                catch {}
            }
            return false;
        }

        public async Task<IRoutable> Go(INode resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml" )
            {
                try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create(resource.GetStream())));
                    return new AtomFeedRoutable(feedFormatter.Feed);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

    }
}
