using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stars.Router;

namespace Stars.Supply
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(IEnumerable<ISupplier> generators): base(generators)
        {
        }

        internal IEnumerable<ISupplier> GetSuppliers(IResource resource)
        {
            return _items.Where(r => r.CanSupply(resource));
        }
    }
}