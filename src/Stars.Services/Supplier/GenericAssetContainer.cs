// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GenericAssetContainer.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supplier
{
    public class GenericAssetContainer : IAssetsContainer
    {
        private readonly ILocatable locatable;
        private readonly IDictionary<string, IAsset> assets;

        public GenericAssetContainer(ILocatable locatable, IDictionary<string, IAsset> assets)
        {
            this.locatable = locatable;
            this.assets = assets;
        }

        public Uri Uri => locatable.Uri;

        public IReadOnlyDictionary<string, IAsset> Assets => new ReadOnlyDictionary<string, IAsset>(assets);

    }
}
