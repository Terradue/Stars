using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Router;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Supply
{
    [PluginPriority(100)]
    public class OutletStore : ISupplier
    {

        public string Id => "Native";

        public bool CanSupply(IResource resource)
        {
            return resource is IAssetsContainer;
        }

        public Task<IEnumerable<IResource>> LocalizeResource(IResource resource)
        {
            return Task.FromResult((IEnumerable<IResource>)new List<IResource>() { resource });
        }
    }
}