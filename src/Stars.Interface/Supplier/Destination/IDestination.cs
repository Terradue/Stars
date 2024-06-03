// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IDestination.cs

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestination : ILocatable
    {
        IDestination To(IResource route, string relPath = null);

        void PrepareDestination();

        string ToString();
    }
}
