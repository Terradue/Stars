using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquareConfiguration
    {
        public GeosquareConfiguration()
        {
        }

        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        public string DefaultIndex { get; set; }

        public bool CreateIndex { get; set; }

        public string AuthorizationHeader { get; set; }

        public Dictionary<string, UriMap> UriMaps { get; set; }

        public Uri MapUri(Uri uri)
        {
            var mapping = UriMaps.FirstOrDefault(kvp =>
            {
                try
                {
                    Regex regex = new Regex(kvp.Value.Pattern);
                    return regex.IsMatch(uri.ToString());
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.Replacement))
                return new Uri(Regex.Replace(uri.ToString(), mapping.Value.Pattern, mapping.Value.Replacement));
            return uri;
        }

        public Dictionary<string, UriMap> OpenSearchTemplatesMap { get; internal set; }

        public string GetOpenSearchForUri(Uri uri)
        {
            var mapping = OpenSearchTemplatesMap.FirstOrDefault(kvp =>
            {
                try
                {
                    return Regex.IsMatch(uri.ToString(), kvp.Value.Pattern);
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.Replacement))
                return Regex.Replace(uri.ToString(), mapping.Value.Pattern, mapping.Value.Replacement);
            return uri.ToString();
        }

        public GeosquarePublicationModel CreatePublicationModel(GeosquarePublicationModel publishCatalogModel, ClaimsPrincipal user)
        {
            GeosquarePublicationModel geosquarePublicationModel = new GeosquarePublicationModel(publishCatalogModel);

            //if index not set in the body, we use the username as index
            if (string.IsNullOrEmpty(geosquarePublicationModel.Index))
            {
                if (user != null)
                    geosquarePublicationModel.Index = user.Identity.Name;
                else if (!string.IsNullOrEmpty(DefaultIndex))
                    geosquarePublicationModel.Index = DefaultIndex;
            }
            if (geosquarePublicationModel.AuthorizationHeaderValue == null)
            {
                if (!string.IsNullOrWhiteSpace(AuthorizationHeader))
                    geosquarePublicationModel.AuthorizationHeader = AuthorizationHeader;
                // TODO pass user identity auth
            }
            geosquarePublicationModel.Index = geosquarePublicationModel.Index.ToLower();

            geosquarePublicationModel.CreateIndex |= CreateIndex;

            return geosquarePublicationModel;
        }
    }

    public class UriMap
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }
    }
}