using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace Stars.Tests
{
    public abstract class S3BaseTest
    {

        protected async Task CreateBucketAsync(string s3bucketUri)
        {
            System.Net.S3.S3WebRequest s3WebRequest = (System.Net.S3.S3WebRequest)WebRequest.Create(s3bucketUri);
            s3WebRequest.Method = "MKB";
            System.Net.S3.S3ObjectWebResponse<PutBucketResponse> s3WebResponse =
                (System.Net.S3.S3ObjectWebResponse<PutBucketResponse>)(await s3WebRequest.GetResponseAsync());

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