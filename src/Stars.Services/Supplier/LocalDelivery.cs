// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: LocalDelivery.cs

using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class LocalDelivery : IDelivery
    {
        private readonly ICarrier carrier;
        private readonly IResource route;
        private readonly LocalFileDestination fileDestination;
        private readonly int cost;

        public LocalDelivery(ICarrier carrier, IResource route, LocalFileDestination fileDestination, int cost)
        {
            this.carrier = carrier;
            this.route = route;
            this.fileDestination = fileDestination;
            this.cost = cost;
        }

        public int Cost => cost;

        public IDestination Destination => fileDestination;

        public IResource Resource => route;

        public ICarrier Carrier => carrier;

        public string LocalPath => fileDestination.Uri.AbsolutePath;

        public Func<IDelivery, IResource, Task> PreCheckDeliveryFunction { get; set; }

        public override string ToString()
        {
            return string.Format("{0} from {1} to {2} ({3})", Carrier.Id, Resource, Destination, Cost);
        }
    }
}
