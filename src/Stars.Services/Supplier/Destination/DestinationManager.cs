using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
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
            foreach (var guide in GetPlugins().Where(r => r.Value.CanGuide(output, route)))
            {
                var destination = await guide.Value.Guide(output, route);
                if (destination != null)
                    return destination;
            }
            throw new NotSupportedException("Impossible to create a destination to " + output);
        }
    }
}
