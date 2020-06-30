using Stars.Router;

namespace Stars.Supplier.Catalog
{
    public interface ILocalCatalogGenerator
    {
        string Id { get; }

        IResource LocalizeResource(IResource resource);
    }
}