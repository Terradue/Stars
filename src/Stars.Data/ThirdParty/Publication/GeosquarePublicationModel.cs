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

        public GeosquarePublicationModel(IPublicationModel pubModel)
        {
            Url = pubModel.Url;
            AdditionalLinks = pubModel.AdditionalLinks;
            SubjectsList = new List<Subject>();
            if (pubModel.Subjects != null)
            {
                SubjectsList.AddRange(pubModel.Subjects.Select(s => new Subject(s)));
            }
            Collection = pubModel.Collection;
            CatalogId = pubModel.CatalogId;
            Depth = pubModel.Depth;
        }

        public GeosquarePublicationModel(GeosquarePublicationModel publishCatalogModel)
        {
            Url = publishCatalogModel.Url;
            Index = publishCatalogModel.Index;
            AdditionalLinks = publishCatalogModel.AdditionalLinks;
            SubjectsList = publishCatalogModel.Subjects.Select(s => new Subject(s)).ToList();
            CreateIndex = publishCatalogModel.CreateIndex;
            Collection = publishCatalogModel.Collection;
            CustomLinkUpdater = publishCatalogModel.CustomLinkUpdater;
            CatalogId = publishCatalogModel.CatalogId;
            Depth = publishCatalogModel.Depth;
        }

        /// <summary>
        /// Url to the catalog (STAC or Atom) to publish
        /// </summary>
        /// <value></value>
        [Required]
        [DataMember]
        public string Url { get; set; }

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
        /// Recursivity Depth of the catalog to publish
        /// </summary>
        [DataMember]
        public int Depth { get; set; } = 4;

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

        public Action<SyndicationLink, AtomItem, IAssetsContainer> CustomLinkUpdater { get; set; }

        internal void UpdateLink(SyndicationLink link, AtomItem item, IAssetsContainer assetsContainer)
        {
            if (CustomLinkUpdater != null)
                CustomLinkUpdater(link, item, assetsContainer);
        }

        public bool ThrowPublicationException { get; set; } = true;

        public string CatalogId { get; set; }
    }

}