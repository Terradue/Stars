using System;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Persistence.FileStore
{
    internal class FileTransactionState : ITransactionState
    {
        public FileTransactionState(Uri resourceUri)
        {
            ResourceUri = resourceUri;
        }

        public Uri ResourceUri { get; }

        internal static FileTransactionState Create(Uri resourceUri)
        {
            return new FileTransactionState(resourceUri);
        }
    }
}