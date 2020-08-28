using System.Collections.Generic;
using System.Linq;
using Stars.Interface.Router;

namespace Stars.Interface.Supply
{
    public interface IDeliveryQuotation
    {

        IDictionary<IRoute, IOrderedEnumerable<IDelivery>> DeliveryQuotes { get; }
    }
}