// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: OpenSearchableSupplier.cs

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Data.Routers;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Data.Suppliers
{
    public class OpenSearchableSupplier : ISupplier
    {
        protected IOpenSearchable openSearchable;
        protected TranslatorManager translatorManager;
        protected ILogger logger;
        protected OpenSearchEngine opensearchEngine;

        public OpenSearchableSupplier(ILogger logger, TranslatorManager translatorManager)
        {
            opensearchEngine = new OpenSearchEngine();
            opensearchEngine.RegisterExtension(new OpenSearch.Engine.Extensions.AtomOpenSearchEngineExtension());
            opensearchEngine.RegisterExtension(new OpenSearch.GeoJson.Extensions.FeatureCollectionOpenSearchEngineExtension());
            this.logger = logger;
            this.translatorManager = translatorManager;
        }

        public int Priority { get; set; }
        public string Key { get; set; }


        public virtual string Id => "OS" + nameof(openSearchable);

        public virtual string Label => "Generic data supplier for OpenSearch interfaces";

        public virtual async Task<IResource> SearchForAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            return (IResource)new OpenSearchResultItemRoutable((await QueryAsync(node, ct, identifierRegex)).Items.FirstOrDefault(), new Uri("os://" + openSearchable.Identifier), logger);
        }

        public virtual async Task<IOpenSearchResultCollection> QueryAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            // TEMP skipping catalog for the moment
            if (!(node is IItem)) return null;

            // Let's translate the node to STAC
            var stacNode = await translatorManager.TranslateAsync<StacNode>(node, ct);

            IOpenSearchResultCollection results = null;

            if (stacNode == null)
            {
                results = await OpenSearchQuery(node, identifierRegex);
            }
            else
            {
                results = await OpenSearchQuery(stacNode, identifierRegex);
            }

            if (results == null || results.Count == 0)
                return null;
            // TODO manage more than first item
            return results;
        }

        protected virtual async Task<IOpenSearchResultCollection> OpenSearchQuery(IResource node, string identifierRegex = null)
        {
            if (!(node is IItem)) return null;
            NameValueCollection nvc = CreateOpenSearchParametersFromItem(node as IItem, identifierRegex);
            if (nvc == null) return null;

            return await Task.Run(() => (AtomFeed)opensearchEngine.Query(openSearchable, nvc, typeof(AtomFeed)));
        }

        private NameValueCollection CreateOpenSearchParametersFromItem(IItem item, string identifierRegex = null)
        {
            string identifier = item.Id;
            NameValueCollection parameters = new NameValueCollection();
            if (identifierRegex != null)
            {
                Regex regex = new Regex(identifierRegex);
                var match = regex.Match(identifier);
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    foreach (string groupName in regex.GetGroupNames())
                    {
                        switch (groupName)
                        {
                            case "id":
                            case "uid":
                                parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", groups[groupName].Value);
                                break;
                            default:
                                parameters.Set(groupName, groups[groupName].Value);
                                break;
                        }
                    }
                    return parameters;
                }
            }

            parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", identifier);

            // USGS: Setting the collection to parentIdentifier parameter
            if (item is StacItemNode)
            {
                var stacItem = item as StacItemNode;
                if (stacItem.StacItem.StacExtensions.Any(e => e.Contains("landsat")) && stacItem.StacItem.GetProperty<string>("landsat:collection_category") != null)
                {
                    if (stacItem.StacItem.GetProperty<string>("landsat:collection_category").StartsWith("T")
                        && stacItem.StacItem.GetProperty<string>("landsat:collection_category") != "C2")
                    {
                        if (stacItem.StacItem.GetProperty<string>("landsat:correction").StartsWith("L1"))
                            parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", "landsat_ot_c2_l1");
                        if (stacItem.StacItem.GetProperty<string>("landsat:correction").StartsWith("L2"))
                            parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", "landsat_ot_c2_l2");
                    }
                }
            }

            return parameters;
        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }
    }
}
