using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Persistence;

namespace Terradue.Stars.Services.Persistence.Stac.FileStore
{
    public class StacFileStorePersistenceMapper : PersistenceMapper
    {
        public StacFileStorePersistenceMapper(IStacPersistenceService stacPersistenceService) : base(stacPersistenceService)
        {
        }

        public async Task<string> GetPath(ITransactableResource resource)
        {
            var ancestors = await GetAncestorsList(resource);

            string path = "";
            var currentLink = ancestors.Last;
            while (currentLink != null || currentLink.Value != resource)
            {
                path += Path.Join(path, currentLink.Value.Id);
                currentLink = currentLink.Previous;
            }

            return path;
        }
    }
}