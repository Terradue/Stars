// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IItem.cs

using System.Collections.Generic;
using GeoJSON.Net.Geometry;

namespace Terradue.Stars.Interface
{
    public interface IItem : IResource, IAssetsContainer
    {
        string Title { get; }
        string Id { get; }
        IGeometryObject Geometry { get; }
        IDictionary<string, object> Properties { get; }
        Itenso.TimePeriod.ITimePeriod DateTime { get; }
        IReadOnlyList<IResourceLink> GetLinks();
    }
}
