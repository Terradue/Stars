using System.Threading.Tasks;

namespace Terradue.Stars.Service.Supply.Receipt
{
    public interface IReceiptAction
    {
        bool CanReceive(NodeInventory deliveryForm);

        Task<NodeInventory> Receive(NodeInventory deliveryForm);
    }
}