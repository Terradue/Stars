using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Interface.Model.Stac
{
    public interface IStacAssetsContainer : ITransactableResource
    {
        IDictionary<string, StacAsset> Assets { get; }
    }
}