using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;

namespace Terradue.Stars.Services.Supply.Receipt
{
    internal class FolderRoute : IRoute
    {
        private Uri folderUri;

        public FolderRoute(IRoute route)
        {
            this.folderUri = new Uri(Path.GetDirectoryName(route.Uri.ToString()).Replace("file:", "file://"));
        }

        public Uri Uri => folderUri;

        public ContentType ContentType => new ContentType("application/x-directory");

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => 0;

        public Task<INode> GoToNode()
        {
            throw new NotSupportedException();
        }
    }
}