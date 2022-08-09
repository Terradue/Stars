using System;
using System.Net.Mime;

namespace Terradue.Stars.Interface
{
    public interface IResourceLink : IResource
    {
        string Relationship { get; }

        string Title { get; }

    }
}