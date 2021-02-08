using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;


namespace Terradue.Stars.Services.Processing
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

            if (streamableAsset == null)
                throw new System.IO.InvalidDataException("Asset must be streamable to be read as an archive");

            ArchiveCompression compression = FindCompression(asset);

            switch (compression)
            {
                case ArchiveCompression.Zip:
                    var zipFile = Ionic.Zip.ZipFile.Read(await streamableAsset.GetStreamAsync());
                    return new ZipArchiveAsset(zipFile, asset);
                default:
                    throw new System.IO.InvalidDataException("Asset is not recognized as an archive");
            }
        }

        private static ArchiveCompression FindCompression(IAsset asset)
        {
            return ArchiveFileExtensions[Path.GetExtension(asset.Uri.ToString())];
        }

        public abstract IReadOnlyDictionary<string, IAsset> Assets { get; }

        public abstract Uri Uri { get; }

        public abstract string AutodetectSubfolder();

        protected static String Findstem(String[] arr)
        {
            // Determine size of the array  
            int n = arr.Length;

            // Take first word from array as reference  
            String s = arr[0];
            int len = s.Length;

            String res = "";

            for (int i = 0; i < len; i++)
            {
                for (int j = i + 1; j <= len; j++)
                {

                    // generating all possible substrings  
                    // of our reference string arr[0] i.e s  
                    String stem = s.Substring(i, j - i);
                    int k = 1;
                    for (k = 1; k < n; k++)

                        // Check if the generated stem is  
                        // common to to all words  
                        if (!arr[k].Contains(stem))
                            break;

                    // If current substring is present in  
                    // all strings and its length is greater  
                    // than current result  
                    if (k == n && res.Length < stem.Length)
                        res = stem;
                }
            }

            return res;
        }
    }
}