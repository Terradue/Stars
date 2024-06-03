// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IRouter.cs

using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router
{
    public interface IRouter : IPlugin
    {
        string Label { get; }

        bool CanRoute(IResource node);

        Task<IResource> RouteAsync(IResource node, CancellationToken ct);

        Task<IResource> RouteLinkAsync(IResource resource, IResourceLink childLink, CancellationToken ct);
    }
}
