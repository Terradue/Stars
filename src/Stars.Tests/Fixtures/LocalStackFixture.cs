using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.OutputConsumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace Terradue.Systems.Charter.Supervisor.Tests.Fixtures
{
    public class LocalStackFixture : IAsyncLifetime
    {
        private readonly TestcontainersContainer _localStackContainer;
        private readonly IOptions<LocalStackOptions> options;

        public LocalStackFixture(IOptions<LocalStackOptions> options, Amazon.Extensions.NETCore.Setup.AWSOptions awsOptions)
        {
            var localStackBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack")
                .WithCleanUp(true)
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithEnvironment("DEFAULT_REGION", "eu-central-1")
                .WithEnvironment("SERVICES", "s3")
                .WithEnvironment("DOCKER_HOST", "unix:///var/run/docker.sock")
                .WithEnvironment("DEBUG", "1")
                .WithPortBinding(4566, 4566);

            if (awsOptions != null)
            {
                if (awsOptions.Credentials != null)
                {
                    var awsCreds = awsOptions.Credentials.GetCredentials();
                    localStackBuilder.WithEnvironment("AWS_ACCESS_KEY_ID", awsCreds.AccessKey)
                        .WithEnvironment("AWS_SECRET_ACCESS_KEY", awsCreds.SecretKey);
                }
            }

            _localStackContainer = localStackBuilder.Build();
            this.options = options;
        }
        public async Task InitializeAsync()
        {
            if (options.Value.Enabled)
                await _localStackContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            if (options.Value.Enabled)
                await _localStackContainer.StopAsync();
        }
    }
}