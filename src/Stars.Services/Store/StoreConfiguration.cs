using System.IO;

namespace Terradue.Stars.Services.Store
{
    public class StoreOptions
    {
        public StoreOptions()
        {
            RootCatalogue = new CatalogueConfiguration()
            {
                Identifier = "catalog",
                Description = "Root catalog",
                Url = string.Format("file://{0}/catalog.json", Directory.GetCurrentDirectory()),
                DestinationUrl = string.Format("file://{0}", Directory.GetCurrentDirectory())
            };
        }

        public CatalogueConfiguration RootCatalogue { get; set; }
    }
}