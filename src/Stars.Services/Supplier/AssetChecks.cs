// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AssetChecks.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetChecks : List<IAssetCheck>
    {
        public static AssetChecks None => new AssetChecks();

        public static AssetChecks NoHtml
        {
            get
            {
                var af = new AssetChecks();
                af.Add(new DoNotAcceptHtmlAssetCheck());
                return af;
            }
        }

        public override string ToString()
        {
            return Count == 0 ? "No filter" : string.Join(", ", this.Select(af => af.ToString()));
        }

    }

    internal class DoNotAcceptHtmlAssetCheck : IAssetCheck
    {
        public Task Check(IAsset reference, IResource resource)
        {
            if (reference.ContentType.MediaType == "text/html")
                return Task.CompletedTask;

            if (resource.ContentType.MediaType == "text/html")
                throw new AssetCheckException("Asset is HTML.");

            return Task.CompletedTask;
        }
    }

    [Serializable]
    internal class AssetCheckException : Exception
    {
        public AssetCheckException()
        {
        }

        public AssetCheckException(string message) : base(message)
        {
        }

        public AssetCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetCheckException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public interface IAssetCheck
    {
        Task Check(IAsset reference, IResource resource);
    }


}
