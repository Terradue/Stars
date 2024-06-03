using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class VolumeDimapProfiler : AirbusProfiler
    {
        private readonly AirbusMetadataExtractor airbusMetadataExtractor;

        public VolumeDimapProfiler(Dimap_Document dimap, AirbusMetadataExtractor airbusMetadataExtractor) : base(dimap)
        {
            this.airbusMetadataExtractor = airbusMetadataExtractor;
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders => throw new NotImplementedException();

        public override string GetPlatformInternationalDesignator()
        {
            return null;
        }

        internal async Task<IEnumerable<StacItemNode>> GetDatasets(IItem item, string suffix)
        {
            IEnumerable<IAsset> metadataAssets = Dimap.Dataset_Content.Dataset_Components.Component
                                        .Where(c => c.COMPONENT_TYPE == "DIMAP")
                                        .Select(c => airbusMetadataExtractor.FindFirstAssetFromFilePathRegex(item, @".*" + c.COMPONENT_PATH.Href))
                                        .Where(a => a != null);

            List<StacItemNode> stacNodes = new List<StacItemNode>();

            foreach (var metadataAsset in metadataAssets)
            {
                Dimap_Document metadata = await airbusMetadataExtractor.ReadMetadata(metadataAsset);
                AirbusProfiler dimapProfiler = airbusMetadataExtractor.GetProfiler(metadata);
                StacItemNode stacNode = airbusMetadataExtractor.ExtractMetadata(item, dimapProfiler, metadataAsset, suffix);
                stacNodes.Add(stacNode);
            }

            return stacNodes;

        }
    }
}
