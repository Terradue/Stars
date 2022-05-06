using System;
using System.Collections.Generic;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services
{
    public static class StarsConfigurationExtensions
    {
        public static IStarsBuilder UseDefaultConfiguration(this IStarsBuilder starsBuilder, IConfiguration configuration)
        {
            starsBuilder.Services.AddSingleton<IConfiguration>(configuration);
            starsBuilder.UseCredentialsOptions(configuration.GetSection("Credentials"));
            starsBuilder.UsePluginsOptions(configuration.GetSection("Plugins"));
            // Only for retro-compatibility
            starsBuilder.UseMultiS3Options(configuration);
            return starsBuilder;
        }

        public static IStarsBuilder UseCredentialsOptions(this IStarsBuilder starsBuilder, IConfigurationSection configurationSection)
        {
            // Add Credentials from config
            var credOptions = configurationSection.Get<Dictionary<string, CredentialsOption>>();
            starsBuilder.Services.Configure<CredentialsOptions>(co => co.Load(credOptions));
            return starsBuilder;
        }

        public static IStarsBuilder UseMultiS3Options(this IStarsBuilder starsBuilder, IConfiguration configuration)
        {
            // Add MultiS3 from config
            var s3Section = configuration.GetSection("S3");
            starsBuilder.Services.Configure<S3Options>(c =>
            {
                s3Section.Bind(c);
                c.ConfigurationSection = s3Section;
                c.RootConfiguration = configuration;
            });
            starsBuilder.Services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            starsBuilder.Services.AddAWSService<IAmazonS3>();
            return starsBuilder;
        }

        public static IStarsBuilder UsePluginsOptions(this IStarsBuilder starsBuilder, IConfigurationSection configurationSection)
        {
            var pluginsOptions = configurationSection.Get<PluginsOptions>();
            // Add Plugins from config
            starsBuilder.Services.Configure<PluginsOptions>(co => co.Load(pluginsOptions));
            return starsBuilder;
        }
    }
}