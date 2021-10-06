using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        }

        public void Configure(ILoggerFactory loggerfactory, ITestOutputHelperAccessor accessor)
        {
            loggerfactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
        }

        public IConfiguration GetApplicationConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return builder;
        }
    }
}