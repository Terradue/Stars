// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: DataHubResultItemRoutable.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Terradue.OpenSearch.DataHub;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Data.Suppliers;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Data.Routers
{
    public class DataHubResultItemRoutable : OpenSearchResultItemRoutable, IResource, IAssetsContainer
    {
        private readonly DataHubSourceSupplier supplier;

        public DataHubResultItemRoutable(IOpenSearchResultItem item, DataHubSourceSupplier supplier, Uri sourceUri, ILogger logger) : base(item, sourceUri, logger)
        {
            this.supplier = supplier;
        }

        private readonly Dictionary<string, IAsset> assets = null;

        public override IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                if (assets != null)
                    return assets;
                var enclosureAccess = supplier.Wrapper.GetEnclosureAccess(osItem);
                try
                {
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
                    logger.LogWarning("Exception trying to get assets for {0}: {1}", Id, e.Message);
                    logger.LogDebug(e.StackTrace);
                }

                if (osItem.Links != null)
                {
                    var icon = osItem.Links.FirstOrDefault(l => l.RelationshipType == "icon");
                    if (icon != null)
                    {
                        var resource = new GenericResource(icon.Uri);
                        var roles = new List<string>();
                        if (icon.Title != null && icon.Title.ToLower().Contains("thumbnail"))
                            roles.Add("thumbnail");
                        if (icon.Title != null && icon.Title.ToLower().Contains("browse"))
                            roles.Add("overview");
                        assets.Add("browse", new GenericAsset(resource, icon.Title, roles));
                    }
                }

                return assets;
            }

        }
    }
}
