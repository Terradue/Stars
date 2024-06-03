using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services.Supplier
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(ILogger<SupplierManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public PluginList<ISupplier> GetSuppliers(SupplierFilters supplierFilters = null)
        {
            var suppliers = GetPlugins();

            if (supplierFilters?.IncludeIds != null)
            {
                suppliers = new PluginList<ISupplier>(suppliers.Where(s => supplierFilters.IncludeIds.Contains(s.Key)));
            }

            if (supplierFilters?.ExcludeIds != null)
            {
                suppliers = new PluginList<ISupplier>(suppliers.Where(s => !supplierFilters.ExcludeIds.Contains(s.Key)));
            }

            return suppliers;
        }
    }
}
