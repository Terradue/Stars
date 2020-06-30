using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;
using Stars.Supplier.Catalog;

namespace Stars.Model.Stac
{
    public class StacGenerator : ILocalCatalogGenerator
    {

        public StacGenerator()
        {
        }

        public string Id => "stac";

        public IResource LocalizeResource(IResource resource)
        {
            switch (resource.ResourceType)
            {
                case ResourceType.Catalog:
                    return GenerateCatalog(resource);
                case ResourceType.Collection:
                    return GenerateCollection(resource);
                case ResourceType.Item:
                    return GenerateItem(resource);
                default:
                    return null;
            }
        }

        private IResource GenerateItem(IResource resource)
        {
            throw new NotImplementedException();
        }

        private IResource GenerateCollection(IResource resource)
        {
            throw new NotImplementedException();
        }

        private IResource GenerateCatalog(IResource resource)
        {
            IStacCatalog stacObject = (IStacCatalog)resource.ReadAsStacObject();
            if ( stacObject != null ){
                foreach ( var link in stacObject.Links ){
                    link.Uri
                }
            }
        }
    }
}
