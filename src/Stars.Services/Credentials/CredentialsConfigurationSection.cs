// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: CredentialsConfigurationSection.cs

using Newtonsoft.Json;

namespace Terradue.Stars.Services.Credentials
{
    [JsonObject]
    public class CredentialsConfigurationSection
    {
        [JsonProperty]
        public string AuthType { get; set; }

        [JsonProperty]
        public string UriPrefix { get; set; }

        [JsonProperty]
        public string Username { get; set; }

        [JsonProperty]
        public string Password { get; set; }
    }
}
