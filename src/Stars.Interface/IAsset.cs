// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IAsset.cs

using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAsset : IResource
    {
        string Title { get; }

        IReadOnlyList<string> Roles { get; }

        IReadOnlyDictionary<string, object> Properties { get; }

        IEnumerable<IAsset> Alternates { get; }

    }
}
