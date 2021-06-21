using System;

namespace Terradue.Stars.Interface
{
    public interface ITransaction
    {
        void SetDestinationUri(Uri uri);

        void BeforeWrite(IResource existingResource);

        void AfterCommit(ITransactableResource committedResource);
    }
}