using System.Threading.Tasks;
using Xunit;

namespace Terradue.Systems.Charter.Supervisor.Tests.Fixtures
{
    public class StacFileStoreServiceFixture 
    {
        public StacFileStoreServiceFixture(StacFileStoreService stacFileStoreService)
        {
            StacFileStoreService = stacFileStoreService;
        }

        public StacFileStoreService StacFileStoreService { get; }
    }
}