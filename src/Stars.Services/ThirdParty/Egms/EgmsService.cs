using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Stac;
using System.Linq;
using Stac.Extensions.Projection;
using Stac.Extensions.Eo;
using Stac.Extensions.Raster;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    public class EgmsService : IThirdPartyService
    {
        private readonly IOptions<EgmsConfiguration> options;
        private readonly ILogger<EgmsService> logger;

        public EgmsService(IOptions<EgmsConfiguration> options,
                                   ILogger<EgmsService> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public Uri Uri => Configuration?.BaseUri;

        public EgmsConfiguration Configuration => options.Value;

        public bool IsAvailable => Configuration != null;

        public Uri GetCoverageLink(StacItem stacItem)
        {
            var link = stacItem.Links.Where(a => a.RelationshipType == "coverage").First();
            if(link != null) return link.Uri;
            return null;
        }
    }
}