using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services
{
    [Serializable]
    internal class AssetImportException : AggregateException
    {
        private IReadOnlyDictionary<string, IAsset> assets;

        public AssetImportException()
        {
        }

        public AssetImportException(string message) : base(message)
        {
        }

        public AssetImportException(string message, IReadOnlyDictionary<string, IAsset> assets) : base(message)
        {
            this.assets = assets;
        }

        public AssetImportException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetImportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
