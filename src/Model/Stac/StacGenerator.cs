using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;
using Stars.Supplier.Asset;
using Stars.Supplier.Catalog;

namespace Stars.Model.Stac
{
    public class StacGenerator : ILocalCatalogGenerator
    {

        public StacGenerator()
        {
        }

        public string Id => "stac";

         public IResource LocalizeResource(IResource resource, IEnumerable<IResource> children, IEnumerable<IAsset> assets)
        {
            switch (resource.ResourceType)
            {
                case ResourceType.Catalog:
                    return GenerateCatalog(resource, children);
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

        private IResource GenerateCatalog(IResource resource, IEnumerable<IResource> children)
        {
            IStacCatalog stacObject = null;
            if ( resource is IStacResource )
                stacObject = ((IStacResource)resource).ReadAsStacObject();
            if ( stacObject != null ){
                foreach ( var link in stacObject.Links ){
                    // link.Uri
                }
            }
            return null;
        }
    }
}
