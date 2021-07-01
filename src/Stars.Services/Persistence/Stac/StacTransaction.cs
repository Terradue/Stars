using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Persistence.Stac.FileStore;

namespace Terradue.Stars.Services.Persistence.Stac
{
    internal class StacTransaction : ITransaction
    {
        private StacNode newResource;

        public StacTransaction(StacNode newResource)
        {
            this.newResource = newResource;
        }

        internal static StacTransaction CreateWriteTransaction(StacNode resource)
        {
            return new StacTransaction(resource.Clone() as StacNode);
        }

        public void AfterCommit(ITransactableResource committedResource)
        {
            // Update parent
            UpdateParent(committedResource as StacNode);
        }

        public void BeforeWrite(IResource existingResource)
        {
            // Manage Versioning
            ManageVersion(existingResource);
        }

        public void SetDestinationUri(Uri uri)
        {
            newResource.Uri = uri;
        }

        private async Task UpdateParent(StacNode stacNode)
        {
            if (stacNode.Parent == null) return;
            var parentNode = await stacFileStoreService.LoadLink(stacNode.Parent);
            if (parentNode == null)
                throw new InvalidOperationException(string.Format("Stac Object {0} referencing parent {1} that does not exist and cannot be automatically created.", stacNode.Id, stacNode.Parent));
            StacCatalogNode catalogNode = parentNode as StacCatalogNode;
            if (catalogNode == null)
                throw new InvalidOperationException(string.Format("Stac Object {0} referencing parent {1} that is not a catalog or a collection.", stacNode.Id, stacNode.Parent));

            if (catalogNode.StacCatalog is StacCollection)
            {
                if (!(stacNode is StacItemNode))
                    throw new InvalidOperationException(string.Format("Stac Object {0} referencing collection {1} is not an Item.", stacNode.Id, stacNode.Parent));
                Dictionary<Uri, StacItem> items = new Dictionary<Uri, StacItem>();
                items.Add(stacNode.Uri, (stacNode as StacItemNode).StacItem);
                UpdateCollection(catalogNode.StacCatalog as StacCollection, items);
            }
            else
            {
                Dictionary<Uri, IStacObject> items = new Dictionary<Uri, IStacObject>();
                items.Add(stacNode.Uri, stacNode.StacObject);
                parentNode = UpdateCatalog(catalogNode.StacCatalog as StacCatalog, items);
            }
        }

        private ITransactableResource UpdateCatalog(StacCatalog stacCatalog, Dictionary<Uri, IStacObject> items)
        {
            throw new NotImplementedException();
        }

        private void UpdateCollection(StacCollection stacCollection, IDictionary<Uri, StacItem> items)
        {
            stacCollection.Update(items);
        }

        private void ManageVersion(IResource existingResource)
        {
            throw new NotImplementedException();
        }
    }
}