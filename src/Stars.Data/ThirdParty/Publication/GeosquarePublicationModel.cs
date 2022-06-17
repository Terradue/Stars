using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Xml;
using Stac;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    /// <summary>
    /// Publication Model
    /// </summary>
    [DataContract]
    public class GeosquarePublicationModel : IPublicationModel
    {
        private AuthenticationHeaderValue authorizationHeaderValue;

        public GeosquarePublicationModel(){
            
        }

        public GeosquarePublicationModel(GeosquarePublicationModel publishCatalogModel)
        {
            Url = publishCatalogModel.Url;
            authorizationHeaderValue = publishCatalogModel.authorizationHeaderValue;
            Index = publishCatalogModel.Index;
            AdditionalLinks = publishCatalogModel.AdditionalLinks;
            Categories = publishCatalogModel.Categories;
            CreateIndex = publishCatalogModel.CreateIndex;
        }

        /// <summary>
        /// Url to the catalog (STAC or Atom) to publish
        /// </summary>
        /// <value></value>
        [Required]
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Authorization Header used to publish on the catalog
        /// </summary>
        [DataMember]
        public string AuthorizationHeader { get => authorizationHeaderValue?.ToString(); set => authorizationHeaderValue = AuthenticationHeaderValue.Parse(value); }

        /// <summary>
        /// Index used to publish on the catalog
        /// </summary>
        [DataMember]
        public string Index { get; set; }

        /// <summary>
        /// Indicates if catalog index should be created if not exists
        /// </summary>
        [DataMember]
        public bool CreateIndex { get; set; }

        /// <summary>
        /// Collection used to publish on the catalog
        /// </summary>
        [DataMember]
        public string Collection { get; set; }

        /// <summary>
        /// Links to be added to the catalog items
        /// </summary>
        [DataMember]
        public List<StacLink> AdditionalLinks { get; set; }

        /// <summary>
        /// Categories to be added to the catalog items
        /// </summary>
        [DataMember]
        public List<SyndicationCategoryModel> Categories { get; set; }
        
        public AuthenticationHeaderValue AuthorizationHeaderValue { get => authorizationHeaderValue; set => authorizationHeaderValue = value; }

        public void SetAuthorizationHeader(string Username, string Password)
        {
            var authenticationString = $"{Username}:{Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            authorizationHeaderValue = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

        }
    }

    public class SyndicationCategoryModel
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Scheme { get; set; }
        public SyndicationCategory ToSyndicationCategory()
        {
            var category = new SyndicationCategory(Name, Scheme, Label);
            return category;
        }
    }
   
}