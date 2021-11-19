using System;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace Stars.Tests
{
    public class LocalStackFixture : IAsyncLifetime
    {
        private readonly TestcontainersContainer _localStackContainer;
        private readonly IOptions<LocalStackOptions> options;
        private readonly string _networkName;
        private readonly IDockerNetwork _network;

        public LocalStackFixture(IOptions<LocalStackOptions> options, Amazon.Extensions.NETCore.Setup.AWSOptions awsOptions)
        {
            // _networkName = Guid.NewGuid().ToString();
            // var networkLabel = Guid.NewGuid().ToString();

            // // When
            // _network = new TestcontainersNetworkBuilder()
            //   .WithName(_networkName)
            //   .WithLabel("label", networkLabel)
            //   .Build();

            var localStackBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack")
                .WithCleanUp(true)
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithEnvironment("DEFAULT_REGION", "eu-central-1")
                .WithEnvironment("SERVICES", "s3")
                .WithEnvironment("DOCKER_HOST", "unix:///var/run/docker.sock")
                .WithEnvironment("DEBUG", "1")
                .WithPortBinding(4566, 4566)
                // .WithNetwork(_network)
                .WithName("localstack");

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
            if (options.Value.Enabled){
                // await _network.CreateAsync();
                await _localStackContainer.StartAsync();
            }
        }

        public async Task DisposeAsync()
        {
            if (options.Value.Enabled){
                await _localStackContainer.StopAsync();
                // await _network.DeleteAsync();
            }
        }
    }
}