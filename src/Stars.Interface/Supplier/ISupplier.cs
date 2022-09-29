using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ISupplier : IPlugin
    {
        string Id { get; }
        string Label { get; }

        Task<IResource> SearchForAsync(IResource item, CancellationToken ct, string identifierRegex = null);

        Task<IOrder> Order(IOrderable orderableRoute);
    }
}