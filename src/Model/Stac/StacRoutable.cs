using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal abstract class StacRoutable : IRoutable
    {
        protected IStacObject stacObject;
        protected ContentType contentType;

        public StacRoutable(IStacObject stacObject)
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

        public abstract IEnumerable<IRoute> GetRoutes();

        public abstract string ReadAsString();

        internal static IRoutable Create(IStacObject stacObject)
        {
            if (stacObject is IStacCatalog)
                return new StacCatalogRoutable(stacObject as IStacCatalog);

            if (stacObject is IStacItem)
                return new StacItemRoutable(stacObject as IStacItem);

            return null;
        }

        public Stream GetAsStream()
        {
            throw new NotImplementedException();
        }
    }
}