using System;
using Microsoft.Extensions.Configuration;

namespace Terradue.Stars.Interface
{
    public interface IPlugin
    {
        int Priority { get; set; }
        string Key { get; set; }

        void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider);
    }
}