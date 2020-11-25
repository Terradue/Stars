using System;

namespace Terradue.Stars.Interface
{
    public interface IPluginOption
    {
        string Type { get; }
        int Priority { get; }
    }
}