// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PluginPriorityAttribute.cs

using System;

namespace Terradue.Stars.Services.Plugins
{
    public class PluginPriorityAttribute : Attribute
    {
        private readonly int v;

        public PluginPriorityAttribute(int v)
        {
            this.v = v;
        }

        public int Priority => v;
    }
}
