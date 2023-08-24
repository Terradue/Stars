// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IAssetsContainer.cs

using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAssetsContainer : ILocatable
    {
        IReadOnlyDictionary<string, IAsset> Assets { get; }


    }
}
