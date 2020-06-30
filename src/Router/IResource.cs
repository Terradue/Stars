using System;
using System.IO;
using System.Net.Mime;
using Stac;

namespace Stars.Router
{
    public interface IResource
    {
        string Label { get; }
        ContentType ContentType { get; }
        Uri Uri { get; }

        string Id { get; }

        ResourceType ResourceType { get; }
        string Filename { get; }

        string ReadAsString();

        IStacObject ReadAsStacObject();

        Stream GetAsStream();
    }
}