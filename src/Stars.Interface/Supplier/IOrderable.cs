using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IOrderable : IResource
    {
        string Id { get; }

        Uri OriginUri { get; }

        ISupplier Supplier { get; }
    }
}