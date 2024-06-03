using System.Collections.Generic;
using System.Linq;
using Stac;
using Terradue.Stars.Interface.Extensions.TimeSeries;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    internal class EgmsTimeSeriesOperationStatus : IAbstractTimeSeriesOperationStatus<StacCollection>
    {
        private StacCollection _collection;
        private List<EgmsTimeSeriesImportTask> _statuses;

        public EgmsTimeSeriesOperationStatus(StacCollection collection, List<EgmsTimeSeriesImportTask> statuses)
        {
            _collection = collection;
            _statuses = statuses;
        }

        public string NamespaceId => _collection.GetProperty<string>("ts:ns_id");

        public string CollectionId => _collection.Id;

        public string OperationId => string.Join(",", _statuses.Select(s => s.JobId));

        public string Status => string.Join(",", _statuses.Select(s => s.Status));

        public bool IsCompleted => _statuses.All(s => s.Status == "Succeeded" || s.Status == "Failed" || s.Status == "Deleted");

        public IReadOnlyDictionary<string, string> Properties => throw new System.NotImplementedException();
    }
}
