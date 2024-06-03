using System.Collections.Generic;
using Stac.Collection;

namespace Terradue.Stars.Interface
{
    public interface ICollection : ICatalog, IAssetsContainer
    {
        StacExtent Extent { get; }

        IDictionary<string, object> Properties { get; }
    }
}
