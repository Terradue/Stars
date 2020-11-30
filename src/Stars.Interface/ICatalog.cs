using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface ICatalog : IResource
    {
        string Label { get; }
        string Id { get; }

        IReadOnlyList<IResource> GetRoutes();
    }
}