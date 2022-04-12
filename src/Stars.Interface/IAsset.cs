using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface IAsset : IResource
    {
        string Title { get; }

        IReadOnlyList<string> Roles { get; }

        IReadOnlyDictionary<string, object> Properties { get; }
        
        IEnumerable<IAsset> Alternates { get; }

        Task CacheHeaders(bool force = false);
        
        IStreamable GetStreamable();

    }
}