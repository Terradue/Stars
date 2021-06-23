using Terradue.Stars.Interface.Persistence;

namespace Terradue.Stars.Services.Persistence.Stac
{
    public interface IStacPersistenceService : IPersistenceService
    {
        StacOptions StacOptions { get; }
    }
}