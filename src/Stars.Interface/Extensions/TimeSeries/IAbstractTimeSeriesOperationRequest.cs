using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface IAbstractTimeSeriesOperationRequest<T>
    {
        string NamespaceId { get; }

        string CollectionId { get; }

        Task<T> GetOperationPayloadAsync(CancellationToken cancellationToken);

        IReadOnlyDictionary<string, string> Properties { get; }
    }
}
