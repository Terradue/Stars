using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Processing
{
    internal class ZipEntryAsset : IAsset, IStreamResource
    {
        private ZipEntry entry;
        private readonly ZipFile zipFile;
        private readonly IAsset parentAsset;

        public ZipEntryAsset(ZipEntry entry, ZipFile zipFile, IAsset parentAsset)
        {
            this.entry = entry;
            this.zipFile = zipFile;
            this.parentAsset = parentAsset;
        }

        public string Title => parentAsset.Title + " / " + entry.FileName;

        public IReadOnlyList<string> Roles
        {
            get
            {
                var ext = Path.GetExtension(entry.FileName).TrimStart('.'); 
                if ( (new string[]{ "txt", "xml", "json" }).Contains(ext)) return new string[] { "metadata" };
                return new string[] { "data" };
            }
        }

        public Uri Uri => new Uri(entry.FileName, UriKind.Relative);

        public ContentType ContentType => new ContentType(MimeTypes.GetMimeType(entry.FileName));

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => Convert.ToUInt64(entry.UncompressedSize);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = entry.FileName };

        public bool CanBeRanged => false;

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("filename", entry.FileName);
                return props;
            }
        }

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task.FromResult(entry.OpenReader() as Stream);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Zip Entry {0}", entry.FileName);
        }
    }
}