using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stac;
using Stac.Api.Interfaces;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Data.Suppliers.PlanetScope
{
    public class PlanetScopeSupplier : ISupplier
    {
        protected ILogger logger;
        private readonly TranslatorManager translatorManager;
        private ICredentials credentials;
        private string baseUrl;

        // These are the asset types to be supported initially
        public static Dictionary<string, AssetTypeInformation> assetTypes = new Dictionary<string, AssetTypeInformation>()
        {
            { "ortho_analytic_3b", new AssetTypeInformation("Orthorectified top of atmosphere radiance (3 Band)", "data", "image/tiff") },
            { "ortho_analytic_3b_xml", new AssetTypeInformation("Orthorectified top of atmosphere radiance (3 Band) metadata", "metadata", "application/xml") },
            { "ortho_analytic_4b", new AssetTypeInformation("Orthorectified top of atmosphere radiance (4 Band)", "data", "image/tiff") },
            { "ortho_analytic_4b_xml", new AssetTypeInformation("Orthorectified atmospherically corrected surface reflectance (4 Band) metadata", "metadata", "application/xml") },
            { "ortho_visual", new AssetTypeInformation("Orthorectified color corrected visual image product (3 Band)", "data", "image/tiff") },
        };

        public PlanetScopeSupplier(ILogger<PlanetScopeSupplier> logger, TranslatorManager translatorManager, ICredentials credentials, IPluginOption pluginOption)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.credentials = credentials;
            SupplierPluginOption supplierPluginOption = pluginOption as SupplierPluginOption;
            baseUrl = supplierPluginOption.ServiceUrl;
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

            logger.LogDebug($"Searching for {itemType} for product {stacItemNode.Id}");
            await AddAssets(itemType, stacItemNode);

            //if (stacItemNode.Assets.Count == 0) return null;

            return stacItemNode;
        }

        private async Task AddAssets(string itemType, StacItemNode itemNode)
        {
            HttpClientHandler handler = new HttpClientHandler() { Credentials = credentials };

            try
            {
                string content = string.Empty;
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    // The API Base URL is https://api.planet.com/data/v1
                    var request = new HttpRequestMessage() { RequestUri = new Uri($"{baseUrl}/item-types/{itemType}/items/{itemNode.Id}/assets") };

                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    content = await response.Content.ReadAsStringAsync();

                    if (content.StartsWith(@"{""general"":"))
                    {
                        if (content.Contains("Could not find the requested item"))
                        {
                            throw new Exception($"Item not found: {itemNode.Id}");
                        }
                        GeneralMessageObject error = JsonConvert.DeserializeObject<GeneralMessageObject>(content);
                        if (error.Messages != null && error.Messages.Length != 0)
                        {
                            throw new Exception(error.Messages[0].Message);
                        }
                        throw new Exception($"Error during asset retrieval for {itemNode.Id}");
                    }
                    else if (content.StartsWith(@"{""message"":"))
                    {
                        if (content.Contains("Please enter a valid API key"))
                        {
                            throw new UnauthorizedAccessException("Invalid API key");
                        }
                        MessageObject error = JsonConvert.DeserializeObject<MessageObject>(content);
                        throw new Exception(error.Message);
                    }

                    Dictionary<string, AssetInformation> itemAssets = JsonConvert.DeserializeObject<Dictionary<string, AssetInformation>>(content);

                    Dictionary<string, StacAsset> assets = new Dictionary<string, StacAsset>();

                    foreach (KeyValuePair<string, AssetInformation> kvp in itemAssets)
                    {
                        string assetTypeId = kvp.Key;
                        AssetInformation assetInformation = kvp.Value;

                        // Ignore if asset type is not required
                        if (!assetTypes.ContainsKey(assetTypeId)) continue;

                        AssetTypeInformation assetType = assetTypes[assetTypeId];

                        // Ignore if no download permission
                        if (!assetInformation.Permissions.Contains("download")) continue;

                        string url = (assetInformation.Status == "active" ? assetInformation.Location : null);
                        assets.Add(
                            assetTypeId,
                            new StacAsset(
                                itemNode.StacItem,
                                new Uri(url),
                                new string[] { assetType.Role },
                                assetType.Title,
                                new System.Net.Mime.ContentType(assetType.ContentType)
                            )
                        );
                    }

                    // Add all assets
                    foreach (KeyValuePair<string, StacAsset> kvp in assets)
                    {
                        itemNode.StacItem.Assets.Add(
                            kvp.Key,
                            kvp.Value
                        );
                    }

                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string errorContent = (new StreamReader(e.Response.GetResponseStream())).ReadToEnd();
                    try
                    {
                        MessageObject error = JsonConvert.DeserializeObject<MessageObject>(errorContent);
                        throw new Exception(error.Message);
                    }
                    catch (Exception)
                    {
                        logger.LogDebug(errorContent);
                        throw new Exception($"Error during asset retrieval for {itemNode.Id}");
                    }
                }
            }
        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }

        public Task<IItemCollection> SearchForAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<object> InternalSearchExpressionAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }


    public class AssetTypeInformation
    {
        public string Title { get; set; }
        public string Role { get; set; }
        public string ContentType { get; set; }

        public AssetTypeInformation(string title, string role, string contentType)
        {
            Title = title;
            Role = role;
            ContentType = contentType;
        }
    }


}
