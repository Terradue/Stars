// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ICollection.cs

using System.Collections.Generic;
using Stac.Collection;

namespace Terradue.Stars.Interface
{
    public interface ICollection : ICatalog, IAssetsContainer
    {
        StacExtent Extent { get; }

        IDictionary<string, object> Properties { get; }
    }
}
