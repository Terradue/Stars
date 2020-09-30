using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        bool CanProcess(IRoute deliveryForm);

        Task<IRoute> Process(IRoute deliveryForm);
    }
}