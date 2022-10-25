using System;
using System.Collections.Generic;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquarePublicationState : IPublicationState
    {

        public GeosquarePublicationState(GeosquarePublicationModel model)
        {
            this.GeosquarePublicationModel = model;
        }
        public KeyValuePair<string, string> Hash { get; internal set; }
        public GeosquarePublicationModel GeosquarePublicationModel { get; internal set; }
        public Uri OsdUri { get; internal set; }

        public StacLink GetPublicationLink()
        {
            return new StacLink(OsdUri, "search", "OpenSearch Description", "application/xml+opensearchdescription", 0);
        }
    }
}