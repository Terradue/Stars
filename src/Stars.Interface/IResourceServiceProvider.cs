using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Interface
{
    public interface IResourceServiceProvider
    {
        Task<bool> CanBeRanged(IResource resource);

        Task<Stream> OpenStreamAsync(IResource resource);

        Task<Stream> GetStreamAsync(IResource resource, long start, long end = -1);
    }
}