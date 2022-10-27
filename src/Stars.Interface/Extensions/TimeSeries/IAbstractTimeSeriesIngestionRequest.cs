using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface IAbstractTimeSeriesCreationRequest
    {
        StacLink Source { get; }

        string NamespaceId { get; }

        string TimeSeriesId { get; }

        string Collection { get; }

        IReadOnlyDictionary<string, string> Properties { get; }
    }
}