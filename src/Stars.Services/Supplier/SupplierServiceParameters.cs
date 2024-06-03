// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: SupplierServiceParameters.cs

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
