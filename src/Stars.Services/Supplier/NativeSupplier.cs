using System;
using System.Threading;
using System.Threading.Tasks;
using Stac.Api.Interfaces;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Services.Supplier
{
    [PluginPriority(10)]
    public class NativeSupplier : ISupplier
    {
        private readonly CarrierManager carriersManager;

        public NativeSupplier(CarrierManager carriersManager)
        {
            this.carriersManager = carriersManager;
        }

        public int Priority { get; set; }
        public string Key { get => Id; set { } }

        public string Id => "Native";

        public string Label => "Native Supplier (self resource)";

        public Task<IResource> SearchForAsync(IResource resource, CancellationToken ct, string identifierRegex = null)
        {
            return Task.FromResult(resource);
        }

        public Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }

        public Task<IItemCollection> SearchForAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            // the Native supplier will never return a resource from a search expression
            return null;
        }

        public Task<object> InternalSearchExpressionAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
