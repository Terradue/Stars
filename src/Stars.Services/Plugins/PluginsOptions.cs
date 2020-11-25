using System.Collections.Generic;
using Newtonsoft.Json;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Plugins
{
    public class PluginsOptions : Dictionary<string, PluginsOption>
    {
        internal void Load(PluginsOptions pluginsOptions)
        {
            if (pluginsOptions == null) return;
            foreach (var pluginsOption in pluginsOptions)
            {
                this.Remove(pluginsOption.Key);
                this.Add(pluginsOption.Key, pluginsOption.Value);
            }
        }
    }

    public class PluginsOption
    {
        public PluginsOption()
        {
        }

        public string Assembly { get; set; }

        public Dictionary<string, PluginOption> Routers { get; set; }

        public Dictionary<string, SupplierPluginOption> Suppliers { get; set; }

        public Dictionary<string, PluginOption> Translators { get; set; }

        public Dictionary<string, PluginOption> Destinations { get; set; }

        public Dictionary<string, PluginOption> Carriers { get; set; }

        public Dictionary<string, PluginOption> Processings { get; set; }

    }

    public class PluginOption : IPluginOption
    {
        public PluginOption()
        {
            Priority = 50;
        }

        public string Type { get; set; }
        public int Priority { get; set; }
    }

    public class SupplierPluginOption : PluginOption
    {
        public string ServiceUrl { get; set; }
    }
}