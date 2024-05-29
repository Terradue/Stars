using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Collection;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface
{
    public interface ICollection : ICatalog, IAssetsContainer
    {
        StacExtent Extent { get; }

        IDictionary<string, object> Properties { get; }
    }
}