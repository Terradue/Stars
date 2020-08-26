using System.Collections.Generic;

namespace Stars.Supply.Asset
{
    internal interface IAssetsContainer
    {
        IEnumerable<IAsset> GetAssets();
    }
}