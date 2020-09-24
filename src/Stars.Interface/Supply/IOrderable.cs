using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supply
{
    public interface IOrderable : IRoute
    {
        string Id { get; }

        Uri OriginUri { get; }
    }
}