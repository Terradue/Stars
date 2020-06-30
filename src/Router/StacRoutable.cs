using System;
using System.Collections.Generic;
using System.Net.Mime;
using Stac;
using Stac.Catalog;
using Stac.Item;

namespace Stars.Router
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

        public abstract IEnumerable<IRoute> GetRoutes();

        public abstract string ReadAsString();

        internal static IRoutable Create(IStacObject stacObject)
        {
            if ( stacObject is IStacCatalog )
                return new StacCatalogRoutable(stacObject as IStacCatalog);

            if ( stacObject is IStacItem )
                return new StacItemRoutable(stacObject as IStacItem);

            return null;
        }
    }
}