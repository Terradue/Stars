// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: LocalstackContainerHealthCheck.cs

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Stars.Tests.TestFixtures
{
    public class LocalstackContainerHealthCheck : IWaitUntil
    {
        private readonly string _endpoint;

        public LocalstackContainerHealthCheck(string endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<bool> UntilAsync(IContainer testContainer)
        {
            // https://github.com/localstack/localstack/pull/6716
            using var httpClient = new HttpClient { BaseAddress = new Uri(_endpoint) };
            JsonNode? result;
            try
            {
                result = await httpClient.GetFromJsonAsync<JsonNode>("/_localstack/health");
            }
            catch
            {
                return false;
            }

            if (result is null)
                return false;

            var s3Status = result["services"]?["s3"];
            if (s3Status is null)
                return false;

            if (s3Status.GetValue<string>() == "available")
                return true;

            return false;
        }


        public record Script(
            [property: JsonPropertyName("stage")] string Stage,
            [property: JsonPropertyName("state")] string State,
            [property: JsonPropertyName("name")] string Name
            );
    }
}
