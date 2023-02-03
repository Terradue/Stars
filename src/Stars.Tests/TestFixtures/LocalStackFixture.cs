using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Services.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Stars.Tests.TestFixtures
{
    public class LocalStackFixture : IAsyncLifetime
    {
        private readonly TestcontainersContainer _localStackContainer;
        private readonly IOptions<LocalStackOptions> options;
        private readonly IS3ClientFactory _s3ClientFactory;

        public string LocalstackUri => "http://localhost:4566";

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
                .WithImage("localstack/localstack:1.3.1")
                .WithCleanUp(true)
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithEnvironment("SERVICES", "s3")
                .WithEnvironment("DOCKER_HOST", "unix:///var/run/docker.sock")
                .WithEnvironment("DEBUG", "1")
                .WithPortBinding(4566, 4566)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable(4566)
                    .AddCustomWaitStrategy(new LocalstackContainerHealthCheck(LocalstackUri))
                )
                .WithName("localstack");


            if (s3ClientFactory != null)
            {
                S3Url s3Url = S3Url.Parse("s3://test");
                var credentials = s3ClientFactory.GetConfiguredCredentials(s3Url);
                if (credentials != null)
                {
                    var creds = credentials.GetCredentials();
                    localStackBuilder.WithEnvironment("AWS_ACCESS_KEY_ID", creds.AccessKey)
                        .WithEnvironment("AWS_SECRET_ACCESS_KEY", creds.SecretKey);
                }
            }

            _localStackContainer = localStackBuilder.Build();
            this.options = options;
            _s3ClientFactory = s3ClientFactory;
        }

        public async Task InitializeAsync()
        {
            if (options.Value.Enabled)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                await _localStackContainer.StartAsync(cts.Token);
            }
        }

        public async Task DisposeAsync()
        {
            if (options.Value.Enabled)
            {
                await _localStackContainer.StopAsync();
            }
        }
    }
}