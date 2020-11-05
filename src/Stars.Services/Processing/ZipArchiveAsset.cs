using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Terradue.Stars.Interface.Router;
using System.Linq;
using System.IO;
using Terradue.Stars.Interface;

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

        public override string AutodetectSubfolder()
        {
            List<string> names = new List<string>();
            foreach (ZipEntry entry in zipFile)
            {
                names.Add(entry.Name);
            }
            var commonfolder = Findstem(names.ToArray());
            if ( commonfolder.IndexOf('/') > 1 )
                return "";
            return Path.GetFileNameWithoutExtension(asset.Uri.ToString());
        }
    }
}