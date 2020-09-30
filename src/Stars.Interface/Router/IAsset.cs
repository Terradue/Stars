using System.Collections.Generic;

namespace Terradue.Stars.Interface.Router
{
    public interface IAsset : IRoute
    {
        string Label { get; }

        IEnumerable<string> Roles { get; }

        IStreamable GetStreamable();
    }
}