using System.Collections.Generic;

namespace Terradue.Stars.Interface.Supply.Asset
{
    public interface IAssetsContainer
    {
        IDictionary<string, IAsset> GetAssets();
    }
}