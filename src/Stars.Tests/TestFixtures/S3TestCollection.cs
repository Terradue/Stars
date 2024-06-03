using Stars.Tests.TestFixtures;
using Xunit;

namespace Stars.Tests
{

    [CollectionDefinition(nameof(S3TestCollection))]
    public class S3TestCollection : ICollectionFixture<LocalStackFixture>, ICollectionFixture<Logging>
    {

    }

}
