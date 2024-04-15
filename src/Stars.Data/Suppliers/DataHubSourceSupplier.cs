using System;
using System.IO;
using System.Net;
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
using System.Linq;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;
using System.Threading;
using Terradue.Stars.Services.Resources;


namespace Terradue.Stars.Data.Suppliers
{
    public class DataHubSourceSupplier : OpenSearchableSupplier, ISupplier
    {
        private IDataHubSourceWrapper wrapper;
        private ICredentials credentialsManager;
        private readonly IS3ClientFactory _s3ClientFactory;

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
            ConfigureWrapper(new Uri(supplierPluginOption.ServiceUrl));
        }

        public override string Id => wrapper.Name;

        public override string Label => "Data supplier for " + wrapper.Name;

        public IDataHubSourceWrapper Wrapper => wrapper;

        public void ConfigureWrapper(Uri serviceUrl)
        {

            if (serviceUrl == null)
                throw new ArgumentNullException("serviceUrl");

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
                wrapper = new Terradue.OpenSearch.Asf.AsfApiWrapper(target_uri, target_creds);
            }

            if (target_uri.Host == "api.daac.asf.alaska.edu")
            {
                wrapper = new Terradue.OpenSearch.Asf.AsfApiWrapper(target_uri, target_creds);
            }

            // USGS case
            if (target_uri.Host == "earthexplorer.usgs.gov")
            {
                // usgsOpenSearchable
                wrapper = new Terradue.OpenSearch.Usgs.UsgsDataWrapper(new Uri("https://m2m.cr.usgs.gov"), target_creds);
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
                    //AuthType UriPrefix Username Password
                    string filePath = "gcloud_auth.json"; 
                    string contentToWrite = (target_creds as NetworkCredential).Password; //.Password;
                    File.WriteAllText(filePath, contentToWrite);
                    string absolutePath = Path.GetFullPath(filePath);
                    wrapper = new GoogleWrapper(absolutePath, null, null, "https://cloud.google.com");
                }
            //public GoogleWrapper(string authenticationFile, string projectName, ICredentials credentials, string osUrl = "http://opensearch.sentinel-hub.com/resto/api/collections/describe.xml", string preferredContentType = "application/json")
            this.openSearchable = wrapper.CreateOpenSearchable(new OpenSearchableFactorySettings(this.opensearchEngine));
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

        private IResource CreateDataHubResultItem(OpenSearch.Result.IOpenSearchResultCollection result, Uri sourceUri)
        {
            var firstItem = result.Items.FirstOrDefault();
            return new DataHubResultItemRoutable(firstItem, this, sourceUri, logger);
        }

        public override async Task<IOrder> Order(IOrderable orderableRoute)
        {
            OrderableAsset orderableAsset = orderableRoute as OrderableAsset;
            IAssetAccess assetAccess = await Task<IAssetAccess>.Run(() => wrapper.OrderProduct(orderableAsset.OpenSearchResultItem));
            OrderVoucher orderVoucher = new OrderVoucher(orderableRoute, assetAccess.Id);
            return orderVoucher;
        }

    }

}
