using System;
using System.IO;
using System.Runtime.Serialization;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Persistence.FileStore
{
    public class FileStorePersistenceMapper : PersistenceMapper
    {
        public string GetPath(IResource resource)
        {
            var ancestors = GetAncestorsList(resource);

            string path = "";
            var currentNode = ancestors.Last;
            while (currentNode != null || currentNode.Value != resource)
            {
                path += Path.Join(path, currentNode.Value.Id);
                currentNode = currentNode.Previous;
            }

            return path;
        }
    }
}