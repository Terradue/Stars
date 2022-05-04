using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Terradue.Stars.Services.Resources;

namespace Stars.Tests
{
    public abstract class S3BaseTest
    {
        protected IOptions<S3Options> s3Options;

        protected S3BaseTest(IOptions<S3Options> options)
        {
            this.s3Options = options;
        }

        protected async Task CreateBucketAsync(string s3bucketUri)
        {
            var s3uri = S3Url.Parse(s3bucketUri);
            var client = await S3Resource.GetS3ClientAsync(s3uri, s3Options.Value, null);
            await client.PutBucketAsync(s3uri.Bucket);
        }

        protected async Task CopyLocalDataToBucketAsync(string filename, string s3destination)
        {
            System.Net.S3.S3WebRequest s3WebRequest = (System.Net.S3.S3WebRequest)WebRequest.Create(s3destination);
            s3WebRequest.Method = "POST";
            s3WebRequest.ContentType = "application/octet-stream";

            Stream uploadStream = await s3WebRequest.GetRequestStreamAsync();
            Task.Run(() =>
            {
                using ( var fileStream = File.Open(filename, FileMode.Open, FileAccess.Read)){
                    fileStream.CopyTo(uploadStream, 4096);
                }
                uploadStream.Close();
            });
            System.Net.S3.S3WebResponse s3WebResponse = (System.Net.S3.S3WebResponse)await s3WebRequest.GetResponseAsync();

    }

}
}