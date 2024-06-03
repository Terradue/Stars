// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IDelivery.cs

using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IDelivery
    {
        int Cost { get; }

        IResource Resource { get; }

        ICarrier Carrier { get; }

        IDestination Destination { get; }

        string ToString();

    }
}
