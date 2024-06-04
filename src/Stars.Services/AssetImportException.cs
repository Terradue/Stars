// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AssetImportException.cs

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services
{
    [Serializable]
    internal class AssetImportException : AggregateException
    {
        private readonly IReadOnlyDictionary<string, IAsset> assets;

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
