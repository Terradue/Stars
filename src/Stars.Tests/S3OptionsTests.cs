// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3OptionsTests.cs

using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Resources;
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
            serviceProvider = sp;
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
        }

    }
}
