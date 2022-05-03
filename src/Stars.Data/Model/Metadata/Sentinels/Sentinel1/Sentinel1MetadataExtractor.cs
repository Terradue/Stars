using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1
{
    public class Sentinel1MetadataExtractor : SentinelMetadataExtractor
    {
        public override string Label => "Sentinel-1 (ESA) mission product metadata extractor";

        public Sentinel1MetadataExtractor(ILogger<Sentinel1MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            XFDUType xfdu = await base.ReadManifest(manifestAsset);
            if (!xfdu.informationPackageMap.contentUnit[0].textInfo.StartsWith("Sentinel-1"))
            {
                throw new FormatException(String.Format("Not a Sentinel-1 manifest SAFE file asset"));
            }
            return xfdu;
        }

        protected async override Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            foreach (var bandAsset in FindAllAssetsFromFileNameRegex(item, "\\.tiff$"))
            {
                string filename = Path.GetFileNameWithoutExtension(bandAsset.Value.Uri.ToString());
                IAsset annotationAsset = FindFirstAssetFromFileNameRegex(item, @"^(.*\/+|)" + filename + @"\.xml$");
                if (annotationAsset == null)
                    throw new FileNotFoundException(string.Format("No XML annotation found for S1 tiff asset '{0}'", bandAsset.Key));
                var bandStacAsset = await AddBandAsset(stacItem, bandAsset.Value, annotationAsset, stacFactory);
                var annotationStacAsset = await AddAnnotationAsset(stacItem, annotationAsset, stacFactory);
            }

            foreach (var calibrationAsset in FindAllAssetsFromFileNameRegex(item, @"^(.*\/+|)calibration.*\.xml$"))
            {
                var calibrationStacAsset = await AddCalibrationAsset(stacItem, calibrationAsset.Value, stacFactory);
            }

            foreach (var noiseAsset in FindAllAssetsFromFileNameRegex(item, @"^(.*\/+|)noise.*\.xml$"))
            {
                var noiseStacAsset = await AddNoiseAsset(stacItem, noiseAsset.Value, stacFactory);
            }

            stacItem.Assets.Add("manifest", CreateManifestAsset(stacItem, GetManifestAsset(item)).Value);

        }

        private string AddOverviewAsset(StacItem stacItem, string key, IItem item, SentinelSafeStacFactory stacFactory, ContentType contentType = null)
        {
            var asset = item.Assets[key];
            string filename = Path.GetFileNameWithoutExtension(asset.Uri.ToString()).ToLower();

            if (contentType == null)
            {
                contentType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()));
            }

            StacAsset stacAsset = StacAsset.CreateOverviewAsset(stacItem, asset.Uri, contentType);
            stacAsset.Properties.AddRange(asset.Properties);
            stacItem.Assets.Add(filename, stacAsset);
            return filename;
        }

        private async Task<string> AddBandAsset(StacItem stacItem, IAsset bandAsset, IAsset annotationAsset, SentinelSafeStacFactory stacFactory)
        {
            SentinelAssetFactory assetFactory = null;

            if (stacFactory.GetProductType() == "OCN")
                assetFactory = await S1L2AssetProduct.CreateData(annotationAsset, resourceServiceProvider);
            else
                assetFactory = await S1L1AssetProduct.Create(annotationAsset, bandAsset, resourceServiceProvider);


            stacItem.Assets.Add(assetFactory.GetId(), assetFactory.CreateDataAsset(stacItem));
            return assetFactory.GetId();
        }

        private async Task<string> AddAnnotationAsset(StacItem stacItem, IAsset annotationAsset, SentinelSafeStacFactory stacFactory)
        {
            SentinelAssetFactory assetFactory = null;

            if (stacFactory.GetProductType() == "OCN")
                assetFactory = await S1L2AssetProduct.CreateData(annotationAsset, resourceServiceProvider);
            else
                assetFactory = await S1L1AssetProduct.Create(annotationAsset, null, resourceServiceProvider);

            stacItem.Assets.Add(assetFactory.GetAnnotationId(), assetFactory.CreateMetadataAsset(stacItem));
            return assetFactory.GetAnnotationId();
        }

        private async Task<string> AddCalibrationAsset(StacItem stacItem, IAsset asset, SentinelSafeStacFactory stacFactory)
        {
            SentinelAssetFactory assetFactory = null;

            if (stacFactory.GetProductType() == "OCN")
                assetFactory = await S1L2AssetProduct.CreateData(asset, resourceServiceProvider);
            else
                assetFactory = await Calibration.S1L1AssetCalibration.Create(asset, resourceServiceProvider);

            stacItem.Assets.Add(assetFactory.GetId(), assetFactory.CreateDataAsset(stacItem));
            return assetFactory.GetId();
        }

        private async Task<string> AddNoiseAsset(StacItem stacItem, IAsset asset, SentinelSafeStacFactory stacFactory)
        {
            SentinelAssetFactory assetFactory = null;

            if (stacFactory.GetProductType() == "OCN")
                assetFactory = await S1L2AssetProduct.CreateData(asset, resourceServiceProvider);
            else
                assetFactory = await Noise.S1L1AssetNoise.Create(asset, resourceServiceProvider);

            stacItem.Assets.Add(assetFactory.GetId(), assetFactory.CreateDataAsset(stacItem));
            return assetFactory.GetId();
        }

        protected override SentinelSafeStacFactory CreateSafeStacFactory(XFDUType manifest, IItem item, string identifier)
        {
            return S1SafeStacFactory.Create(manifest, item, identifier);
        }
    }
}
