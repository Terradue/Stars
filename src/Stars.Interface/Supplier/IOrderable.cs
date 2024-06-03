// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IOrderable.cs

using System;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IOrderable : IResource
    {
        string Id { get; }

        Uri OriginUri { get; }

        ISupplier Supplier { get; }
    }
}
