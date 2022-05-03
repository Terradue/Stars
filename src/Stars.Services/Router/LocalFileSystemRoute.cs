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
    public class LocalFileSystemResource : IResource, IStreamResource
    {
        private readonly ResourceType resourceType;
        private FileInfo fileInfo;

        public LocalFileSystemResource(string filePath, ResourceType ResourceType)
        {
            this.fileInfo = new FileInfo(filePath);
            resourceType = ResourceType;
        }

        public Uri Uri => new Uri("file://" + fileInfo.FullName);

        public ContentType ContentType => new ContentType(MimeTypes.GetMimeType(fileInfo.Name));

        public ResourceType ResourceType => resourceType;

        public ulong ContentLength => Convert.ToUInt64(fileInfo.Length);

        public FileInfo File => fileInfo;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = fileInfo.Name };

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