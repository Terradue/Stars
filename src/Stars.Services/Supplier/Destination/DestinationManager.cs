using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class DestinationManager : AbstractManager<IDestinationGuide>
    {

        public DestinationManager(ILogger<DestinationManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<IDestination> CreateDestination(string output, IResource route)
        {
            foreach (var guide in Plugins.Where(r => r.Value.CanGuide(output, route)))
            {
                var destination = await guide.Value.Guide(output, route);
                if (destination != null)
                    return destination;
            }
            throw new NotSupportedException("Impossible to create a destination to " + output);
        }
    }
}