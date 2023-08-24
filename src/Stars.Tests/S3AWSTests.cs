// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3AWSTests.cs

using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Resources;
using Xunit;

namespace Stars.Tests
{
    [Collection(nameof(S3TestCollection))]
    public class S3AWSTests : S3BaseTest
    {
        private readonly AssetService assetService;
        private readonly IServiceProvider serviceProvider;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public S3AWSTests(AssetService assetService,
                       IServiceProvider sp,
                       IResourceServiceProvider resourceServiceProvider,
                       IS3ClientFactory s3ClientFactory) : base(s3ClientFactory)
        {
            this.assetService = assetService;
            serviceProvider = sp;
            this.resourceServiceProvider = resourceServiceProvider;
        }

        [Fact(Skip = "AWS credentials required")]
        public async Task AdaptRegion()
        {
            var client = s3ClientFactory.CreateS3Client(S3Url.Parse("s3://usgs-landsat/collection02/level-2/standard/oli-tirs/2022/088/084/LC09_L2SP_088084_20220405_20220407_02_T2/LC09_L2SP_088084_20220405_20220407_02_T2_thumb_small.jpeg"));
            await Assert.ThrowsAsync<AmazonS3Exception>(async () => await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = "usgs-landsat",
                Prefix = "collection02/level-2/standard/oli-tirs/2022/088/084/LC09_L2SP_088084_20220405_20220407_02_T2/LC09_L2SP_088084_20220405_20220407_02_T2_thumb_small.jpeg"
            }));
        }

    }
}
