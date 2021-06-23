using Xunit;

namespace Terradue.Systems.Charter.Supervisor.Tests.Fixtures
{
    [CollectionDefinition(nameof(S3TestCollection))]
    public class S3TestCollection : ICollectionFixture<LocalStackFixture>, ICollectionFixture<WebRequestFixture>
    {
    }
}