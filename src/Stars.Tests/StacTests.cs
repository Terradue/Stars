using System;
using System.IO;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;

namespace Stars.Tests
{
    public class StacTests : IClassFixture<WebRequestFixture>
    {
        [Fact]
        public void FolderRoute()
        {
            WebRoute route = WebRoute.Create(new Uri("file://" + Path.Join(Environment.CurrentDirectory, "../../../In/stacRoute/catalog.json")));
            StacRouter router = new StacRouter(null); 
            Assert.True(router.CanRoute(route));
            route = WebRoute.Create(new Uri(Path.Join(Environment.CurrentDirectory, "../../../In/stacRoute/catalog.json")));
            Assert.True(router.CanRoute(route));
        }
    }
}
