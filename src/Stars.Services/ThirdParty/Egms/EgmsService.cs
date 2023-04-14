using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stac;
using System.Linq;
using Terradue.Stars.Interface.Extensions.TimeSeries;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using Terradue.Stars.Interface;
using Stac.Common;
using System.IO;
using System.Collections.Generic;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    public class EgmsService : IThirdPartyService, ITimeSeriesService
    {
        private readonly IOptions<EgmsConfiguration> options;
        private readonly ILogger<EgmsService> logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IResourceServiceProvider _resourceServiceProvider;

        public EgmsService(IOptions<EgmsConfiguration> options,
                                   ILogger<EgmsService> logger,
                                   IHttpClientFactory httpClientFactory,
                                   IResourceServiceProvider resourceServiceProvider)
        {
            this.options = options;
            this.logger = logger;
            _httpClientFactory = httpClientFactory;
            _resourceServiceProvider = resourceServiceProvider;
        }

        public Uri Uri => Configuration?.BaseUri;

        public EgmsConfiguration Configuration => options.Value;

        public bool IsAvailable => Configuration != null;

        public async Task<StacCollection> CreateNewTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> ingestionRequest, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("egms");
            client.BaseAddress = Configuration.BaseUri;

            StacCollection collection = await ingestionRequest.GetOperationPayloadAsync(cancellationToken);

            var request = new HttpRequestMessage(HttpMethod.Post, $"ns/{ingestionRequest.NamespaceId}/cs");
            logger.LogDebug(JsonConvert.SerializeObject(collection));
            request.Content = new StringContent(JsonConvert.SerializeObject(collection), System.Text.Encoding.UTF8, collection.MediaType.ToString());

            var response = await client.SendAsync(request, cancellationToken);
            try {
                response.EnsureSuccessStatusCode();
            } catch (Exception e) {
                logger.LogError(e, "Error while creating time series collection");
                logger.LogDebug(response.Content.ReadAsStringAsync().Result);
                throw;
            }

            return await GetCollectionFromResponseAsync(response);
        }

        private async Task<StacCollection> GetCollectionFromResponseAsync(HttpResponseMessage response)
        {
            return StacConvert.Deserialize<StacCollection>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StacCollection> UpdateTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> updateRequest, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = Configuration.BaseUri;
            StacCollection collection = await updateRequest.GetOperationPayloadAsync(cancellationToken);

            var request = new HttpRequestMessage(HttpMethod.Put, $"ns/${updateRequest.NamespaceId}/cs/{collection.Id}");
            request.Content = new StringContent(JsonConvert.SerializeObject(collection));

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            collection = await GetCollectionFromResponseAsync(response);

            return collection;
        }

        public async Task<StacCollection> PatchTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<Patch> patchRequest, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = Configuration.BaseUri;
            Patch patch = await patchRequest.GetOperationPayloadAsync(cancellationToken);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"ns/${patchRequest.NamespaceId}/cs/{patchRequest.CollectionId}");
            request.Content = new StringContent(JsonConvert.SerializeObject(patch));

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            StacCollection collection = await GetCollectionFromResponseAsync(response);

            return collection;
        }

        public async Task DeleteTimeSeriesCollectionAsync(IAbstractTimeSeriesOperationRequest<StacCollection> deleteRequest, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = Configuration.BaseUri;

            var request = new HttpRequestMessage(HttpMethod.Delete, $"ns/${deleteRequest.NamespaceId}/cs/{deleteRequest.CollectionId}");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

        }

        public Uri GetEgmsApiLink(StacCollection stacCollection)
        {
            var link = stacCollection.Links.Where(a => a.RelationshipType == "tsapi").FirstOrDefault();
            if (link != null) return link.Uri;
            return null;
        }

        public async Task<IAbstractTimeSeriesOperationStatus<T>> GetTimeSeriesOperationStatusAsync<T>(StacCollection collection, CancellationToken cancellationToken)
        {
            // Get all time series links
            var itemLinks = collection.GetItemLinks();
            if (itemLinks.Count() == 0)
                throw new Exception("No timeseries link found in collection");
            var client = _httpClientFactory.CreateClient("egms");
            List<EgmsTimeSeriesImportTask> statuses = new List<EgmsTimeSeriesImportTask>();

            foreach (var itemLink in itemLinks)
            {
                var json = await client.GetStringAsync(itemLink.Uri);
                StacItem stacItem = StacConvert.Deserialize<StacItem>(json);
                var importJobId = stacItem.GetProperty<string>("ts:import_job_id");
                Uri importJobStatus = new Uri($"{itemLink.Uri}/import/jobs/{importJobId}/status");
                var statusJson = await client.GetStringAsync(importJobStatus);
                statuses.Add(JsonConvert.DeserializeObject<EgmsTimeSeriesImportTask>(statusJson));
            }

            return new EgmsTimeSeriesOperationStatus(collection, statuses) as IAbstractTimeSeriesOperationStatus<T>;

        }
    }
}