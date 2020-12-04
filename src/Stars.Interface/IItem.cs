using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface IItem : IResource, IAssetsContainer, IStreamable
    {
        string Label { get; }
        string Id { get; }
        IGeometryObject Geometry { get; }
        IDictionary<string, object> Properties { get; }

        IReadOnlyList<IResourceLink> GetLinks();
    }
}