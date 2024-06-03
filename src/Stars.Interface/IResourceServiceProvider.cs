using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IResourceServiceProvider
    {
        Task<IStreamResource> CreateStreamResourceAsync(IResource resource, CancellationToken ct);

        Task<IStreamResource> GetStreamResourceAsync(IResource resource, CancellationToken ct);

        Task<Stream> GetAssetStreamAsync(IAsset asset, CancellationToken ct);

        Task<IAssetsContainer> GetAssetsInFolderAsync(IResource resource, CancellationToken ct);

        Task DeleteAsync(IResource resource, CancellationToken ct);

        Uri ComposeLinkUri(IResourceLink childLink, IResource resource);
    }
}
