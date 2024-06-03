using System;
using System.Collections.Generic;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetDeleteReport
    {
        private IDictionary<string, Exception> assetsExceptions;

        public AssetDeleteReport()
        {
            assetsExceptions = new Dictionary<string, Exception>();
        }

        public IDictionary<string, Exception> AssetsExceptions { get => assetsExceptions; set => assetsExceptions = value; }

    }
}
