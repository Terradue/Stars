using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Services.Processing.Destination
{
    public class DestinationManager : AbstractManager<IDestinationGuide>
    {

        public DestinationManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<IDestination> CreateDestination(string output)
        {
            var dg = Plugins.FirstOrDefault(r => r.CanGuide(output));
            if ( dg == null )
                return null;
            return await dg.Guide(output);
        }
    }
}