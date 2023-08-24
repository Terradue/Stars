// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacItemMerger.cs

using System.Collections.Generic;
using System.Linq;
using Stac;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Utils
{
    public static class StacItemMerger
    {
        internal static StacItem Merge(IEnumerable<StacItemNode> items)
        {
            if (items == null || items.Count() == 0) return null;
            StacItem mergedItem = new StacItem(items.First().StacItem);

            foreach (var nextItem in items.Skip(1))
            {
                mergedItem = Merge(mergedItem, nextItem);
            }

            return mergedItem;
        }

        private static StacItem Merge(StacItem item1, StacItemNode item2)
        {
            if (item1 == null || item2 == null) return null;
            StacItem mergedItem = new StacItem(item1);

            foreach (var link2 in item2.StacItem.Links)
            {
                if (mergedItem.Links.Any(link => link.Uri.Equals(link2.Uri))) continue;
                mergedItem.Links.Add(link2);
            }
            mergedItem.MergeAssets(item2);
            foreach (var prop in item2.Properties)
            {
                if (prop.Key == "product_type")
                    if (!mergedItem.Properties.ContainsKey(prop.Key))
                    {
                        mergedItem.Properties.Add(prop.Key, prop.Value);
                    }
            }

            return mergedItem;
        }
    }
}
