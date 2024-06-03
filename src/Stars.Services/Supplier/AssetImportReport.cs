// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AssetImportReport.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

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
            importedAssets = new Dictionary<string, IAsset>();
            assetsExceptions = new Dictionary<string, Exception>();
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
