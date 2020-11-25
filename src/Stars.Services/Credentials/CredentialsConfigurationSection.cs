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