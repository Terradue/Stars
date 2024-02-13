using System;
using Amazon.S3;

namespace Terradue.Stars.Services.Resources
{
    internal class AmazonCustomS3Config: AmazonS3Config
    {
        private string _region;
        private string _serviceURL;

        public override string DetermineServiceURL()
        {
            if ( !string.IsNullOrEmpty(_serviceURL) )
            {
                return _serviceURL;
            }
            return base.DetermineServiceURL();
        }

        internal void ForceCustomRegion(string region)
        {
            _region = region;
            base.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region);
        }

        internal void SetServiceURL(string serviceURL)
        {
            _serviceURL = serviceURL;
            this.ServiceURL = _serviceURL;
        }
    }
}