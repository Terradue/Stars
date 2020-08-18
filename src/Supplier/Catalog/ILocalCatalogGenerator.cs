using System.Collections.Generic;
using Stars.Router;
using Stars.Supplier.Asset;

namespace Stars.Supplier.Catalog
{
    public interface ILocalCatalogGenerator
    {
        string Id { get; }

        IResource LocalizeResource(IResource resource, IEnumerable<IResource> children, IEnumerable<IAsset> assets);
    }
}