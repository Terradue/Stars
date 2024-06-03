// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IProcessing.cs

using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        ProcessingType ProcessingType { get; }
        string Label { get; }

        bool CanProcess(IResource resource, IDestination destination);

        Task<IResource> ProcessAsync(IResource resource, IDestination destination, CancellationToken ct, string suffix = null);

        string GetRelativePath(IResource resource, IDestination destination);
    }
}
