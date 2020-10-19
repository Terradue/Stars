using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Services
{
    public class PluginList<T> : Dictionary<string, T> where T : class, IPlugin
    {

        public PluginList(IEnumerable<T> plugins)
        {
            foreach (var plugin in plugins)
            {
                this.Add(plugin.Key, plugin);
            }
        }

        public new ValueCollection Values
        {
            get
            {
                return new ValueCollection(this.OrderBy(v => v.Value.Priority).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
        }

        internal void Add(T plugin)
        {
            this.Add(plugin.Key, plugin);
        }
    }
}