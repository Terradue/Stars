using System;
using System.Collections.Generic;
using Stac;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public interface IVectorService : IThirdPartyService
    {
        Uri BuildServiceUri(StacItemNode stacItemNode, KeyValuePair<string, StacAsset> vectorAsset);

        IDictionary<string, StacAsset> SelectVectorAssets(StacItem stacItem);
    }
}
