using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Data.Suppliers.PlanetScope
{
    [JsonObject]
    public class AssetInformation
    {
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonProperty("_links")]
        public AssetLinks Links { get; set; }

        [JsonProperty("_permissions")]
        public List<string> Permissions { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("md5_digest")]
        public string MD5Digest { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }


    [JsonObject]
    public class AssetLinks
    {
        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("activate")]
        public string Activate { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}