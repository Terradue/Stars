using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Persistence;

namespace Terradue.Stars.Services.Persistence
{
    public abstract class PersistenceMapper
    {
        private readonly IPersistenceService persistenceService;

        protected PersistenceMapper(IPersistenceService persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public virtual async Task<LinkedList<ITransactableResource>> GetAncestorsList(ITransactableResource resource)
        {
            LinkedList<ITransactableResource> ancestors = new LinkedList<ITransactableResource>();
            await AddParent(ancestors, resource);
            return ancestors;
        }

        protected async Task AddParent(LinkedList<ITransactableResource> ancestors, ITransactableResource resource)
        {
            if (resource.Parent == null) return;
            var parent = await persistenceService.LoadLink(resource.Parent);
            ancestors.AddLast(parent);
            await AddParent(ancestors, parent);
            return;
        }
    }
}