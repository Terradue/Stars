using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Services.Router;
using Terradue.ServiceModel.Syndication;
using System.Net;
using Terradue.Stars.Interface;
using System.Linq;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomLinkAsset : IAsset
    {
        private SyndicationLink link;
        private SyndicationItem item;
        private readonly ICredentials credentials;
        private readonly WebRoute webRoute;

        public AtomLinkAsset(SyndicationLink link, SyndicationItem item, System.Net.ICredentials credentials = null)
        {
            this.link = link;
            this.item = item;
            this.credentials = credentials;
            this.webRoute = WebRoute.Create(Uri, Convert.ToUInt64(link.Length), credentials);
        }

        public Uri Uri => link.Uri;

        public ContentType ContentType
        {
            get
            {
                string mediaType = MimeTypes.GetMimeType(link.Uri.ToString());
                try
                {
                    return new ContentType(link.MediaType);
                }
                catch (Exception e)
                {
                    return new ContentType(mediaType);
                }
            }
        }

        public ulong ContentLength => link.Length == 0 ? webRoute.ContentLength : Convert.ToUInt64(link.Length);

        public string Title => link.Title == null ? Path.GetFileName(link.Uri.AbsolutePath) : link.Title.ToString();

        public ResourceType ResourceType => ResourceType.Asset;

        public IReadOnlyList<string> Roles => new string[] { link.RelationshipType };

        public ContentDisposition ContentDisposition => webRoute.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => link.AttributeExtensions.ToDictionary(k => k.Key.ToString(), k => k.Value as object);

        public IStreamable GetStreamable()
        {
            return webRoute;
        }

        public static IDictionary<string, IAsset> ResolveEnclosure(SyndicationLink link, SyndicationItem item, ICredentials credentials, string key)
        {
            Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
            WebRoute webRoute = WebRoute.Create(link.Uri, Convert.ToUInt64(link.Length), credentials);
            if (webRoute.IsFolder)
            {
                IEnumerable<WebRoute> childrenRoutes = webRoute.ListFolder();
                int i = 0;
                foreach (var childRoute in childrenRoutes)
                {
                    i++;
                    assets.Add(key + "-" + i, new GenericAsset(childRoute,
                        link.Title + " " + childRoute.Uri.ToString().Replace(webRoute.Uri.ToString(), ""),
                         new string[] { link.RelationshipType }));
                }
            }
            else
                assets.Add(key, new AtomLinkAsset(link, item, credentials));

            return assets;
        }
    }
}