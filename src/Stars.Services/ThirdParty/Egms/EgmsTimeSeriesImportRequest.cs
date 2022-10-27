using System.Runtime.Serialization;
using Stac;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    [DataContract]
    public class EgmsTimeSeriesImportRequest
    {
        [DataMember(Name = "collection")]
        public string Collection { get; internal set; }
        [DataMember(Name = "url")]
        public StacLink Url { get; internal set; }
        [DataMember(Name = "tsId")]
        public string TimeSeriesId { get; internal set; }
        [DataMember(Name = "namespaceId")]
        public string Format { get; internal set; }
    }
}