using System;
using Stac;
using Terradue.Stars.Services.Model.Stac;
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
            StacCatalogNode node = StacCatalogNode.CreateUnlocatedNode(new StacCatalog("test", "test"));
            S3ObjectDestination s3ObjectDestination = S3ObjectDestination.Create("s3://cpe-production-catalog/", node);
            Assert.Equal("s3://cpe-production-catalog/catalog.json", s3ObjectDestination.Uri.ToString());
            // var newDestination = s3ObjectDestination.To(webRoute,);
        }
    }
}
