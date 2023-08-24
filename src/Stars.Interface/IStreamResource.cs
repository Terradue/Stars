// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IStreamResource.cs

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IStreamResource : IResource
    {
        bool CanBeRanged { get; }

        Task<Stream> GetStreamAsync(CancellationToken ct);

        Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1);
    }
}
