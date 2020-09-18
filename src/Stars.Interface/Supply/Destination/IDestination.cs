using System;
using Stars.Interface.Router;

namespace Stars.Interface.Supply.Destination
{
    public interface IDestination
    {
        Uri Uri { get; }

        Uri RelativeUri(IRoute to);
        IDestination RelativeDestination(IRoute from, IRoute to);
        IDestination To(IRoute route);
    }
}