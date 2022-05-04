using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Interface
{
    public interface IResourceServiceProvider
    {
        Task<IStreamResource> CreateStreamResourceAsync(Uri uri);

        Task<IStreamResource> CreateStreamResourceAsync(IResource resource);
        
        Task<Stream> GetAssetStreamAsync(IAsset asset);
        
        Task<IAssetsContainer> GetAssetsInFolder(Uri uri);

        
    }
}