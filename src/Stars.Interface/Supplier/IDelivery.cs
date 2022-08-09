using System;
using Terradue.Stars.Interface.Router;
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