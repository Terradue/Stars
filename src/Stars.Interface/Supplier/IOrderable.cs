using System;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IOrderable : IResource
    {
        string Id { get; }

        Uri OriginUri { get; }

        ISupplier Supplier { get; }
    }
}
