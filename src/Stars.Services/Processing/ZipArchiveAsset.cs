using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Processing
{
    internal class ZipArchiveAsset : Archive
    {
        private ZipFile zipFile;
        private readonly IAsset asset;

        public ZipArchiveAsset(ZipFile zipFile, IAsset asset)
        {
            this.zipFile = zipFile;
            this.asset = asset;
        }

        public override IDictionary<string, IAsset> GetAssets()
        {
            Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
            foreach (ZipEntry entry in zipFile)
            {
                if (!entry.IsFile) continue;

                assets.Add(entry.Name, new ZipEntryAsset(entry, zipFile, asset));
            }
            return assets;
        }
    }
}