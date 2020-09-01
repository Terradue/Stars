using System.Collections.Generic;

namespace Stars.Interface.Supply.Asset
{
    public interface IAssetsContainer
    {
        IEnumerable<IAsset> GetAssets();
    }
}