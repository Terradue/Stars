﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PluginsOptions.cs

using System.Collections.Generic;
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
                Remove(pluginsOption.Key);
                Add(pluginsOption.Key, pluginsOption.Value);
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
        }

        public string Type { get; set; }
        public int? Priority { get; set; }
    }

    public class SupplierPluginOption : PluginOption
    {
        public string ServiceUrl { get; set; }

        public string AccountFile { get; set; }

        public string ProjectId { get; set; }
    }
}
