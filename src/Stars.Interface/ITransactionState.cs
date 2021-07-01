using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface ITransaction
    {
        void SetDestinationUri(Uri uri);

        void BeforeWrite(IResource existingResource);

        void AfterCommit(ITransactableResource committedResource);

        LinkedList<IResourceLink> GetResourceMap();
    }
}