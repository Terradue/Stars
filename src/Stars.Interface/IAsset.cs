using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IAsset : IResource
    {
        string Title { get; }

        IReadOnlyList<string> Roles { get; }

        IReadOnlyDictionary<string, object> Properties { get; }

        IEnumerable<IAsset> Alternates { get; }

    }
}
