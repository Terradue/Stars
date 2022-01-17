using Xunit;
using System.Net;
using System.Net.Http;

namespace Stars.Tests
{
    [CollectionDefinition(nameof(Atom2StacCollection))]
    public class Atom2StacCollection : ICollectionFixture<Logging>
    {
    }
}