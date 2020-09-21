using System;
using Stars.Interface.Router;

namespace Stars.Interface.Supply.Destination
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