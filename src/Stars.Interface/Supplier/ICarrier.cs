// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ICarrier.cs

using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ICarrier : IPlugin
    {
        string Id { get; }

        bool CanDeliver(IResource route, IDestination destination);

        Task<IResource> DeliverAsync(IDelivery delivery, CancellationToken ct, bool overwrite = false);

        IDelivery QuoteDelivery(IResource route, IDestination destination);

    }
}
