using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class S3DestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;

        public S3DestinationGuide(ILogger<S3DestinationGuide> logger)
        {
            this.logger = logger;
        }

        public int Priority { get; set; }

        public string Key { get => Id; set { } }

        public string Id => "S3";

        public bool CanGuide(string output, IResource route)
        {
            try
            {
                return output.StartsWith("s3:/");
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