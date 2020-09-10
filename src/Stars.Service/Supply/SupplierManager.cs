using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Service.Router;

namespace Stars.Service.Supply
{
    public class SupplierManager : AbstractManager<ISupplier>
    {
        public SupplierManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        internal async Task<IDictionary<ISupplier, INode>> GetSuppliers(IStacNode stacNode)
        {
            Dictionary<ISupplier, INode> newResources = new Dictionary<ISupplier, INode>();
            foreach (var supplier in Plugins)
            {
                var newResource = await supplier.SearchFor(stacNode);
                if(newResource != null)
                    newResources.Add(supplier, newResource);
            }
            return newResources;
        }

        internal ISupplier GetDefaultSupplier()
        {
            return Plugins.FirstOrDefault(i => i.Id == "Native");
        }
    }
}