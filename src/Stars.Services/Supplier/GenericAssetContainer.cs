using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class GenericAssetContainer : IAssetsContainer
    {
        private readonly ILocatable locatable;
        private readonly IDictionary<string, IAsset>  assets;
        
        public GenericAssetContainer(ILocatable locatable, IDictionary<string, IAsset> assets)
        {
            this.locatable = locatable;
            this.assets = assets;
        }

        public Uri Uri => locatable.Uri;

        public IReadOnlyDictionary<string, IAsset> Assets => new ReadOnlyDictionary<string, IAsset>(assets);

    }
}