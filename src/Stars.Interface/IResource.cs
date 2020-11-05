using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IResource
    {
        Uri Uri { get; }
        ContentType ContentType { get; }
        ResourceType ResourceType { get; }
        ulong ContentLength { get; }
        ContentDisposition ContentDisposition { get; }
        
    }
}