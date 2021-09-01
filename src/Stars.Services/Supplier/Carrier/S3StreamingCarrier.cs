using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using System.Security.AccessControl;
using System.Net;
using System.Net.S3;
using Amazon.S3.Model;
using System.Threading;
using Amazon.S3.Transfer;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using System.Reflection;
using Amazon.S3.Util;
using System.Text.RegularExpressions;
using System.Linq;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class S3StreamingCarrier : ICarrier
    {
        private readonly ILogger logger;
        private readonly AWSOptions options;
        private readonly S3BucketsOptions s3BucketsConfiguration;

        private readonly Regex regEx = new Regex(@"^s3://(?'hostOrBucket'[^/]*)(/.*)?$");

        public S3StreamingCarrier(ILogger<S3StreamingCarrier> logger, Amazon.Extensions.NETCore.Setup.AWSOptions options, S3BucketsOptions s3BucketsConfiguration = null)
        {
            this.logger = logger;
            this.options = options;
            this.s3BucketsConfiguration = s3BucketsConfiguration;
            Priority = 75;

        }

        public int Priority { get; set; }
        public string Key { get => Id; set { } }

        public string Id => "S3Streaming";

        public bool CanDeliver(IResource route, IDestination destination)
        {
            if (route is IOrderable) return false;
            if (!(destination is S3ObjectDestination)) return false;
            if (route is IAsset) return true;
            if (!(route is IStreamable)) return false;

            return true;
        }

        public async Task<IResource> Deliver(IDelivery delivery, bool overwrite = false)
        {
            S3Delivery s3Delivery = delivery as S3Delivery;
            WebRoute s3Route = WebRoute.Create(s3Delivery.Destination.Uri);

            IStreamable streamable = delivery.Route as IStreamable;
            if (streamable == null && delivery.Route is IAsset)
                streamable = (delivery.Route as IAsset).GetStreamable();

            if (streamable == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", delivery.Route.Uri));

            if (!overwrite && streamable.ContentLength > 0 &&
               Convert.ToUInt64(s3Route.ContentLength) == streamable.ContentLength)
            {
                logger.LogDebug("Object {0} exists with the same size. Skipping transfer", s3Route.Uri);
                return s3Route;
            }
            s3Route = await StreamToS3Object(streamable, s3Route, overwrite);
            // if (streamable.ContentLength > 0 && Convert.ToUInt64(s3Route.ContentLength) != streamable.ContentLength)
                // throw new InvalidDataException(string.Format("Data transferred size ({0}) does not correspond with stream content length ({1})", s3Route.ContentLength, streamable.ContentLength));
            return s3Route;
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            if (!CanDeliver(route, destination)) return null;

            // Let's make a cost as MB to download
            int cost = 1000;
            try
            {
                if (route.ContentLength > 0)
                    cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);
            }
            catch (Exception e)
            {
                logger.LogWarning("Error trying to get size of the node {0} : {1}", route.Uri, e.Message);
            }

            return new S3Delivery(this, route, destination as S3ObjectDestination, cost);
        }

        private async Task<WebRoute> StreamToS3Object(IStreamable streamable, WebRoute s3Resource, bool overwrite = false)
        {
            try
            {
                // in case source is also S3
                if ( streamable is WebRoute && (streamable as WebRoute).Request is S3WebRequest)
                {
                    S3WebRequest s3CopyWebRequest = (S3WebRequest)(streamable as WebRoute).Request.CloneRequest(streamable.Uri);
                    s3CopyWebRequest.Method = System.Net.S3.S3RequestMethods.Copy;
                    s3CopyWebRequest.CopyTo = s3Resource.Uri;
                    await s3CopyWebRequest.GetResponseAsync().ConfigureAwait(false);
                    return WebRoute.Create(s3Resource.Uri);
                }

                // TODO Try a resume
                S3WebRequest s3WebRequest = (S3WebRequest)(s3Resource.Request as S3WebRequest).CloneRequest(s3Resource.Uri);
                var tx = new TransferUtility(s3WebRequest.S3Client);
                TransferUtilityUploadRequest ur = new TransferUtilityUploadRequest();
                SetRequestParametersWithUri(s3Resource.Uri, ur);

                Stream contentStream = await streamable.GetStreamAsync().ConfigureAwait(false);
                ur.InputStream = contentStream;
                ur.ContentType = streamable.ContentType.MediaType;
                await tx.UploadAsync(ur);

                // s3WebRequest.Method = "POST";
                // s3WebRequest.ContentLength = (long)streamable.ContentLength;
                // s3WebRequest.ContentType = streamable.ContentType.MediaType;
                // var uploadStream = await s3WebRequest.GetRequestStreamAsync();
                // Stream contentStream = await streamable.GetStreamAsync();
                // var writingTask = contentStream.CopyToAsync(uploadStream);
                // var responseTask = s3WebRequest.GetResponseAsync();
                // await writingTask;
                // uploadStream.Close();
                // S3ObjectWebResponse<PutObjectResponse> s3WebResponse = (S3ObjectWebResponse<PutObjectResponse>)await responseTask;
                return WebRoute.Create(s3Resource.Uri);
            }
            catch (WebException we)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(we.Response.GetResponseStream()))
                        logger.LogDebug(sr.ReadToEnd());
                }
                catch { }
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void SetRequestParametersWithUri(Uri uri, TransferUtilityUploadRequest ur)
        {
            ur.BucketName = S3UriParser.GetBucketName(uri);
            ur.Key = S3UriParser.GetKey(uri);
        }
    }

}