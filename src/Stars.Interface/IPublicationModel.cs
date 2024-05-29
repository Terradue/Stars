using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface IPublicationModel
    {
        string Url { get; }

        string CatalogId { get; }

        List<StacLink> AdditionalLinks { get; }

        List<ISubject> Subjects { get; }

        string Collection { get; }

        int Depth { get; }
        
        List<string> AssetsFilters { get; }
    }
}