using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stars.Interface.Supply.Destination;

namespace Stars.Service.Supply.Destination
{
    public class DestinationManager : AbstractManager<IDestinationGuide>
    {

        public DestinationManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        internal async Task<IDestination> CreateDestination(string output)
        {
            var dg = Plugins.FirstOrDefault(r => r.CanGuide(output));
            if ( dg == null )
                return null;
            return await dg.Guide(output);
        }
    }
}