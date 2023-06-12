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
    public class AssetImportReport : IAssetsContainer
    {
        private readonly IDeliveryQuotation quotation;
        private IDictionary<string, IAsset> importedAssets;

        private IDictionary<string, Exception> assetsExceptions;

        private readonly IDestination destination;

        public AssetImportReport(IDeliveryQuotation quotation,
                              IDestination destination)
        {
            this.quotation = quotation;
            this.importedAssets = new Dictionary<string, IAsset>();
            this.assetsExceptions = (quotation == null || quotation.AssetsExceptions == null ? new Dictionary<string, Exception>() : quotation.AssetsExceptions);
            this.destination = destination;
        }

        public IDestination Destination => destination;

        public IDictionary<string, Exception> AssetsExceptions { get => assetsExceptions; set => assetsExceptions = value; }

        public IDeliveryQuotation Quotation => quotation;

        public IDictionary<string, IAsset> ImportedAssets { get => importedAssets; set => importedAssets = value; }

        public IReadOnlyDictionary<string, IAsset> Assets => new ReadOnlyDictionary<string, IAsset>(importedAssets);

        public Uri Uri => destination.Uri;
    }
}