// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ICatalog.cs

using System.Collections.Generic;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface
{
    public interface ICatalog : IResource
    {
        string Title { get; }

        string Id { get; }

        IReadOnlyList<IResourceLink> GetLinks();

        IReadOnlyList<IResource> GetRoutes(IRouter router);
    }
}
