using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Supply.Destination
{
    public interface IDestinationGuide
    {
        string Id { get; }

        bool CanGuide(string destination);

        Task<IDestination> Guide(string destination);

    }
}