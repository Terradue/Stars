using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IResource : ILocatable
    {
        ContentType ContentType { get; }
        ResourceType ResourceType { get; }
        ulong ContentLength { get; }
        ContentDisposition ContentDisposition { get; }

    }
}