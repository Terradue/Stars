// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacNavigationHelpers.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;

namespace Stars.Services.Model.Stac
{
    public static class StacNavigationHelpers
    {

        public static IEnumerable<StacCatalogNode> GetChildren(this StacCatalogNode stacCatalog, StacRouter router)
        {
            return GetChildrenAsync(stacCatalog, router, CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<StacCatalogNode>> GetChildrenAsync(this StacCatalogNode catalog, StacRouter router, CancellationToken ct)
        {
            List<StacCatalogNode> children = new List<StacCatalogNode>();
            foreach (var childLink in catalog.GetLinks().Where(l => !string.IsNullOrEmpty(l.Relationship) && l.Relationship == "child"))
            {
                IResource childRoute = await router.RouteLinkAsync(catalog, childLink, ct);
                children.Add(childRoute as StacCatalogNode);
            }
            return children;
        }

        public static IEnumerable<StacItemNode> GetItems(this StacCatalogNode stacCatalog, StacRouter router, bool throwOnError = false)
        {
            return stacCatalog.GetLinks().Where(l => !string.IsNullOrEmpty(l.Relationship) && l.Relationship == "item")
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Select(itemLink =>
                {
                    try
                    {
                        return router.RouteLinkAsync(stacCatalog, itemLink, CancellationToken.None).GetAwaiter().GetResult() as StacItemNode;
                    }
                    catch (Exception)
                    {
                        if (throwOnError)
                            throw;
                    }
                    return null;
                }).Where(i => i != null);
        }

    }
}
