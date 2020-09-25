using System;
using Microsoft.Extensions.Configuration;

namespace Terradue.Stars.Interface
{
    public interface IPlugin
    {
        void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider);
    }
}