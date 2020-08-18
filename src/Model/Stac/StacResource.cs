using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal abstract class StacResource : IResource
    {
        protected IStacObject stacObject;
        protected ContentType contentType;

        public StacResource(IStacObject stacObject)
        {
            this.stacObject = stacObject;
            this.contentType = new ContentType("application/json");
        }

        public string Label => stacObject.Id;

        public ContentType ContentType => contentType;

        public Uri Uri => stacObject.Uri;

        public abstract ResourceType ResourceType { get; }

        public string Id => stacObject.Id.CleanIdentifier();

        public abstract string Filename { get; }

        public abstract string ReadAsString();

        internal static StacResource Create(IStacObject stacObject)
        {
            if (stacObject is IStacCatalog)
                return new StacCatalogResource(stacObject as IStacCatalog);

            if (stacObject is IStacItem)
                return new StacItemResource(stacObject as IStacItem);

            return null;
        }

        public Stream GetAsStream()
        {
            throw new NotImplementedException();
        }

        public IStacObject ReadAsStacObject()
        {
            throw new NotImplementedException();
        }

        public Task<IResource> GotoResource()
        {
            return Task.FromResult((IResource)this);
        }
    }
}