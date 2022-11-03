using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stac;
using Stac.Common;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface IAbstractTimeSeriesOperationStatus<T>
    {
        string NamespaceId { get; }

        string CollectionId { get; }

        string OperationId { get; }

        string Status { get; }

        bool IsCompleted { get; }

        IReadOnlyDictionary<string, string> Properties { get; }
    }
}