using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Xml;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    /// <summary>
    /// Publication Model
    /// </summary>
    [DataContract]
    public class GeosquarePublicationModel
    {
        private AuthenticationHeaderValue authorizationHeaderValue;

        public GeosquarePublicationModel(){
            
        }

        public GeosquarePublicationModel(GeosquarePublicationModel publishCatalogModel)
        {
            Url = publishCatalogModel.Url;
            authorizationHeaderValue = publishCatalogModel.authorizationHeaderValue;
            Index = publishCatalogModel.Index;
            Links = publishCatalogModel.Links;
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
        /// Links to be added to the catalog items
        /// </summary>
        [DataMember]
        public List<SyndicationLinkModel> Links { get; set; }

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

    public class SyndicationLinkModel
    {
        public string Title { get; set; }
        public string Rel { get; set; }
        public string Href { get; set; }
        public string Type { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }

        public SyndicationLink ToSyndicationLink()
        {
            var link = new SyndicationLink(new System.Uri(Href), Rel, Title, Type, 0);
            if (Attributes != null)
            {
                foreach (var attr in Attributes) link.AttributeExtensions.Add(new XmlQualifiedName(attr.Key), attr.Value);
            }
            return link;
        }
    }
}