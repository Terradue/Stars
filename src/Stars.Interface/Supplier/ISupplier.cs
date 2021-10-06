using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ISupplier : IPlugin
    {
        string Id { get; }
        string Label { get; }

        Task<IResource> SearchFor(IResource item);

        Task<IOrder> Order(IOrderable orderableRoute);
    }
}