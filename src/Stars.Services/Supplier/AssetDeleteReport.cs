using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetDeleteReport
    {
        private IDictionary<string, Exception> assetsExceptions;

        public AssetDeleteReport()
        {
            this.assetsExceptions = new Dictionary<string, Exception>();
        }

        public IDictionary<string, Exception> AssetsExceptions { get => assetsExceptions; set => assetsExceptions = value; }

    }
}