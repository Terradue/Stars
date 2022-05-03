using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Interface
{
    public interface IResourceServiceProvider
    {
        Task<IStreamResource> GetStreamResourceAsync(IResource resource);
        
        Task<Stream> GetAssetStreamAsync(IAsset asset);
    }
}