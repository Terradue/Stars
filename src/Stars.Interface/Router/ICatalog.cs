using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;

namespace Terradue.Stars.Interface.Router
{
    public interface ICatalog : IRoute
    {
        string Label { get; }
        string Id { get; }

        IList<IRoute> GetRoutes();
    }
}