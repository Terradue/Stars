using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
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

        public override IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsDirectory) continue;

                    assets.Add(entry.FileName, new ZipEntryAsset(entry, zipFile, asset));
                }
                return assets;
            }
        }

        public override System.Uri Uri => asset.Uri;

        public override string AutodetectSubfolder()
        {
            List<string> names = new List<string>();
            foreach (ZipEntry entry in zipFile)
            {
                names.Add(entry.FileName);
            }
            var commonfolder = Findstem(names.ToArray());
            if (commonfolder.IndexOf('/') > 1)
                return "";
            return Path.GetFileNameWithoutExtension(asset.Uri.ToString());
        }
    }
}