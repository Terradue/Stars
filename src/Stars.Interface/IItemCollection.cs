// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IItemCollection.cs

using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IItemCollection : ICollection<IItem>
    {
        IReadOnlyList<IResourceLink> GetLinks();
    }
}
