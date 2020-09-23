using System;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Interface.Supply
{
    public interface IDelivery
    {
        int Cost { get; }

        IRoute Route { get; }

        ICarrier Carrier { get; }

        ISupplier Supplier { get; }

        Uri TargetUri { get; }

        string ToString();

    }
}