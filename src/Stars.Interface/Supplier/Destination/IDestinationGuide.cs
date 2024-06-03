// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IDestinationGuide.cs

using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestinationGuide : IPlugin
    {
        string Id { get; }

        bool CanGuide(string destination, IResource route);

        Task<IDestination> Guide(string destination, IResource route);

    }
}
