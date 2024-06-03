using System.Collections.Generic;

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
