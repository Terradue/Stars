using System.IO;
using System.Runtime.Loader;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Services;
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
            var awsOptions = Configuration.GetAWSOptions("AWS");
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            chain.TryGetAWSCredentials(awsOptions.Profile, out awsCredentials);
            awsOptions.Credentials = awsCredentials;
            services.AddDefaultAWSOptions(awsOptions);
            services.Configure<LocalStackOptions>(Configuration.GetSection("LocalStack"));
            services.AddStarsManagedServices((provider, config) => config
                .UseGlobalConfiguration(Configuration)
            );
            services.LoadConfiguredStarsPlugin(_ => AssemblyLoadContext.Default);
        }

        public void Configure(ILoggerFactory loggerfactory, ITestOutputHelperAccessor accessor)
        {
            loggerfactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
        }

        public IConfiguration GetApplicationConfiguration()
        {
            var configFile = new FileInfo(Path.Join(@"../../../../Stars.Data", "stars-data.json"));
            configFile.OpenRead();
            var builder = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: true)
                .AddNewtonsoftJsonFile(configFile.FullName, optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return builder;
        }
    }
}