using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public abstract class StacNode : IResource, ITransactableResource, IStreamable, ILocatable
    {
        protected IStacObject stacObject;
        protected readonly ICredentials credentials;
        private Uri uri;

        protected StacNode(IStacObject stacObject, Uri uri = null, ICredentials credentials = null)
        {
            if (stacObject == null)
                throw new ArgumentNullException("stacObject");
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("STAC uri must be an absolute uri");
            this.stacObject = stacObject;
            this.credentials = credentials;
            this.uri = uri == null ? new Uri(Id + ".json", UriKind.Relative) : uri;
        }

        public string Label => stacObject.Id;

        public ContentType ContentType => stacObject.MediaType;

        public Uri Uri { get => uri; set => uri = value; }

        public abstract ResourceType ResourceType { get; }

        public string Id => stacObject.Id.CleanIdentifier();

        public virtual ulong ContentLength
        {
            get
            {
                MemoryStream ms = (MemoryStream)GetStreamAsync().GetAwaiter().GetResult();
                return Convert.ToUInt64(ms.Length);
            }
        }

        public bool IsCatalog => (stacObject is IStacCatalog);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = FileName };

        private string filename = null;
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(filename))
                    return filename;
                if (ResourceType == ResourceType.Catalog)
                    return "catalog.json";
                return Id + ".json";
            }
            set
            {
                filename = value;
            }
        }

        public static StacNode Create(IStacObject stacObject, Uri uri)
        {
            if (stacObject is IStacCatalog)
                return new StacCatalogNode(stacObject as IStacCatalog, uri);

            if (stacObject is StacItem)
                return new StacItemNode(stacObject as StacItem, uri);

            return null;
        }

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return stacObject.Links.Select(l => new StacResourceLink(l)).ToList();
        }


        public IStacObject StacObject => stacObject;

        public bool CanBeRanged => false;

        public virtual IResourceLink Parent {
            get {
                var parentLink = StacObject.Links.FirstOrDefault(l => l.RelationshipType == "parent");
                if ( parentLink != null ){
                    return new StacResourceLink(parentLink);
                }
                return null;
            }
        }

        public abstract IReadOnlyList<IResource> GetRoutes(ICredentials credentials);

        public async Task<Stream> GetStreamAsync()
        {
            MemoryStream ms = new MemoryStream();
            return await Task<Stream>.Run(() =>
            {
                var sw = new StreamWriter(ms);
                sw.Write(StacConvert.Serialize(stacObject));
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            });

        }

        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }

        public abstract object Clone();
    }
}