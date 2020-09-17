using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Router
{
    public class LocalFileSystemRoute : IRoute
    {
        private Uri uri;
        private ContentType contentType;
        private ResourceType resourceType;
        private readonly ulong contentLength;

        public LocalFileSystemRoute(Uri uri, ContentType contentType, ResourceType resourceType, ulong contentLength)
        {
            this.uri = uri;
            this.contentType = contentType;
            this.resourceType = resourceType;
            this.contentLength = contentLength;
        }

        public Uri Uri => uri;

        public ContentType ContentType => contentType;

        public ResourceType ResourceType => resourceType;

        public ulong ContentLength => contentLength;

        public async Task<INode> GoToNode()
        {
            return await WebRoute.Create(Uri).GoToNode();
        }

        internal static LocalFileSystemRoute Create(IRoute route, IDestination destination)
        {
            return new LocalFileSystemRoute(destination.Uri, route.ContentType, route.ResourceType, route.ContentLength);
        }
    }
}