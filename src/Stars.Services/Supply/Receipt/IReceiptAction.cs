using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supply.Receipt
{
    public interface IReceiptAction : IPlugin
    {
        bool CanReceive(NodeInventory deliveryForm);

        Task<NodeInventory> Receive(NodeInventory deliveryForm);
    }
}