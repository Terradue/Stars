using Xunit;
using System.Net;
using System.Net.Http;
using Stars.Tests.TestFixtures;

namespace Stars.Tests
{

    [CollectionDefinition(nameof(S3TestCollection))]
    public class S3TestCollection : ICollectionFixture<LocalStackFixture>, ICollectionFixture<Logging>
    {

    }

}