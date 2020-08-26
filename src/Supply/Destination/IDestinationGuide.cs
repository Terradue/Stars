using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Router;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Supply.Destination
{
    public interface IDestinationGuide
    {
        string Id { get; }

        bool CanGuide(string destination);

        Task<IDestination> Guide(string destination);

    }
}