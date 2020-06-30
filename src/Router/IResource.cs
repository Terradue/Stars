using System;
using System.Net.Mime;

namespace Stars.Router
{
    public interface IResource
    {
        string Label { get; }
        ContentType ContentType { get; }
        Uri Uri { get; }

        string ReadAsString();
    }
}