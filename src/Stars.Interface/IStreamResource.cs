using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface IStreamResource : IResource
    {
        bool CanBeRanged { get; }

        Task<Stream> GetStreamAsync(CancellationToken ct);

        Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1);
    }
}