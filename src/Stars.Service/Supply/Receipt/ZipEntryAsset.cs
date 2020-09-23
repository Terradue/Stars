using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;

namespace Terradue.Stars.Service.Supply.Receipt
{
    internal class ZipEntryAsset : IAsset, IStreamable
    {
        private ZipEntry entry;
        private readonly ZipFile zipFile;
        private readonly IAsset parentAsset;

        public ZipEntryAsset(ZipEntry entry, ZipFile zipFile, IAsset parentAsset)
        {
            this.entry = entry;
            this.zipFile = zipFile;
            this.parentAsset = parentAsset;
        }

        public string Label => parentAsset.Label + " / " + entry.Name;

        public IEnumerable<string> Roles => new string[] { "data" };

        public Uri Uri => new Uri(entry.Name, UriKind.Relative);
        
        public ContentType ContentType => new ContentType(System.Net.Mime.MediaTypeNames.Application.Octet);

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => Convert.ToUInt64(entry.Size);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(entry.Name) };

        public Task<INode> GoToNode()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamAsync()
        {
            return Task.FromResult(zipFile.GetInputStream(entry));
        }

        public IStreamable GetStreamable()
        {
            return this;
        }
    }
}