// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IDeletableResource.cs

using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IDeletableResource : IStreamResource
    {
        Task DeleteAsync(CancellationToken ct);
    }
}
