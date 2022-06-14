using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface
{
    public interface ICatalog : IResource
    {
        string Label { get; }
        
        string Id { get; }

        IReadOnlyList<IResourceLink> GetLinks();

        IReadOnlyList<IResource> GetRoutes(IRouter router);
    }
}