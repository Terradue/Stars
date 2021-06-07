using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Persistence
{
    public interface IPersistenceService : IStarsService
    {
        Task Init(bool @new = false);
        Task<T> Commit<T>(T resource) where T : IResource;
        
    }
}