using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public interface IS3ClientFactory
    {
        IAmazonS3 CreateS3Client(S3Url s3Url);
        IAmazonS3 CreateS3Client(IAsset asset);
        Task<IAmazonS3> CreateS3ClientAsync(S3Url url, IIdentityProvider identityProvider);
        AWSCredentials CreateCredentials(S3Url s3Url);
        IAmazonS3 CreateS3Client(string name);
        Task<AWSCredentials> GetWebIdentityCredentialsAsync(string url, JwtSecurityToken jwt, string policy);
        string GetPersonalStoragePolicyName(IIdentityProvider identityProvider);
    }
}