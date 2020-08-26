using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stars.Supply.Destination
{
    public class DestinationManager : AbstractManager<IDestinationGuide>
    {

        public DestinationManager(IEnumerable<IDestinationGuide> guide) : base(guide)
        {
        }

        internal async Task<IDestination> CreateDestination(string output)
        {
            var dg = _items.FirstOrDefault(r => r.CanGuide(output));
            if ( dg == null )
                return null;
            return await dg.Guide(output);
        }
    }
}