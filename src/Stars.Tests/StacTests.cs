using System;
using System.IO;
using System.Threading.Tasks;
using Stac;
using Stac.Exceptions;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Xunit;
using Microsoft.Extensions.Logging;
using MELT;
using Terradue.Stars.Interface;

namespace Stars.Tests
{
    public class StacTests
    {
        private readonly ITestLoggerFactory loggerFactory;
        private readonly ILogger<StacTests> logger;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public StacTests(IResourceServiceProvider resourceServiceProvider)
        {
            this.resourceServiceProvider = resourceServiceProvider;
            loggerFactory = TestLoggerFactory.Create();
            logger = loggerFactory.CreateLogger<StacTests>();
        }

        [Fact]
        public async Task FolderRouteAsync()
        {
            var route = await resourceServiceProvider.CreateStreamResourceAsync(new Uri("file://" + Path.Join(Environment.CurrentDirectory, "../../../In/stacRoute/catalog.json")));
            StacRouter router = new StacRouter(null); 
            Assert.True(router.CanRoute(route));
            route = await resourceServiceProvider.CreateStreamResourceAsync(new Uri(Path.Join(Environment.CurrentDirectory, "../../../In/stacRoute/catalog.json")));
            Assert.True(router.CanRoute(route));
        }

        [Fact]
        public async Task CatalogManuela()
        {
            var route = await resourceServiceProvider.CreateStreamResourceAsync(new Uri("file://" + Path.Join(Environment.CurrentDirectory, "../../../In/catalogManuela/WQ_CHL_S3B_20210918T023115.json")));
            StacRouter router = new StacRouter(null, null);
            Assert.False(router.CanRoute(route));
            Assert.Empty(loggerFactory.Sink.LogEntries);
            router = new StacRouter(null, logger);
            Assert.False(router.CanRoute(route));
            var log = Assert.Single(loggerFactory.Sink.LogEntries);
            Assert.Contains("Cannot read STAC object from", log.Message);
            route = await resourceServiceProvider.CreateStreamResourceAsync(new Uri(Path.Join(Environment.CurrentDirectory, "../../../In/catalogManuela/WQ_CHL_S3B_20210918T023115.json")));
            await Assert.ThrowsAsync<InvalidStacDataException>(async () => await router.Route(route));
        }
    }
}
