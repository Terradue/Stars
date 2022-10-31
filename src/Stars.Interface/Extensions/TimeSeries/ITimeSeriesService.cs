using System;
using System.Threading;
using System.Threading.Tasks;
using Stac;
using Stac.Common;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface ITimeSeriesService
    {
        Task<StacObjectLink> CreateNewTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> request, CancellationToken cancellationToken);

        Task<StacObjectLink> UpdateTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> request, CancellationToken cancellationToken);

        Task<StacObjectLink> PatchTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<Patch> request, CancellationToken cancellationToken);

        Task DeleteTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> request, CancellationToken cancellationToken);
    }
}