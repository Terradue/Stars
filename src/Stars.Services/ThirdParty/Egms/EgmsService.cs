using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Stac;
using System.Linq;
using Stac.Extensions.Projection;
using Stac.Extensions.Eo;
using Stac.Extensions.Raster;
using Terradue.Stars.Interface.Extensions.TimeSeries;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    public class EgmsService : IThirdPartyService, ITimeSeriesService
    {
        private readonly IOptions<EgmsConfiguration> options;
        private readonly ILogger<EgmsService> logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EgmsService(IOptions<EgmsConfiguration> options,
                                   ILogger<EgmsService> logger,
                                   IHttpClientFactory httpClientFactory)
        {
            this.options = options;
            this.logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Uri Uri => Configuration?.BaseUri;

        public EgmsConfiguration Configuration => options.Value;

        public bool IsAvailable => Configuration != null;

        public async Task<StacLink> CreateTimeSeriesAsync(IAbstractTimeSeriesCreationRequest ingestionRequest, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = Configuration.BaseUri;

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/ns/${ingestionRequest.NamespaceId}/import");
            EgmsTimeSeriesImportRequest postRequest = new EgmsTimeSeriesImportRequest()
            {
                Collection = ingestionRequest.Collection,
                Url = ingestionRequest.Source,
                TimeSeriesId = ingestionRequest.TimeSeriesId,
                Format = ingestionRequest.Properties["format"]
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(postRequest));

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return new StacLink(response.Headers.Location, "tsapi", "EGMS TimeSeries", "application/json");
        }

        public Uri GetEgmsApiLink(StacItem stacItem)
        {
            var link = stacItem.Links.Where(a => a.RelationshipType == "tsapi").First();
            if(link != null) return link.Uri;
            return null;
        }

    }
}