using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;

namespace Stars.Tests
{
    internal class TestStreamable : IStreamable
    {
        private Stream stream;
        private readonly ulong contentLength;
        private readonly string filename;

        public TestStreamable(Stream stream, ulong contentLength, string filename)
        {
            this.stream = stream;
            this.contentLength = contentLength;
            this.filename = filename;
        }

        public bool CanBeRanged => false;

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => contentLength;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = filename };

        public Uri Uri => new Uri("http://localhost/" + filename);

        public Task<Stream> GetStreamAsync()
        {
            return Task.FromResult(stream);
        }

        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}