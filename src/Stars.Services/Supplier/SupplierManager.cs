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