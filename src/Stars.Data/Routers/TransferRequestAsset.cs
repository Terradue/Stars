using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.OpenSearch.DataHub;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Routers
{
    internal class TransferRequestAsset : IAsset, IStreamable
    {
        private ITransferRequest tr;
        private readonly string label;
        private Dictionary<string, object> properties;

        public TransferRequestAsset(ITransferRequest tr, string label = null)
        {
            this.tr = tr;
            this.label = label;
            this.properties = new Dictionary<string, object>();
        }

        public string Title => label ?? tr.RequestUri.ToString();

        public Uri Uri => tr.RequestUri;

        public ContentType ContentType => tr.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => tr.ContentLength;

        public ContentDisposition ContentDisposition => tr.ContentDisposition;

        public IReadOnlyList<string> Roles => new string[] { "data" };

        public bool CanBeRanged => tr.CanBeRanged;

        public IReadOnlyDictionary<string, object> Properties => properties;

        public IStreamable GetStreamable()
        {
            return this;
        }

        public async Task<Stream> GetStreamAsync()
        {
            var response = await tr.GetResponseAsync();
            return response.GetResponseStream();
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            tr.AddRange(start, end);
            var response = await tr.GetResponseAsync();
            return response.GetResponseStream();
        }

        public Task Remove()
        {
            throw new NotImplementedException();
        }

        public Task CacheHeaders(bool force = false)
        {
            return Task.CompletedTask;
        }
    }
}