using Stac;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal interface IStacResource : IResource
    {
        IStacCatalog ReadAsStacObject();
    }
}