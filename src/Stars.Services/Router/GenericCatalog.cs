using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class GenericCatalog : ICatalog
    {
        private readonly IEnumerable<IRoute> routes;
        private readonly string id;

        public GenericCatalog(IEnumerable<IRoute> routes, string id)
        {
            this.routes = routes;
            this.id = id;
        }

        public string Label => id;

        public ContentType ContentType => new ContentType("application/catalog");

        public Uri Uri => new Uri("stars://catalog");

        public ResourceType ResourceType => ResourceType.Catalog;

        public string Id => id;

        public string Filename => Id;

        public ulong ContentLength => 0;

        public bool IsCatalog => true;

        public ContentDisposition ContentDisposition => null;

        public IList<IRoute> GetRoutes()
        {
            return routes.ToList();
        }

    }
}