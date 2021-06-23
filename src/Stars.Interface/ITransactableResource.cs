using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface ITransactableResource : IResource, IStreamable, ICloneable
    {
        string Id { get; }
        IResourceLink Parent { get; }
    }
}