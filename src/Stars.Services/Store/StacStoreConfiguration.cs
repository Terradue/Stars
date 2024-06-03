// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacStoreConfiguration.cs

using System.IO;

namespace Terradue.Stars.Services.Store
{
    public class StacStoreConfiguration
    {
        public StacStoreConfiguration()
        {
            RootCatalogue = new StacCatalogueConfiguration()
            {
                Identifier = "catalog",
                Description = "Root catalog",
                Url = string.Format("file://{0}/catalog.json", Directory.GetCurrentDirectory()),
                DestinationUrl = string.Format("file://{0}", Directory.GetCurrentDirectory())
            };
            AbsoluteAssetsUrl = true;
        }

        public StacCatalogueConfiguration RootCatalogue { get; set; }

        public bool AllRelative { get; set; }

        public bool AbsoluteAssetsUrl { get; set; }
    }
}
