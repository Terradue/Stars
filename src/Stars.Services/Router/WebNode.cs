using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;

namespace Terradue.Stars.Services.Router
{
    internal class WebNode : INode, IStreamable
    {
        private WebResponse webResponse;
        private Stream responseStream;

        public WebNode(WebResponse webResponse)
        {
            this.webResponse = webResponse;
            responseStream = CopyAndClose(webResponse.GetResponseStream());
        }

        public string Label => webResponse.ResponseUri.ToString();

        public ContentType ContentType => new ContentType(webResponse.ContentType);

        public Uri Uri => webResponse.ResponseUri;

        public ResourceType ResourceType => ResourceType.Unknown;

        public ContentDisposition ContentDisposition
        {
            get
            {
                try
                {
                    string contentDispositionStr = webResponse.Headers["content-disposition"];
                    if (!string.IsNullOrEmpty(contentDispositionStr))
                    {
                        return new ContentDisposition(contentDispositionStr);
                    }
                }
                catch
                {

                }
                return new ContentDisposition() { FileName = Path.GetFileName(Uri.ToString()) };
            }
        }

        public string Id => Uri.ToString().CleanIdentifier();

        public ulong ContentLength => Convert.ToUInt64(webResponse.ContentLength);

        public bool IsCatalog => false;

        private static Stream CopyAndClose(Stream inputStream)
        {
            const int readSize = 256;
            byte[] buffer = new byte[readSize];
            MemoryStream ms = new MemoryStream();

            int count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Position = 0;
            inputStream.Close();
            return ms;
        }

        public Task<Stream> GetStreamAsync()
        {
            responseStream.Position = 0;
            return Task<Stream>.FromResult(responseStream as Stream);
        }

        public Task<INode> GoToNode()
        {
            return Task.FromResult((INode)this);
        }
    }
}