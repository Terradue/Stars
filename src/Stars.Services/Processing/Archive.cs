using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Services.Processing
{
    public abstract class Archive : ILocatable
    {
        public static readonly Dictionary<string, ArchiveType> ArchiveFileExtensions = new Dictionary<string, ArchiveType>() {
            { ".tar.gz", ArchiveType.TarGzip },
            { ".tgz",  ArchiveType.TarGzip },
            { ".tar.Z",   ArchiveType.TarGzip },
            { ".tar.bz2",  ArchiveType.TarBzip2 },
            { ".tbz2",   ArchiveType.TarBzip2 },
            { ".tar.lz",   ArchiveType.TarLzip },
            { ".tlz",   ArchiveType.TarLzip },
            { ".gz",   ArchiveType.Gzip },
            {".tar.xz",   ArchiveType.TarXz },
            {".txz",  ArchiveType.TarXz },
            {".zip",   ArchiveType.Zip },
            {".zipx",   ArchiveType.Zip }
        };

        public static readonly string[] ArchiveContentTypes = {
            "application/x-gtar",
            "application/x-gzip",
            "application/zip"
        };

        internal async static Task<Archive> Read(IAsset asset, ILogger logger)
        {
            IStreamable streamableAsset = asset.GetStreamable();

            if (streamableAsset == null)
                throw new System.IO.InvalidDataException("Asset must be streamable to be read as an archive");

            ArchiveType compression = FindCompression(asset);

            switch (compression)
            {
                case ArchiveType.Zip:
                    return new ZipArchiveAsset(asset, logger);
                case ArchiveType.TarGzip:
                    return new TarGzipArchive(asset, logger);
                case ArchiveType.Gzip:
                    return new GzipArchive(asset, logger);

                default:
                    throw new System.IO.InvalidDataException("Asset is not recognized as an archive");
            }
        }

        private static ArchiveType FindCompression(IAsset asset)
        {
            return ArchiveFileExtensions.FirstOrDefault(ext => asset.ContentDisposition.FileName.EndsWith(ext.Key)).Value;
        }

        public abstract Uri Uri { get; }

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

        internal abstract Task<IAssetsContainer> ExtractToDestination(IDestination destination, CarrierManager carrierManager);
    }
}