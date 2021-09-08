using System;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;

namespace Stars.Tests
{
    public class S3Tests : IClassFixture<WebRequestFixture>
    {
        [Fact]
        public void Test1()
        {
            // S3ObjectDestination s3ObjectDestination = new S3ObjectDestination(new Uri("s3://cpe-production-catalog/calls/call-841/datasets/DS_PHR1A_202109071021559_FR1_PX_E001N06_1017_01793/DS_PHR1A_202109071021559_FR1_PX_E001N06_1017_01793.json"));
            // WebRoute webRoute = WebRoute.Create(new Uri("file:///tmp/c1eb4243-935e-4ab9-8192-9396efb7c44a/5915740201/IMG_PHR1A_PMS_001/DIM_PHR1A_PMS_202109071022279_ORT_5915740201.XML"));
            // var newDestination = s3ObjectDestination.To(webRoute,);
        }
    }
}
