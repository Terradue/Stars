using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class S3DestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;
        private readonly IS3ClientFactory s3ClientFactory;

        public S3DestinationGuide(ILogger<S3DestinationGuide> logger,
                                  IS3ClientFactory s3ClientFactory)
        {
            this.logger = logger;
            this.s3ClientFactory = s3ClientFactory;
        }

        public int Priority { get; set; }

        public string Key { get => Id; set { } }

        public string Id => "S3";

        public bool CanGuide(string output, IResource route)
        {
            S3Url s3Url = null;
            try
            {
                s3Url = S3Url.Parse(output);
                s3ClientFactory.CreateAndLoadAsync(s3Url).GetAwaiter().GetResult();
                return true;
            }
            catch (AmazonS3Exception e)
            {
                if ( e.StatusCode == HttpStatusCode.NotFound)
                {
                    return true;
                }
                logger.LogDebug(e.ResponseBody);
                return false;
            }
            catch (Exception e)
            {
                logger.LogWarning(e.Message);
                return false;
            }
            
        }

        public Task<IDestination> Guide(string output, IResource route)
        {
            // TODO Check bucket exists
            return Task.FromResult<IDestination>(S3ObjectDestination.Create(output, route));
        }
    }
}