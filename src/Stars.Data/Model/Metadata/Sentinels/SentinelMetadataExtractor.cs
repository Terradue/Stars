using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels
{
    public abstract class SentinelMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer xfduSerializer = new XmlSerializer(typeof(XFDUType));

        protected SentinelMetadataExtractor(ILogger logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset manifestAsset = GetManifestAsset(item);
                XFDUType manifest = ReadManifest(manifestAsset).GetAwaiter().GetResult();
                string identifier = null;
                Match match = Regex.Match(manifestAsset.Uri.ToString(), @"(.*\/)*(?'identifier'S(1|2|3)[^\.\/]{10,})(\.\w+)*(\/.*)*");
                if (match.Success)
                    identifier = match.Groups["identifier"].Value;
                else return false;
                SentinelSafeStacFactory stacFactory = CreateSafeStacFactory(manifest, item, identifier);
                return stacFactory != null;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset manifestAsset = GetManifestAsset(item);

            string identifier = null;
            Match match = Regex.Match(manifestAsset.Uri.ToString(), @"(.*\/)*(?'identifier'S(1|2|3)[^\.\/]{10,})(\.\w+)*(\/.*)*");
            if (match.Success)
                identifier = match.Groups["identifier"].Value;
            else
                identifier = item.Id;
            identifier += suffix;

            XFDUType manifest = await ReadManifest(manifestAsset);

            SentinelSafeStacFactory stacFactory = CreateSafeStacFactory(manifest, item, identifier);
            StacItem stacItem = stacFactory.CreateStacItem();
            await AddAssets(stacItem, item, stacFactory);

            // AddEoBandPropertyInItem(stacItem);

            var stacNode = StacItemNode.Create(stacItem, item.Uri);

            return stacNode;
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            if (eo == null) return;
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        protected abstract Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory);

        protected virtual IAsset GetManifestAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, "manifest.safe$");
            if (manifestAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the manifest SAFE file asset"));
            }
            return manifestAsset;
        }
        protected abstract SentinelSafeStacFactory CreateSafeStacFactory(XFDUType manifest, IItem item, string identifier);

        public virtual async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await manifestAsset.GetStreamable().GetStreamAsync())
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (XFDUType)xfduSerializer.Deserialize(reader);
            }
        }


        protected KeyValuePair<string, StacAsset> CreateManifestAsset(IStacObject stacObject, IAsset asset)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacObject, asset.Uri, new ContentType("text/xml"), "SAFE Manifest");
            stacAsset.Properties.AddRange(asset.Properties);
            return new KeyValuePair<string, StacAsset>("manifest", stacAsset);
        }
    }
}
