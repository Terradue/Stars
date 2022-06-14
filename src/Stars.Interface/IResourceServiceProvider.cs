using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Interface
{
    public interface IResourceServiceProvider
    {
        Task<IStreamResource> CreateStreamResourceAsync(IResource resource);

        Task<IStreamResource> GetStreamResourceAsync(IResource resource);
        
        Task<Stream> GetAssetStreamAsync(IAsset asset);
        
        Task<IAssetsContainer> GetAssetsInFolder(IResource resource);
        
        Task Delete(IResource resource);

        Uri ComposeLinkUri(IResourceLink childLink, IResource resource);
    }
}