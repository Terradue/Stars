using System;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Services.Resources;
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

        public LocalStackFixture(IOptions<LocalStackOptions> options, IS3ClientFactory s3ClientFactory)
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

            if (s3ClientFactory != null)
            {
                S3Url s3Url = S3Url.Parse("s3://test");
                var credentials = s3ClientFactory.CreateCredentials(s3Url);
                if (credentials != null)
                {
                    var creds = credentials.GetCredentials();
                    localStackBuilder.WithEnvironment("AWS_ACCESS_KEY_ID", creds.AccessKey)
                        .WithEnvironment("AWS_SECRET_ACCESS_KEY", creds.SecretKey);
                }
            }

            _localStackContainer = localStackBuilder.Build();
            this.options = options;
        }
        public async Task InitializeAsync()
        {
            if (options.Value.Enabled)
            {
                // await _network.CreateAsync();
                await _localStackContainer.StartAsync();
            }
        }

        public async Task DisposeAsync()
        {
            if (options.Value.Enabled)
            {
                await _localStackContainer.StopAsync();
                // await _network.DeleteAsync();
            }
        }
    }
}