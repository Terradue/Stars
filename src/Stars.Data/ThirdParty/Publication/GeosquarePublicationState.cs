using System;
using System.Collections.Generic;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquarePublicationState
    {

        public GeosquarePublicationState(GeosquarePublicationModel model)
        {
            this.GeosquarePublicationModel = model;
        }
        public KeyValuePair<string, string> Hash { get; internal set; }
        public GeosquarePublicationModel GeosquarePublicationModel { get; internal set; }
    }
}