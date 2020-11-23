using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Services.Supplier
{
    public class SupplyParameters
    {
        public static SupplyParameters Default => new SupplyParameters();

        public SupplyParameters()
        {
        }

        

        public AssetFilters AssetFilters { get; set; }
        
        public bool ContinueOnDeliveryError { get; set; }
    }
}