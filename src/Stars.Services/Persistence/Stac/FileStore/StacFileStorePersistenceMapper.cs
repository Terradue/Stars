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

        public string GetPath(ITransaction transaction)
        {
            var ancestors = transaction.GetResourceMap();

            string path = "";
            var currentLink = ancestors.Last;
            while (currentLink != null || currentLink.Value != transaction)
            {
                path += Path.Join(path, currentLink.Value.Id);
                currentLink = currentLink.Previous;
            }

            return path;
        }
    }
}