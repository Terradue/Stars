using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Router;

namespace Terradue.Data.Tests
{
    internal class TestFileAsset : IAsset, IStreamResource
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

        public Uri Uri => new Uri(fileInfo.FullName);

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => Convert.ToUInt64(fileInfo.Length);

        public ContentDisposition ContentDisposition => new ContentDisposition()
        {
            FileName = fileInfo.Name,
            Size = fileInfo.Length,
            CreationDate = fileInfo.CreationTime,
            ModificationDate = fileInfo.LastWriteTime,
            Inline = false,
            DispositionType = "attachment"
        };

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("filename", folderUri.MakeRelativeUri(new Uri(fileInfo.FullName)).ToString());
                props.Add("file:size", fileInfo.Length);
                return props;
            }
        }

        public bool CanBeRanged => false;

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task.FromResult<Stream>(fileInfo.OpenRead());
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}