namespace Terradue.Stars.Services.Supplier
{
    public class SupplierServiceParameters
    {
        public SupplierServiceParameters()
        {
            SupplierFilters = new SupplierFilters();
        }

        public SupplierFilters SupplierFilters { get; set; }
        public bool ContinueOnDeliveryError { get; set; }
    }
}