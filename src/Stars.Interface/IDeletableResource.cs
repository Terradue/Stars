using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IDeletableResource : IStreamResource
    {
        Task DeleteAsync(CancellationToken ct);
    }
}
