// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AmazonCustomS3Config.cs

using Amazon.S3;

namespace Terradue.Stars.Services.Resources
{
    internal class AmazonCustomS3Config : AmazonS3Config
    {
        private string _region;
        private string _serviceURL;

        public override string DetermineServiceURL()
        {
            if (!string.IsNullOrEmpty(_serviceURL))
            {
                return _serviceURL;
            }
            return base.DetermineServiceURL();
        }

        internal void ForceCustomRegion(string region)
        {
            _region = region;
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region);
        }

        internal void SetServiceURL(string serviceURL)
        {
            _serviceURL = serviceURL;
        }
    }
}
