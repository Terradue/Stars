using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
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

        public S3Tests(AssetService assetService, IServiceProvider sp)
        {
            this.assetService = assetService;
            this.serviceProvider = sp;
        }

        [Fact]
        public void Test1()
        {
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://cpe-production-catalog/test.json");
            StacCatalogNode node = (StacCatalogNode)StacCatalogNode.Create(new StacCatalog("test", "test"), s3ObjectDestination.Uri);
            Assert.Equal("s3://cpe-production-catalog/test.json", node.Uri.ToString());
        }

        [Fact]
        public async Task ImportAssetsS3toS3()
        {
            await CreateBucketAsync("s3://cpe-acceptance-catalog");
            // await CreateBucketAsync("s3://dest");
            await CopyLocalDataToBucketAsync(Path.Join(Environment.CurrentDirectory, "../../../In/assets/test.tif"), "s3://cpe-acceptance-catalog/users/evova11/uploads/0HMD4AJ2DCT0E/500x477.tif");
            System.Net.S3.S3WebRequest s3WebRequest = (System.Net.S3.S3WebRequest)WebRequest.Create("s3://cpe-acceptance-catalog/users/evova11/uploads/0HMD4AJ2DCT0E/500x477.tif");
            s3WebRequest.Method = "GET";
            System.Net.S3.S3WebResponse s3WebResponse = (System.Net.S3.S3WebResponse)await s3WebRequest.GetResponseAsync();
            StacItem item = StacConvert.Deserialize<StacItem>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")));
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://cpe-acceptance-catalog/calls/857/notifications/test502.json");
            StacItemNode itemNode = (StacItemNode)StacItemNode.Create(item, s3ObjectDestination.Uri);
            await assetService.ImportAssets(itemNode, s3ObjectDestination, AssetFilters.SkipRelative);
        }

        [Fact]
        public async Task ImportUnlimitedStreamabletoS3()
        {
            await CreateBucketAsync("s3://unlimited");
            WebRoute s3Route = WebRoute.Create(new Uri("s3://unlimited/test.bin"));
            BlockingStream stream = new BlockingStream(0, 100);
            S3StreamingCarrier s3StreamingCarrier = serviceProvider.GetRequiredService<S3StreamingCarrier>();
            s3StreamingCarrier.StartSourceCopy(
                                    File.OpenRead(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")),
                                    stream);
            IStreamable streamable = new TestStreamable(stream, 0);
            var newRoute = await s3StreamingCarrier.StreamToS3Object(streamable, s3Route);
            System.Net.S3.S3WebRequest s3WebRequest = (System.Net.S3.S3WebRequest)WebRequest.Create("s3://unlimited/test.bin");
            s3WebRequest.Method = "GET";
            System.Net.S3.S3WebResponse s3WebResponse = (System.Net.S3.S3WebResponse)await s3WebRequest.GetResponseAsync();
            Assert.Equal(new FileInfo(Path.Join(Environment.CurrentDirectory, "../../../In/items/test502.json")).Length, s3WebResponse.ContentLength);            
        }
    }
}
