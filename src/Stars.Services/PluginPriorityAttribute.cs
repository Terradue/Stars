using System;

namespace Terradue.Stars.Services
{
    public class PluginPriorityAttribute : Attribute
    {
        private int v;

        public PluginPriorityAttribute(int v)
        {
            this.v = v;
        }

        public int Priority => v;
    }
}