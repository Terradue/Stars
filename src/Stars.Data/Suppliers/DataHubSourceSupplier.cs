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
    public class DataHubSourceSupplier : OpenSearchableSupplier, ISupplier
    {
        protected IDataHubSourceWrapper wrapper;
        protected readonly ICredentials credentialsManager;
        protected readonly IS3ClientFactory _s3ClientFactory;

        public DataHubSourceSupplier(ILogger<DataHubSourceSupplier> logger,
                                     TranslatorManager translatorManager,
                                     ICredentials credentialsManager,
                                     IS3ClientFactory s3ClientFactory,
                                     IPluginOption pluginOption) : base(logger, translatorManager)
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

        public virtual void ConfigureWrapper(SupplierPluginOption pluginOption)
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
                wrapper = new CopernicusOdataWrapper(
                    target_creds,
                    "https://catalogue.dataspace.copernicus.eu/odata/v1"
                );
            }
            else if (target_uri.Host.EndsWith("copernicus.eu"))
            {
                wrapper = new DHuSWrapper(target_uri, target_creds);
            }


            if (target_uri.Host == "catalogue.onda-dias.eu")
            {
                wrapper = new OndaDiasWrapper(new Uri(string.Format("https://catalogue.onda-dias.eu/dias-catalogue")), target_creds, null);
            }

            if (target_uri.Host == "finder.creodias.eu")
            {
                wrapper = new CreoDiasWrapper(target_creds);
            }

            if (target_uri.Host.Contains("mundiwebservices.com"))
            {
                wrapper = new MundiDiasWrapper(target_creds, openStackStorageSettings: null);
                // wrapper.S3KeyId = targetSiteConfig.Data.S3KeyId;
                // wrapper.S3SecretKey = targetSiteConfig.Data.S3SecretKey;
            }

            if (target_uri.Host.Contains("sobloo.eu"))
            {
                wrapper = new SoblooDiasWrapper(target_creds);
            }

            if (target_uri.Host == "api.daac.asf.alaska.edu")
            {
                wrapper = new OpenSearch.Asf.AsfApiWrapper(target_uri, target_creds);
            }

            if (target_uri.Host == "api.daac.asf.alaska.edu")
            {
                wrapper = new OpenSearch.Asf.AsfApiWrapper(target_uri, target_creds);
            }

            // USGS case
            if (target_uri.Host == "earthexplorer.usgs.gov")
            {
                // usgsOpenSearchable
                wrapper = new OpenSearch.Usgs.UsgsDataWrapper(new Uri("https://m2m.cr.usgs.gov"), target_creds);
            }

            if (target_uri.Host.EndsWith("amazon.com"))
            {
                Amazon.Runtime.AWSCredentials awscreds = _s3ClientFactory.GetConfiguredCredentials(S3Url.Parse("s3://sentinel-s2-l1c/test.tif"));
                if (awscreds.GetType().ToString().Contains("DefaultInstanceProfileAWSCredentials"))
                {
                    awscreds = null;
                }
                var amazonWrapper = new AmazonStacWrapper(awscreds?.GetCredentials().AccessKey, awscreds?.GetCredentials().SecretKey, target_creds);
                wrapper = amazonWrapper;
            }

            if (target_uri.Host.EndsWith("googleapis.com") || target_uri.Host.EndsWith("google.com"))
            {
                wrapper = new GoogleWrapper(pluginOption.AccountFile, pluginOption.ProjectId, target_creds, "https://cloud.google.com");
            }

            openSearchable = wrapper.CreateOpenSearchable(new OpenSearchableFactorySettings(opensearchEngine));

        }

        [Obsolete("Method kept for backward compatibility")]
        public void ConfigureWrapper(Uri serviceUrl)
        {
            if (serviceUrl == null)
                throw new ArgumentNullException("serviceUrl");

            ConfigureWrapper(new SupplierPluginOption() { ServiceUrl = serviceUrl.AbsoluteUri });
        }

        internal static NetworkCredential GetNetworkCredentials(IConfigurationSection credentials)
        {
            if (credentials == null)
                return null;
            return new NetworkCredential(credentials["Username"], credentials["Password"]);
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

        protected IResource CreateDataHubResultItem(OpenSearch.Result.IOpenSearchResultCollection result, Uri sourceUri)
        {
            var firstItem = result.Items.FirstOrDefault();
            return new DataHubResultItemRoutable(firstItem, this, sourceUri, logger);
        }

        public override async Task<IOrder> Order(IOrderable orderableRoute)
        {
            OrderableAsset orderableAsset = orderableRoute as OrderableAsset;
            IAssetAccess assetAccess = await Task.Run(() => wrapper.OrderProduct(orderableAsset.OpenSearchResultItem));
            OrderVoucher orderVoucher = new OrderVoucher(orderableRoute, assetAccess.Id);
            return orderVoucher;
        }

    }

}
