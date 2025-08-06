// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: DataHubSourceSupplier.cs

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.OpenSearch;
using Terradue.OpenSearch.DataHub;
using Terradue.OpenSearch.DataHub.Aws;
using Terradue.OpenSearch.DataHub.DHuS;
using Terradue.OpenSearch.DataHub.Dias;
using Terradue.OpenSearch.DataHub.GoogleCloud;
using Terradue.Stars.Data.Routers;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Translator;


namespace Terradue.Stars.Data.Suppliers
{
    public class CopernicusDataspaceS3Supplier : DataHubSourceSupplier, ISupplier
    {
        private IDataHubSourceWrapper wrapper;
        private readonly ICredentials credentialsManager;
        private readonly IS3ClientFactory _s3ClientFactory;

        public CopernicusDataspaceS3Supplier(ILogger<DataHubSourceSupplier> logger,
                                     TranslatorManager translatorManager,
                                     ICredentials credentialsManager,
                                     IS3ClientFactory s3ClientFactory,
                                     IPluginOption pluginOption) : base(logger, translatorManager, credentialsManager, s3ClientFactory, pluginOption)
        {
            Logger4Net.Setup(logger);
            this.credentialsManager = credentialsManager;
            _s3ClientFactory = s3ClientFactory;
            SupplierPluginOption supplierPluginOption = pluginOption as SupplierPluginOption;
            ConfigureWrapper(supplierPluginOption);
        }

        public override string Id => wrapper.Name;

        public override string Label => "Data supplier for " + wrapper.Name;

        public IDataHubSourceWrapper Wrapper => wrapper;

        public override void ConfigureWrapper(SupplierPluginOption pluginOption)
        {
            if (pluginOption == null)
                throw new ArgumentNullException("pluginOption");

            Uri serviceUrl = new Uri(pluginOption.ServiceUrl);

            var target_uri = serviceUrl;
            var target_creds = credentialsManager;

            if (target_creds == null)
                logger.LogWarning("Credentials are not set, target sites' services requiring credentials for data access will fail!");


            if (target_uri.Host == "catalogue.dataspace.copernicus.eu")
            {
                S3Configuration s3Configuration = _s3ClientFactory.GetS3Configuration("s3://eodata/test.tif");
                CopernicusOdataWrapper copernicusOdataWrapper = new CopernicusOdataWrapper(
                    target_creds,
                    "https://catalogue.dataspace.copernicus.eu/odata/v1"
                )
                {
                    EnableDirectDataAccess = true,
                    S3AccessKey = s3Configuration.AccessKey,
                    S3SecretKey = s3Configuration.SecretKey,
                    S3Endpoint = s3Configuration.ServiceURL
                };
                wrapper = copernicusOdataWrapper;
            }
            else if (target_uri.Host.EndsWith("copernicus.eu"))
            {
                wrapper = new DHuSWrapper(target_uri, target_creds);
            }

            openSearchable = wrapper.CreateOpenSearchable(new OpenSearchableFactorySettings(opensearchEngine));
        }

        public override async Task<IResource> SearchForAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            var result = await QueryAsync(node, ct, identifierRegex);
            if (result == null) return null;

            Uri sourceUri = wrapper.Settings.ServiceUrl;
            var sourceLink = result.Links.FirstOrDefault(l => l.RelationshipType == "self");
            if (sourceLink != null)
                sourceUri = sourceLink.Uri;

            return CreateDataHubResultItem(result, sourceUri);
        }

    }

}
