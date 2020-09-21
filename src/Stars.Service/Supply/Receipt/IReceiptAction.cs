using System.Threading.Tasks;

namespace Stars.Service.Supply.Receipt
{
    public interface IReceiptAction
    {
        bool CanReceive(NodeInventory deliveryForm);

        Task<NodeInventory> Receive(NodeInventory deliveryForm);
    }
}