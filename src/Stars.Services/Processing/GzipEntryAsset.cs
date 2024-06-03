using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Processing
{
    public class GzipEntryAsset : IAsset, IStreamResource
    {
        private string name;
        private Stream inputStream;

        public GzipEntryAsset(string name, Stream inputStream)
        {
            this.name = name;
            this.inputStream = inputStream;
        }

        public string Name => name;

        public string Title => name;

        public IReadOnlyList<string> Roles => new string[] { "data" };

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("filename", name);
                return props;
            }
        }

        public ContentType ContentType
        {
            get
            {
                string mediaType = MimeTypes.GetMimeType(name);
                return new ContentType(mediaType);
            }
        }

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength
        {
            get
            {
                try { return Convert.ToUInt64(inputStream.Length); }
                catch { return 0; }
            }
        }

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(name) };

        public Uri Uri => new Uri(name, UriKind.Relative);

        public bool CanBeRanged => false;

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public Task CacheHeaders(bool force = false)
        {
            return Task.CompletedTask;
        }

        public IStreamResource GetStreamable()
        {
            return this;
        }

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task<Stream>.FromResult((Stream)inputStream);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}
