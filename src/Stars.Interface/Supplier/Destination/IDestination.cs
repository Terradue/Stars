using System;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestination : ILocatable
    {
        Uri Uri { get; }

        IDestination To(IResource route, string relPath = null);
        void PrepareDestination();

        string ToString();
    }
}