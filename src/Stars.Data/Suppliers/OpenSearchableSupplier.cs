using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Stac;
using Stac;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Data.Routers;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Model.Stac;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface;

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
            this.opensearchEngine = new OpenSearchEngine();
            this.opensearchEngine.RegisterExtension(new Terradue.OpenSearch.Engine.Extensions.AtomOpenSearchEngineExtension());
            this.opensearchEngine.RegisterExtension(new Terradue.OpenSearch.GeoJson.Extensions.FeatureCollectionOpenSearchEngineExtension());
            this.logger = logger;
            this.translatorManager = translatorManager;
        }

        public int Priority { get; set; }
        public string Key { get; set; }


        public virtual string Id => "OS" + nameof(openSearchable);

        public virtual string Label => "Generic data supplier for OpenSearch interfaces";

        public virtual async Task<IResource> SearchFor(IResource node)
        {
            return (IResource)new OpenSearchResultItemRoutable((await Query(node)).Items.FirstOrDefault(), new Uri("os://" + openSearchable.Identifier), logger);
        }

        public virtual async Task<IOpenSearchResultCollection> Query(IResource node)
        {
            // TEMP skipping catalog for the moment
            if (!(node is IItem)) return null;

            // Let's translate the node to STAC
            var stacNode = await translatorManager.Translate<StacNode>(node);

            IOpenSearchResultCollection results = null;

            if (stacNode == null)
            {
                results = await OpenSearchQuery(node);
            }
            else
            {
                results = await OpenSearchQuery(stacNode);
            }

            if (results == null || results.Count == 0)
                return null;
            // TODO manage more than first item
            return results;
        }

        protected virtual async Task<IOpenSearchResultCollection> OpenSearchQuery(IResource node)
        {
            if (!(node is IItem)) return null;
            NameValueCollection nvc = CreateOpenSearchParametersFromItem(node as IItem);
            if (nvc == null) return null;

            return await Task.Run<AtomFeed>(() => (AtomFeed)opensearchEngine.Query(openSearchable, nvc, typeof(AtomFeed)));
        }

        private NameValueCollection CreateOpenSearchParametersFromItem(IItem item)
        {
            string identifier = item.Id;

            NameValueCollection parameters = new NameValueCollection();

            parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", identifier);

            return parameters;
        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }
    }
}