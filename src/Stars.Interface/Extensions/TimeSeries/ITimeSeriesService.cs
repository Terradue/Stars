using System.Threading;
using System.Threading.Tasks;
using Stac;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface ITimeSeriesService
    {
        Task<StacLink> CreateTimeSeriesAsync(IAbstractTimeSeriesCreationRequest ingestionRequest, CancellationToken cancellationToken);
    }
}