using System;
using System.IO;
using System.Runtime.Loader;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Services.Supplier.Carrier;
using Xunit;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Stars.Tests
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            Configuration = GetApplicationConfiguration();
            services.AddLogging(builder =>
                {
                    builder.AddConfiguration(Configuration.GetSection("Logging"));
                });
            services.AddOptions();
            var chain = new CredentialProfileStoreChain();
            services.Configure<LocalStackOptions>(Configuration.GetSection("LocalStack"));
            services.AddSingleton<IResourceServiceProvider, DefaultResourceServiceProvider>();
            services.AddSingleton<S3StreamingCarrier, S3StreamingCarrier>();
            services.AddStarsManagedServices(builder =>
                {
                    builder.UseDefaultConfiguration(Configuration);
                });
            services.LoadConfiguredStarsPlugin(_ => AssemblyLoadContext.Default);
        }

        public void Configure(IServiceProvider provider, ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor)
        {
            Assert.NotNull(accessor);
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor, delegate { return true; }));

            ILogger<Startup> logger = provider.GetService<ILogger<Startup>>();
            var root = (IConfigurationRoot)Configuration;
            logger.LogInformation(root.GetDebugView());
        }

        public IConfiguration GetApplicationConfiguration()
        {
            var configFile = new FileInfo(Path.Join(@"../../../../Stars.Data", "stars-data.json"));
            configFile.OpenRead();
            var builder = new ConfigurationBuilder()
                .AddNewtonsoftJsonFile("testsettings.json", optional: true)
                .AddNewtonsoftJsonFile(configFile.FullName, optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return builder;
        }
    }
}