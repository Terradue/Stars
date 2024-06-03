// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GenericCatalog.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class GenericCatalog : ICatalog
    {
        private readonly IEnumerable<IResource> routes;
        private readonly string id;

        public GenericCatalog(IEnumerable<IResource> routes, string id)
        {
            this.routes = routes;
            this.id = id;
        }

        public string Title => id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => new Uri("stars://catalog");

        public ResourceType ResourceType => ResourceType.Catalog;

        public string Id => id;

        public string Filename => Id;

        public ulong ContentLength => 0;

        public bool IsCatalog => true;

        public ContentDisposition ContentDisposition => null;

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return new List<IResourceLink>();
        }

        public IReadOnlyList<IResource> GetRoutes(IRouter router)
        {
            return routes.ToList();
        }

    }
}
