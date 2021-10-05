using System;
using System.Collections.Generic;
using System.Linq;
using Stac;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Utils
{
    public static class StacItemMerger
    {
        internal static StacItem Merge(IEnumerable<StacItem> items)
        {
            if (items == null || items.Count() == 0) return null;
            StacItem mergedItem = new StacItem(items.First());

            foreach (var item in items.Skip(1))
            {
                mergedItem = Merge(mergedItem, item);
            }

            return mergedItem;
        }

        private static StacItem Merge(StacItem item1, StacItem item2)
        {
            if (item1 == null || item2 == null) return null;
            StacItem mergedItem = new StacItem(item1);

            foreach (var link2 in item2.Links)
            {
                if (mergedItem.Links.Any(link => link.Uri.Equals(link2.Uri))) continue;
                mergedItem.Links.Add(link2);
            }
            mergedItem.MergeAssets(StacItemNode.CreateUnlocatedNode(item2));
            foreach (var prop in item2.Properties)
            {
                if ( prop.Key == "product_type")
                if (!mergedItem.Properties.ContainsKey(prop.Key))
                {
                    mergedItem.Properties.Add(prop.Key, prop.Value);
                }
            }

            return mergedItem;
        }
    }
}
