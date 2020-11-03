using System;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestination
    {
        Uri Uri { get; }
        IDestination To(IRoute route, string relPath = null);
        void PrepareDestination();

        string ToString();
    }
}