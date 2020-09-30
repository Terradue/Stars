using System;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supplier.Destination
{
    public interface IDestination
    {
        Uri Uri { get; }

        IDestination RelativeTo(IRoute from, IRoute to);
        IDestination To(IRoute route);

        IDestination To(string relPath);
        void Create();
    }
}