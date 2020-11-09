using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Carrier;
using Microsoft.Extensions.Options;

namespace Terradue.Stars.Services.Supplier
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(ILogger<SupplierManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IEnumerable<ISupplier> GetSuppliers(SupplierFilters supplierFilters = null)
        {
            if (supplierFilters == null) return Plugins.Values;
            List<ISupplier> suppliers = new List<ISupplier>(Plugins.Values);

            if (supplierFilters.IncludeIds != null)
            {
                foreach (var supplierId in supplierFilters.IncludeIds)
                {
                    if (suppliers.FirstOrDefault(supplier => supplierId == supplier.Key) == null)
                        throw new KeyNotFoundException(string.Format("Supplier {0} not found!", supplierId));
                }
                suppliers = suppliers.Where(supplier => supplierFilters.IncludeIds.IsNullOrEmpty() ? true : supplierFilters.IncludeIds.Contains(supplier.Key)).ToList();
            }

            if (supplierFilters.ExcludeIds != null)
            {
                suppliers = suppliers.Where(supplier => supplierFilters.ExcludeIds.IsNullOrEmpty() ? true : !supplierFilters.IncludeIds.Contains(supplier.Key)).ToList();
            }

            return suppliers;
        }
    }
}