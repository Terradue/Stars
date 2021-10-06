using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Router;

namespace Terradue.Data.Test
{
    internal class TestFileAsset : IAsset
    {
        private readonly FileInfo fileInfo;
        private readonly Uri folderUri;
        private readonly Uri itemUri;

        public TestFileAsset(FileInfo fileInfo, Uri folderUri, Uri itemUri)
        {
            this.fileInfo = fileInfo;
            this.folderUri = folderUri;
            this.itemUri = itemUri;
        }

        public string Title => fileInfo.FullName;

        public IReadOnlyList<string> Roles => new string[1] { "data" };

        public Uri Uri => itemUri.MakeRelativeUri(new Uri(fileInfo.FullName));

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => Convert.ToUInt64(fileInfo.Length);

        public ContentDisposition ContentDisposition => null;

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("filename", folderUri.MakeRelativeUri(new Uri(fileInfo.FullName)).ToString());
                return props;
            }
        }

        public IStreamable GetStreamable()
        {
            return WebRoute.Create(new Uri(fileInfo.FullName), Convert.ToUInt64(fileInfo.Length));
        }

        public Task Remove()
        {
            throw new NotImplementedException();
        }
    }
}