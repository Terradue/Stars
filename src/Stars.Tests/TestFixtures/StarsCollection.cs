using Xunit;
using System.Net;
using System.Net.Http;

namespace Stars.Tests
{
    [CollectionDefinition(nameof(StarsCollection))]
    public class StarsCollection : ICollectionFixture<Logging>
    {
    }
}