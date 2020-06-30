using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    public class AtomRouter : IResourceRouter
    {

        public AtomRouter()
        {
        }

        public bool CanRoute(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml" || Path.GetExtension(resource.Uri.LocalPath) == ".xml"
                || Path.GetExtension(resource.Uri.LocalPath) == ".atom")
            {
                try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    feedFormatter.ReadFrom(XmlReader.Create(resource.GetAsStream()));
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        public IRoutable Route(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/atom+xml" || Path.GetExtension(resource.Uri.LocalPath) == ".xml"
                || Path.GetExtension(resource.Uri.LocalPath) == ".atom")
            {
                try
                {
                    Atom10FeedFormatter feedFormatter = new Atom10FeedFormatter();
                    feedFormatter.ReadFrom(XmlReader.Create(resource.GetAsStream()));
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
