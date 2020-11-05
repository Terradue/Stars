using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAssetsContainer
    {
        IDictionary<string, IAsset> GetAssets();
    }
}