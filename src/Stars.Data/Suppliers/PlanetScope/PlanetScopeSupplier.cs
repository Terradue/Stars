using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Stac;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Translator;
using Stac.Extensions.File;

namespace Terradue.Stars.Data.Suppliers.PlanetScope
{
    public class PlanetScopeSupplier : ISupplier
    {
        protected ILogger logger;
        private readonly TranslatorManager translatorManager;
        private ICredentials credentials;
        private string baseUrl;

        public PlanetScopeSupplier(ILogger<PlanetScopeSupplier> logger, TranslatorManager translatorManager, ICredentials credentials, IPluginOption pluginOption)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.credentials = credentials;
            SupplierPluginOption supplierPluginOption = pluginOption as SupplierPluginOption;
            this.baseUrl = supplierPluginOption.ServiceUrl;
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public string Id => "PlanetScope";

        public string Label => "PlanetScope product download";

        public async Task<IResource> SearchForAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            // TEMP skipping catalog for the moment
            if (!(node is IItem)) return null;

            // Let's translate the node to STAC
            var stacNode = await translatorManager.TranslateAsync<StacNode>(node, ct);
            if (stacNode == null || !(stacNode is StacItemNode)) return null;
            StacItemNode stacItemNode = stacNode as StacItemNode;

            string itemType = "PSScene";

            IDictionary<string, IAsset> assets = new Dictionary<string, IAsset>();

            logger.LogDebug($"Searching for {itemType} for product {stacItemNode.Id}");
            await AddAssets(itemType, stacItemNode);

            if (assets.Count == 0) return null;

            return stacItemNode;
        }

        private async Task AddAssets(string itemType, StacItemNode itemNode)
        {
            Uri uri = new Uri($"{baseUrl}/item-types/{itemType}/items/{itemNode.Id}/assets");

            HttpClientHandler handler = new HttpClientHandler() { Credentials = credentials };

            try
            {
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response =  await httpClient.GetAsync($"{baseUrl}/item-types/{itemType}/items/{itemNode.Id}/assets");
                    string content = await response.Content.ReadAsStringAsync();

                    Dictionary<string, ItemTypeInformation> itemTypes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, ItemTypeInformation>>(content);

                    


                }            
            }
            catch (Exception e)
            {
                throw;
            }


        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }
    }
}