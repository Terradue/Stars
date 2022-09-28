using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;

namespace Stars.Tests
{
    internal class TestStreamable : IStreamResource
    {
        private Stream stream;
        private readonly ulong contentLength;

        public TestStreamable(Stream stream, ulong contentLength)
        {
            this.stream = stream;
            this.contentLength = contentLength;
        }

        public bool CanBeRanged => false;

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => contentLength;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = "test.bin" };

        public Uri Uri => new Uri("http://localhost/test.bin");

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task.FromResult(stream);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}