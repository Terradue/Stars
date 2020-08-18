using System.Collections.Generic;

namespace Stars.Supplier.Asset
{
    internal interface IAssetsContainer
    {
        IEnumerable<IAsset> GetAssets();
    }
}