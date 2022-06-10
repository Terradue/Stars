using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Resources
{
    public static class S3Extensions
    {

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         S3Url url,
                                                         IIdentityProvider identityProvider)
        {
            var Client = await factory.CreateS3ClientAsync(url, identityProvider);
            S3Resource s3Resource = new S3Resource(url, Client);
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         S3Url url)
        {
            var Client = await factory.CreateS3ClientAsync(url);
            S3Resource s3Resource = new S3Resource(url, Client);
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         IAsset asset)
        {
            var Client = await factory.CreateS3ClientAsync(S3Url.ParseUri(asset.Uri));
            S3Resource s3Resource = new S3Resource(asset, Client);
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                S3Url url,
                                                                IIdentityProvider identityProvider)
        {
            var Client = await factory.CreateS3ClientAsync(url, identityProvider);
            S3Resource s3Resource = new S3Resource(url, Client);
            await s3Resource.LoadMetadata();
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                S3Url url)
        {
            var Client = await factory.CreateS3ClientAsync(url);
            S3Resource s3Resource = new S3Resource(url, Client);
            await s3Resource.LoadMetadata();
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                IAsset asset)
        {
            var Client = await factory.CreateS3ClientAsync(asset);
            S3Resource s3Resource = new S3Resource(asset, Client);
            await s3Resource.LoadMetadata();
            return s3Resource;
        }


    }
}