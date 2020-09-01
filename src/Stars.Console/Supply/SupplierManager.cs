using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Router;

namespace Stars.Supply
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(IEnumerable<ISupplier> generators) : base(generators)
        {
        }

        internal async Task<IDictionary<ISupplier, INode>> GetSuppliers(INode resource)
        {
            Dictionary<ISupplier, INode> newResources = new Dictionary<ISupplier, INode>();
            foreach (var supplier in _items)
            {
                var newResource = await supplier.SearchFor(resource);
                if(newResource != null)
                    newResources.Add(supplier, newResource);
            }
            return newResources;
        }

        internal ISupplier GetDefaultSupplier()
        {
            return _items.FirstOrDefault(i => i.Id == "Native");
        }
    }
}