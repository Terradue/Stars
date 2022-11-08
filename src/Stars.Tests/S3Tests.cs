using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;

namespace Stars.Tests
{
    [Collection(nameof(S3TestCollection))]
    public class S3Tests : S3BaseTest
    {
        private readonly AssetService assetService;
        private readonly IServiceProvider serviceProvider;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public S3Tests(AssetService assetService,
                       IServiceProvider sp,
                       IResourceServiceProvider resourceServiceProvider,
                       IS3ClientFactory s3ClientFactory) : base(s3ClientFactory)
        {
            this.assetService = assetService;
            this.serviceProvider = sp;
            this.resourceServiceProvider = resourceServiceProvider;
        }

        [Fact]
        public void Test1()
        {
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://local-production-catalog/test.json");
            StacCatalogNode node = (StacCatalogNode)StacCatalogNode.Create(new StacCatalog("test", "test"), s3ObjectDestination.Uri);
            Assert.Equal("s3://local-production-catalog/test.json", node.Uri.ToString());
        }

        [Fact]
        public async Task ImportAssetsS3toS3()
        {
            await CreateBucketAsync("s3://local-acceptance-catalog/test");
            await CopyLocalDataToBucketAsync(Path.Join(Environment.CurrentDirectory, "../../../In/assets/test.tif"), "s3://local-acceptance-catalog/users/evova11/uploads/0HMD4AJ2DCT0E/500x477.tif");
            var s3Resource = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri("s3://local-acceptance-catalog/users/evova11/uploads/0HMD4AJ2DCT0E/500x477.tif")), CancellationToken.None);
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")));
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://local-acceptance-catalog/calls/857/notifications/test502.json");
            StacItemNode itemNode = (StacItemNode)StacItemNode.Create(item, s3ObjectDestination.Uri);
            var importReport = await assetService.ImportAssetsAsync(itemNode, s3ObjectDestination, AssetFilters.SkipRelative, AssetChecks.None, CancellationToken.None);
            foreach (var ex in importReport.AssetsExceptions)
            {
                throw ex.Value;
            }
            var s3dest = await s3ClientFactory.CreateAndLoadAsync(S3Url.Parse("s3://local-acceptance-catalog/calls/857/notifications/500x477.tif"), CancellationToken.None);
            Assert.Equal(s3Resource.ContentLength, s3dest.ContentLength);
        }

        [Fact]
        public async Task ImportAssetsS30SizedtoS3()
        {
            await CreateBucketAsync("s3://local-acceptance-catalog2/indices_cog");
            await CopyLocalDataToBucketAsync(Path.Join(Environment.CurrentDirectory, "../../../In/assets/test.tif"), "s3://local-acceptance-catalog2/indices_cog/cci_fss/CFD/GDA-AID-DR_UC7-ADBMON_Product_FSS-CFD-V01_IronDzud-Khuvsgul-1993.tif");
            var s3Resource = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri("s3://local-acceptance-catalog2/indices_cog/cci_fss/CFD/GDA-AID-DR_UC7-ADBMON_Product_FSS-CFD-V01_IronDzud-Khuvsgul-1993.tif")), CancellationToken.None);
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/cci_fss_CFD_1993.json")));
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://local-acceptance-catalog2/indices_cog/copy/cci_fss_CFD_1993.json");
            StacItemNode itemNode = (StacItemNode)StacItemNode.Create(item, s3ObjectDestination.Uri);
            var importReport = await assetService.ImportAssetsAsync(itemNode, s3ObjectDestination, AssetFilters.SkipRelative, AssetChecks.None, CancellationToken.None);
            foreach (var ex in importReport.AssetsExceptions)
            {
                throw ex.Value;
            }
            var s3dest = await s3ClientFactory.CreateAndLoadAsync(S3Url.Parse("s3://local-acceptance-catalog2/indices_cog/copy/GDA-AID-DR_UC7-ADBMON_Product_FSS-CFD-V01_IronDzud-Khuvsgul-1993.tif"), CancellationToken.None);
            Assert.Equal(s3Resource.ContentLength, s3dest.ContentLength);
            Assert.NotEqual(item.Assets.First().Value.FileExtension().Size, importReport.Assets.First().Value.ContentLength);
        }

        [Fact]
        public async Task ImportUnlimitedStreamabletoS3()
        {
            await CreateBucketAsync("s3://local-unlimited");
            S3Url s3Url = S3Url.Parse("s3://local-unlimited/test.bin");
            S3Resource s3Route = await s3ClientFactory.CreateAsync(s3Url);
            BlockingStream stream = new BlockingStream(0, 100);
            S3StreamingCarrier s3StreamingCarrier = serviceProvider.GetRequiredService<S3StreamingCarrier>();
            s3StreamingCarrier.StartSourceCopy(
                                    File.OpenRead(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")),
                                    stream);
            IStreamResource streamable = new TestStreamable(stream, 0);
            var newRoute = await s3StreamingCarrier.StreamToS3Object(streamable, s3Route, CancellationToken.None);
            var metadata = await s3Route.Client.GetObjectMetadataAsync(s3Url.Bucket, s3Url.Key);
            Assert.Equal(new FileInfo(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")).Length, metadata.ContentLength);
        }

        [Fact]
        public async Task ImportHttpStreamabletoS3()
        {
            await CreateBucketAsync("s3://local-http");
            S3Resource s3Route = await s3ClientFactory.CreateAsync(S3Url.Parse("s3://local-http/S2B_MSIL2A_20211022T045839_N0301_R119_T44NLN_20211022T071547.jpg"));
            BlockingStream stream = new BlockingStream(0, 100);
            S3StreamingCarrier s3StreamingCarrier = serviceProvider.GetRequiredService<S3StreamingCarrier>();
            var httpRoute = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri("https://store.terradue.com/api/scihub/sentinel2/S2MSI2A/2021/10/22/quicklooks/v1/S2B_MSIL2A_20211022T045839_N0301_R119_T44NLN_20211022T071547.jpg")), CancellationToken.None);
            var newRoute = await s3StreamingCarrier.StreamToS3Object(httpRoute, s3Route, CancellationToken.None);
            var metadata = await s3Route.Client.GetObjectMetadataAsync(s3Route.S3Uri.Bucket, s3Route.S3Uri.Key);
            Assert.Equal(httpRoute.ContentLength, Convert.ToUInt64(metadata.ContentLength));
        }

        [Fact]
        public async Task AdaptRegion()
        {
            await CreateBucketAsync("local-nogoodregion", "localstack-eu-west-1");
            var client = s3ClientFactory.CreateS3Client(S3Url.Parse("http://local-nogoodregion.s3.eu-west-1.localhost.localstack.cloud:4566/"));
            var bucketlocation = await client.GetBucketLocationAsync("local-nogoodregion");
            Assert.Equal(Amazon.S3.S3Region.EU, bucketlocation.Location);
            client = s3ClientFactory.CreateS3Client(S3Url.Parse("http://local-nogoodregion.s3.eu-central-1.localhost.localstack.cloud:4566/"));
            var listing = await client.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = "local-nogoodregion",
            });
            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = "local-nogoodregion",
                Key = "test.json",
                ContentBody = File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json"))
            });
            var s3Route = await s3ClientFactory.CreateAsync(S3Url.Parse("s3://local-nogoodregion/test.json"));
        }

    }
}
