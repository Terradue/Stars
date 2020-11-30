using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terradue.Stars.Interface
{
    public interface IAssetsContainer
    {
        IReadOnlyDictionary<string, IAsset> Assets { get; }
    }
}