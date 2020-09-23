using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Service.Supply.Destination;

namespace Terradue.Stars.Service.Router
{
    public class LocalFileSystemRoute : IRoute, IStreamable
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

        public async Task<INode> GoToNode()
        {
            return await WebRoute.Create(Uri).GoToNode();
        }


        public async Task<Stream> GetStreamAsync()
        {
            try
            {
                return await WebRoute.Create(Uri).GetStreamAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}