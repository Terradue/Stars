using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router
{
    public interface IRouter : IPlugin
    {
        string Label { get; }

        bool CanRoute(IRoute node);

        Task<IRoute> Route(IRoute node);
    }
}