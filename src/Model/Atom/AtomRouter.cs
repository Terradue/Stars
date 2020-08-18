using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    [RouterPriority(10)]
    public class AtomRouter : IRouter
    {

        public AtomRouter()
        {
        }

        public string Label => "Atom";

        public bool CanRoute(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml")
            {
               try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    feedFormatter.ReadFrom(XmlReader.Create(resource.GetAsStream()));
                    return true;
                }
                catch {}
            }
            return false;
        }

        public async Task<IRoutable> Go(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml" )
            {
                try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    await Task.Run(() => feedFormatter.ReadFrom(XmlReader.Create(resource.GetAsStream())));
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
