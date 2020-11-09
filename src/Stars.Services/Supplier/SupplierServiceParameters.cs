using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Services.Supplier
{
    public class SupplyParameters
    {
        public static SupplyParameters Default => new SupplyParameters();

        public SupplyParameters()
        {
        }

        public SupplierFilters SupplierFilters { get; set; }
        public bool ContinueOnDeliveryError { get; set; }
    }
}