using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Router
{
    public class LocalFileSystemRoute : IResource, IStreamable
    {
        private string filePath;
        private ContentType contentType;
        private ResourceType resourceType;
        private readonly ulong contentLength;

        public LocalFileSystemRoute(string filePath, ContentType contentType, ResourceType resourceType, ulong contentLength)
        {
            this.filePath = filePath;
            this.contentType = contentType;
            this.resourceType = resourceType;
            this.contentLength = contentLength;
        }

        public Uri Uri => new Uri(filePath);

        public ContentType ContentType => contentType;

        public ResourceType ResourceType => resourceType;

        public ulong ContentLength => contentLength;

        public FileInfo File => new FileInfo(Uri.AbsolutePath);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(filePath) };

        public bool CanBeRanged => true;

        public async Task<Stream> GetStreamAsync()
        {
            return await WebRoute.Create(Uri).GetStreamAsync();
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            return await WebRoute.Create(Uri).GetStreamAsync(start, end);
        }
    }
}