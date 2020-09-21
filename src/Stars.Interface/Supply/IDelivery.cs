using System;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;

namespace Stars.Interface.Supply
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