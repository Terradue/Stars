using System.Collections.Generic;

namespace Stars.Interface.Supply.Asset
{
    public interface IAssetsContainer
    {
        IDictionary<string, IAsset> GetAssets();
    }
}