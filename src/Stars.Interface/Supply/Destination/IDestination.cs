using System;
using Stars.Interface.Router;

namespace Stars.Interface.Supply.Destination
{
    public interface IDestination
    {
        Uri Uri { get; }

        IDestination RelativePath(IRoute route, IRoute subroute);
        IDestination To(IRoute route);
    }
}