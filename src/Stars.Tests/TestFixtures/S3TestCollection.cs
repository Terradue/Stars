using Xunit;
using System.Net;
using System.Net.Http;

namespace Stars.Tests
{
    [CollectionDefinition(nameof(S3AWSTestCollection))]
    public class S3AWSTestCollection : ICollectionFixture<Logging>
    {

    }

    [CollectionDefinition(nameof(S3TestCollection))]
    public class S3TestCollection : ICollectionFixture<LocalStackFixture>, ICollectionFixture<Logging>
    {

    }

}