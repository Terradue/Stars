using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stac;
using Terradue.Stars.Services.ThirdParty.Titiler;

namespace Terradue.Data.Tests
{
    internal class TestVectorService : IVectorService
    {
        public bool IsAvailable => true;

        public Uri BuildServiceUri(Uri stacItemUri, KeyValuePair<string, StacAsset> vectorAsset)
        {
            return new Uri(Path.Combine("https://example.com/s3g", vectorAsset.Value.Uri.IsAbsoluteUri ? vectorAsset.Value.Uri.LocalPath : vectorAsset.Value.Uri.ToString()));
        }

        public IDictionary<string, StacAsset> SelectVectorAssets(StacItem stacItem)
        {
            Dictionary<string, StacAsset> vectorAssets = stacItem.Assets
                                                            .Where(a => a.Value.Roles.Contains("visual"))
                                                            .OrderBy(a => a.Value.GetProperty<long>("size"))
                                                            .Where(a => a.Value.MediaType.MediaType.Contains("application/flatgeobuf") 
                                                                        || a.Value.MediaType.MediaType.Contains("application/geo+json"))
                                                            .ToDictionary(a => a.Key, a => a.Value);
            return vectorAssets;
        }
    }
}