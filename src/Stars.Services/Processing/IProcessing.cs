using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Processing
{
    public interface IProcessing : IPlugin
    {
        bool CanProcess(NodeInventory deliveryForm);

        Task<NodeInventory> Process(NodeInventory deliveryForm);
    }
}