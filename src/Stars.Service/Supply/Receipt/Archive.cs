using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;

namespace Stars.Service.Supply.Receipt
{
    public abstract class Archive : IAssetsContainer
    {
        public static readonly Dictionary<string, ArchiveCompression> ArchiveFileExtensions = new Dictionary<string, ArchiveCompression>() {
            { ".tar.gz", ArchiveCompression.Gzip },
            { ".tgz",  ArchiveCompression.Gzip },
            { ".tar.Z",   ArchiveCompression.Gzip },
            { ".tar.bz2",  ArchiveCompression.Bzip2 },
            { ".tbz2",   ArchiveCompression.Bzip2 },
            { ".tar.lz",   ArchiveCompression.Lzip },
            { ".tlz",   ArchiveCompression.Lzip },
            {".tar.xz",   ArchiveCompression.Xz },
            {".txz",  ArchiveCompression.Xz },
            {".zip",   ArchiveCompression.Zip },
            {".zipx",   ArchiveCompression.Zip }
        };

        public static readonly string[] ArchiveContentTypes = {
            "application/x-gtar",
            "application/zip"
        };

        internal async static Task<Archive> Read(IAsset asset)
        {
            IStreamable streamableAsset = asset.GetStreamable();

            if ( streamableAsset == null )
                throw new System.IO.InvalidDataException("Asset must be streamable to be read as an archive");

            ArchiveCompression compression = FindCompression(asset);
            
            switch (compression){
                case ArchiveCompression.Zip:
                    var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(await streamableAsset.GetStreamAsync());
                    return new ZipArchiveAsset(zipFile, asset);
                default:
                    throw new System.IO.InvalidDataException("Asset is not recognized as an archive");
            }
        }

        private static ArchiveCompression FindCompression(IAsset asset)
        {
            return ArchiveFileExtensions[Path.GetExtension(asset.Uri.ToString())];
        }

        public abstract IDictionary<string, IAsset> GetAssets();

    }
}