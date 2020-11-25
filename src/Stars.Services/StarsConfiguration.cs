using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services
{
    public class StarsConfiguration
    {
        public static StarsConfiguration Configuration => new StarsConfiguration();

        public Dictionary<string, CredentialsOption> CredentialsOptions { get; set; }

        public PluginsOptions PluginsOptions { get; set; }

        public StarsConfiguration UseCredentialsOptions(IConfigurationSection credentialsConfigSection)
        {
            CredentialsOptions = credentialsConfigSection.Get<Dictionary<string, CredentialsOption>>();
            return this;
        }

        public StarsConfiguration UseCredentialsOptions(CredentialsOptions credentialsOptions)
        {
            CredentialsOptions = credentialsOptions;
            return this;
        }

        public StarsConfiguration UseGlobalConfiguration(IConfiguration configuration)
        {
            UseCredentialsOptions(configuration.GetSection("Credentials"));
            UsePluginsOptions(configuration.GetSection("Plugins"));
            return this;
        }

        private StarsConfiguration UsePluginsOptions(IConfigurationSection configurationSection)
        {
            PluginsOptions = configurationSection.Get<PluginsOptions>();
            return this;
        }
    }
}