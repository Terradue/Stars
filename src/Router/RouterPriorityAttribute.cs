using System;

namespace Stars.Router
{
    internal class RouterPriorityAttribute : Attribute
    {
        private int v;

        public RouterPriorityAttribute(int v)
        {
            this.v = v;
        }

        public int Priority => v;
    }
}