using Newtonsoft.Json;

namespace Terradue.Stars.Service
{
    [JsonObject]
    public class CredentialsConfigurationSection
    {
        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string UriPrefix { get; set; }

        [JsonProperty]
        public string Username { get; set; }

        [JsonProperty]
        public string Password { get; set; }
    }
}