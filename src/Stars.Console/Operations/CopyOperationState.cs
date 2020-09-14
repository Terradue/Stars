using Stars.Interface.Supply.Destination;

namespace Stars.Operations
{
    internal class CopyOperationState
    {
        public CopyOperationState(int depth, IDestination destination)
        {
            Depth = depth;
            Destination = destination;
        }

        public int Depth { get; internal set; }
        public IDestination Destination { get; internal set; }
    }
}