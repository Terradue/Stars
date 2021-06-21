using System;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Persistence.Stac.FileStore;

namespace Terradue.Stars.Services.Persistence.Stac
{
    internal class StacTransaction : ITransaction
    {
        private StacNode newResource;
        private IStacPersistenceService stacFileStoreService;

        public StacTransaction(StacNode newResource, IStacPersistenceService stacFileStoreService)
        {
            this.newResource = newResource;
            this.stacFileStoreService = stacFileStoreService;
        }

        internal static StacTransaction Create(StacNode newResource, IStacPersistenceService stacFileStoreService)
        {
            return new StacTransaction(newResource, stacFileStoreService);
        }

        public void AfterCommit(ITransactableResource committedResource)
        {
            // Update Collection
            if (committedResource is StacItemNode)
                UpdateCollection(committedResource as StacItemNode);

            // Update parent
            UpdateParent(committedResource);
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

        private void UpdateCollection(StacItemNode stacItemNode)
        {
            throw new NotImplementedException();
        }

        private void UpdateParent(ITransactableResource committedResource)
        {
            throw new NotImplementedException();
        }

        private void ManageVersion(IResource existingResource)
        {
            throw new NotImplementedException();
        }
    }
}