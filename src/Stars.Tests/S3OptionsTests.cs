using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stac;
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
    public class S3OptionsTests : S3BaseTest, IClassFixture<Logging>
    {
        private readonly IServiceProvider serviceProvider;

        public S3OptionsTests(AssetService assetService,
                       IServiceProvider sp,
                       IResourceServiceProvider resourceServiceProvider,
                       IS3ClientFactory s3ClientFactory) : base(s3ClientFactory)
        {
            this.serviceProvider = sp;
        }

        [Fact]
        public async Task NamedS3ClientAsync()
        {
            var s3Client = s3ClientFactory.CreateS3Client("geohazards-tep");
            Assert.NotNull(s3Client);
            Assert.Equal("fr-par", s3Client.Config.AuthenticationRegion);
            S3Url s3url = S3Url.Parse("https://s3.geohazards-tep.eu/bucket/key");
            s3Client = s3ClientFactory.CreateS3Client(s3url);
            Assert.NotNull(s3Client);
            Assert.Equal("fr-par", s3Client.Config.AuthenticationRegion);
            Assert.Equal("https://s3.geohazards-tep.eu/", s3Client.Config.ServiceURL);
        }

    }
}
