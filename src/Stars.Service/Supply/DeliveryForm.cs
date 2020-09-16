using System.Collections.Generic;
using Stars.Interface.Router;

namespace Stars.Service.Supply
{
    public class DeliveryForm
    {
        private List<IRoute> deliveredRoutes;

        public DeliveryForm(List<IRoute> deliveredRoutes)
        {
            this.deliveredRoutes = deliveredRoutes;
        }
    }
}