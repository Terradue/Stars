using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terradue.Stars.Interface
{
    public interface IAssetsContainer : ILocatable
    {
        IReadOnlyDictionary<string, IAsset> Assets { get; }

        
    }
}