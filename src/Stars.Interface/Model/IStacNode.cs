using System.Threading.Tasks;
using Stac;
using Stars.Interface.Router;

namespace Stars.Interface.Model
{
    public interface IStacNode : INode
    {
        Task<IStacObject> GetStacObject();
    }
}