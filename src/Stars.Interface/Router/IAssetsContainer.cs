using System.Collections.Generic;

namespace Terradue.Stars.Interface.Router
{
    public interface IAssetsContainer
    {
        IDictionary<string, IAsset> GetAssets();
    }
}