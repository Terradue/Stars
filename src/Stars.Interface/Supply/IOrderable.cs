using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Interface.Router;

namespace Stars.Interface.Supply
{
    public interface IOrderable : IRoute
    {
        string Id { get; }

        Uri OriginUri { get; }
    }
}