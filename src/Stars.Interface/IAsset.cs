using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAsset : IResource
    {
        string Label { get; }

        IEnumerable<string> Roles { get; }

        IDictionary<string, object> Properties { get; }

        IStreamable GetStreamable();
    }
}