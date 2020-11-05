using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAsset : IResource
    {
        string Label { get; }

        IEnumerable<string> Roles { get; }

        IStreamable GetStreamable();
    }
}