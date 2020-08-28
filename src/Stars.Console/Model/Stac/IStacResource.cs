using Stac;
using Stars.Interface.Router;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal interface IStacResource : IResource
    {
        IStacCatalog ReadAsStacObject();
    }
}