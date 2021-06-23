using System.Net;
using System.Net.S3;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Terradue.Systems.Charter.Supervisor.Tests.Fixtures
{
    public class WebRequestFixture
    {
        public WebRequestFixture(ILogger<System.Net.S3.S3WebRequest> logger, Amazon.Extensions.NETCore.Setup.AWSOptions options)
        {
            WebRequest.RegisterPrefix("s3", new S3WebRequestCreate(logger, options));
            UriParser.Register(new S3UriParser(), "s3", -1);
        }
    }
}