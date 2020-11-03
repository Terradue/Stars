using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class DestinationManager : AbstractManager<IDestinationGuide>
    {

        public DestinationManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<IDestination> CreateDestination(string output, IRoute route)
        {
            var dg = Plugins.FirstOrDefault(r => r.Value.CanGuide(output, route));
            if ( dg.Value == null )
                return null;
            return await dg.Value.Guide(output, route);
        }
    }
}