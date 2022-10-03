using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Xml;
using Stac;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    /// <summary>
    /// Publication Model
    /// </summary>
    [DataContract]
    public class GeosquarePublicationModel : IPublicationModel
    {
        private AuthenticationHeaderValue authorizationHeaderValue;

        public GeosquarePublicationModel()
        {
            SubjectsList = new List<Subject>();
        }

        public GeosquarePublicationModel(GeosquarePublicationModel publishCatalogModel)
        {
            Url = publishCatalogModel.Url;
            authorizationHeaderValue = publishCatalogModel.authorizationHeaderValue;
            Index = publishCatalogModel.Index;
            AdditionalLinks = publishCatalogModel.AdditionalLinks;
            SubjectsList = publishCatalogModel.Subjects.Select(s => new Subject(s)).ToList();
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
        public string AuthorizationHeader { get => authorizationHeaderValue?.ToString(); set => authorizationHeaderValue = value != null ? AuthenticationHeaderValue.Parse(value) : default(AuthenticationHeaderValue); }

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
        /// Subjects to be added to the catalog items
        /// </summary>
        [DataMember(Name = "subjects")]
        public List<Subject> SubjectsList { get; set; }

        [IgnoreDataMember]
        public List<ISubject> Subjects => this.SubjectsList.Cast<ISubject>().ToList();

        public AuthenticationHeaderValue AuthorizationHeaderValue { get => authorizationHeaderValue; set => authorizationHeaderValue = value; }

        public string ApiKey { get; set; }

        public void SetAuthorizationHeader(string Username, string Password)
        {
            var authenticationString = $"{Username}:{Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            authorizationHeaderValue = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

        }

        public Action<SyndicationLink, AtomItem, IItem> CustomLinkUpdater { get; set; }

        internal void UpdateLink(SyndicationLink link, AtomItem item, IItem itemNode)
        {
            if (CustomLinkUpdater != null)
                CustomLinkUpdater(link, item, itemNode);
        }

        public bool ThrowPublicationException { get; set; } = true;
    }

}