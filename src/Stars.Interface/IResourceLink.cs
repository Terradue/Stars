using System;
using System.Net.Mime;

namespace Terradue.Stars.Interface
{
    public interface IResourceLink
    {
        Uri Uri { get; }

        string Relationship { get; }

        ContentType ContentType { get; }

        string Title { get; }

        ulong ContentLength { get; }
    }
}