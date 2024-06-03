using System.Collections.Generic;

namespace Terradue.Stars.Interface
{
    public interface IItemCollection : ICollection<IItem>
    {
        IReadOnlyList<IResourceLink> GetLinks();
    }
}
