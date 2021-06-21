using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacCollectionNode : StacCatalogNode, ICatalog
    {
        public StacCollectionNode(StacCollection stacCollection, Uri uri, ICredentials credentials = null) : base(stacCollection, uri, credentials)
        {
        }

        public StacCollection StacCollection => stacObject as StacCollection;

        public override ResourceType ResourceType => ResourceType.Collection;

        public static StacCollectionNode CreateUnlocatedNode(StacCollection collection)
        {
            return new StacCollectionNode(collection, new Uri(collection.Id + ".json", UriKind.Relative));
        }

        public override object Clone()
        {
            return new StacCollectionNode(new StacCollection(this.StacCollection), new Uri(this.Uri.ToString()), credentials);
        }
    }
}