using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Processing
{
    internal class TarGzipArchive : TarArchiveAsset
    {
        public TarGzipArchive(IAsset asset, ILogger logger) : base(asset, logger)
        {
        }

        protected override BlockingStream GetTarStream(IAsset asset)
        {
            const int chunk = 4096;
            BlockingStream blockingStream = new BlockingStream(1000);
            asset.GetStreamable().GetStreamAsync()
                .ContinueWith(task =>
                {
                    var stream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(task.GetAwaiter().GetResult());
                    Task.Factory.StartNew(() =>
                    {
                        int read;
                        var buffer = new byte[chunk];
                        do
                        {
                            read = stream.Read(buffer, 0, chunk);
                            blockingStream.Write(buffer, 0, read);
                        } while (read == chunk);
                        blockingStream.Close();
                    });
                });
            return blockingStream;
        }
    }
}