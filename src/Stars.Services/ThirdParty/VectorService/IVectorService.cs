using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Stac;
using System.Linq;
using Stac.Extensions.Projection;
using Stac.Extensions.Eo;
using Stac.Extensions.Raster;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public interface IVectorService : IThirdPartyService
    {
        Uri BuildServiceUri(StacItemNode stacItemNode, KeyValuePair<string, StacAsset> vectorAsset);

        IDictionary<string, StacAsset> SelectVectorAssets(StacItem stacItem);
    }
}