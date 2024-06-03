using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Processing
{
    internal class TarGzipArchive : TarArchiveAsset
    {
        public TarGzipArchive(IAsset asset,
                              IResourceServiceProvider resourceServiceProvider,
                              ILogger logger) : base(asset, resourceServiceProvider, logger)
        {
        }

        protected override async Task<Stream> GetTarStreamAsync(IAsset asset, CancellationToken ct)
        {
            var streamResource = await resourceServiceProvider.GetStreamResourceAsync(asset, ct);
            var stream = new GZipInputStream(await streamResource.GetStreamAsync(ct));
            return BlockingStream.StartBufferedStreamAsync(stream, null, ct);
        }

    }
}
