using Xunit;

namespace Stars.Tests
{
    [CollectionDefinition(nameof(StarsCollection))]
    public class StarsCollection : ICollectionFixture<Logging>
    {
    }
}
