using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supply
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IEnumerable<ISupplier> GetSuppliers(SupplierFilters supplierFilters = null)
        {
            if (supplierFilters == null) return Plugins;
            var suppliers = Plugins.Where(supplier => supplierFilters.IncludeIds.IsNullOrEmpty() ? true : supplierFilters.IncludeIds.Contains(supplier.Id))
                            .Where(supplier => supplierFilters.ExcludeIds.IsNullOrEmpty() ? true : !supplierFilters.IncludeIds.Contains(supplier.Id)).ToList();
            suppliers.Insert(0, GetDefaultSupplier());
            return suppliers;
        }

        public ISupplier GetDefaultSupplier()
        {
            return Plugins.FirstOrDefault(i => i.Id == "Native");
        }

    }
}