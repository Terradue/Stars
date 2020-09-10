using System;

namespace Stars.Service
{
    internal class PluginPriorityAttribute : Attribute
    {
        private int v;

        public PluginPriorityAttribute(int v)
        {
            this.v = v;
        }

        public int Priority => v;
    }
}