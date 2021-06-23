
using Terradue.Systems.Charter.Supervisor.Server.Controllers.Users;
using Terradue.Systems.Charter.Supervisor.Tests.Fixtures;
using Xunit;

namespace Terradue.Systems.Charter.Supervisor.Tests.Api.User
{
    public class StacFileStoreTests : IClassFixture<StacFileStoreServiceFixture>
    {
        private readonly StacFileStoreServiceFixture localStacStoreFixture;

        public S3ProductsTest(StacFileStoreServiceFixture localCharterStacStoreFixture)
        {
            this.localStacStoreFixture = localCharterStacStoreFixture;
        }

        [Fact]
        public async void InitializeNewRootCatalog()
        {
            localStacStoreFixture.StacFileStoreService.ks();
        }

    }
}
