namespace Stars.Operations
{
    internal class ListOperationState
    {
        public ListOperationState(string prefix, int depth)
        {
            Prefix = prefix;
            Depth = depth;
        }

        public string Prefix { get; internal set; }
        public int Depth { get; internal set; }
    }
}