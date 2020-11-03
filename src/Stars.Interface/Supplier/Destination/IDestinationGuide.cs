using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestinationGuide : IPlugin
    {
        string Id { get; }

        bool CanGuide(string destination, Router.IRoute route);

        Task<IDestination> Guide(string destination, Router.IRoute route);

    }
}