using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAssetsContainer : ILocatable
    {
        IReadOnlyDictionary<string, IAsset> Assets { get; }


    }
}
