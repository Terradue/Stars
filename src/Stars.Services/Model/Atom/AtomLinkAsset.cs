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

        public AtomLinkAsset(SyndicationLink link, SyndicationItem item)
        {
            this.link = link;
            this.item = item;
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

        public ulong ContentLength => Convert.ToUInt64(link.Length);

        public string Title => link.Title == null ? Path.GetFileName(link.Uri.AbsolutePath) : link.Title.ToString();

        public ResourceType ResourceType => ResourceType.Asset;

        public IReadOnlyList<string> Roles
        {
            get
            {
                List<string> roles = new List<string>() { link.RelationshipType };
                switch (link.RelationshipType)
                {
                    case "enclosure":
                        roles.Add("data");
                        break;
                    case "icon":
                        roles.Add("thumbnail");
                        break;
                }
                return roles;
            }
        }

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(link.Uri.AbsolutePath) };

        public IReadOnlyDictionary<string, object> Properties => link.AttributeExtensions.ToDictionary(k => k.Key.ToString(), k => k.Value as object);

    }
}