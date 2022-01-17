using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.OpenSearch.DataHub;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Data.Suppliers;
using Terradue.Stars.Interface;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Data.Routers
{
    public class DataHubResultItemRoutable : OpenSearchResultItemRoutable, IResource, IAssetsContainer
    {
        private readonly DataHubSourceSupplier supplier;

        public DataHubResultItemRoutable(IOpenSearchResultItem item, DataHubSourceSupplier supplier, Uri sourceUri, ILogger logger) : base(item, sourceUri, logger)
        {
            this.supplier = supplier;
        }


        public override IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                try
                {
                    var enclosureAccess = supplier.Wrapper.GetEnclosureAccess(osItem);
                    foreach (var dr in enclosureAccess.GetDownloadRequests())
                    {
                        string key = "download";
                        int i = 1;
                        while (assets.ContainsKey(key))
                        {
                            key = "download" + i;
                            i++;
                        }
                        assets.Add(key, new TransferRequestAsset(dr, enclosureAccess.Id + "." + i));
                    }
                }
                catch (ProductArchivingStatusException pase)
                {
                    return new List<IAsset>() {
                            new OrderableAsset(pase.Item, supplier)
                        }.ToDictionary(tr => "order", tr => tr);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Exception trying to get assets for {0}: {1}", this.Id, e.Message);
                }
                return assets;
            }

        }
    }
}