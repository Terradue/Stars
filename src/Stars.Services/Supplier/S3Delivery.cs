﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3Delivery.cs

using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class S3Delivery : IDelivery
    {
        private readonly ICarrier carrier;
        private readonly IResource route;
        private readonly S3ObjectDestination s3ObjectDestination;
        private readonly int cost;

        public S3Delivery(ICarrier carrier, IResource route, S3ObjectDestination s3ObjectDestination, int cost)
        {
            this.carrier = carrier;
            this.route = route;
            this.s3ObjectDestination = s3ObjectDestination;
            this.cost = cost;
        }

        public int Cost => cost;

        public IDestination Destination => s3ObjectDestination;

        public IResource Resource => route;

        public ICarrier Carrier => carrier;

        public string LocalPath => s3ObjectDestination.Uri.AbsolutePath;

        public override string ToString()
        {
            return string.Format("{0} from {1} to {2} ({3})", Carrier.Id, Resource, Destination, Cost);
        }

        public Func<IDelivery, IResource, Task> PreCheckDeliveryFunction { get; set; }
    }
}
