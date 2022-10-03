using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Stac;
using Terradue.Stars.Data.Model.Atom;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Common;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.ThirdParty.Titiler;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services;
using System.Threading;
using Terradue.Stars.Services.Model;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquareService : ICatalogService
    {
        private readonly RouterService routingService;
        private readonly TranslatorManager translatorManager;
        private readonly GeosquareConfiguration geosquareConfiguration;
        private readonly TitilerService titilerService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICredentials credentials;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ILogger<GeosquareService> logger;

        public GeosquareConfiguration GeosquareConfiguration => geosquareConfiguration;

        public GeosquareService(RouterService routerService,
                                  TranslatorManager translatorManager,
                                  IOptions<GeosquareConfiguration> geosquareConfiguration,
                                  TitilerService titilerService,
                                  IHttpClientFactory httpClientFactory,
                                  ICredentials credentials,
                                  IResourceServiceProvider resourceServiceProvider,
                                  ILogger<GeosquareService> logger)
        {
            this.routingService = routerService;
            this.translatorManager = translatorManager;
            this.geosquareConfiguration = geosquareConfiguration.Value;
            this.titilerService = titilerService;
            this.httpClientFactory = httpClientFactory;
            this.credentials = credentials;
            this.resourceServiceProvider = resourceServiceProvider;
            this.logger = logger;
        }

        public async Task<IPublicationState> PublishAsync(IPublicationModel publicationModel, CancellationToken ct)
        {
            GeosquarePublicationModel geosquareModel = publicationModel as GeosquarePublicationModel;
            if ( geosquareModel == null )
            {
                geosquareModel = CreateModelFromPublication(publicationModel);
            }
            if (geosquareModel.CreateIndex) await CreateIndexIfNotExist(geosquareModel.Index);
            InitRoutingTask();
            var guid = CalculateHash(geosquareModel.Url.ToString());
            var route = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(geosquareModel.Url)), ct);

            GeosquarePublicationState state = new GeosquarePublicationState(geosquareModel);
            state.Hash = guid;
            await routingService.RouteAsync(route, 4, null, state, ct);

            state.OsdUri = new Uri(GeosquareConfiguration.BaseUri,
                            string.Format("{0}/cat/{1}/description", geosquareModel.Index, guid.Value));

            return state;
        }

        private GeosquarePublicationModel CreateModelFromPublication(IPublicationModel publicationModel)
        {
            return new GeosquarePublicationModel
            {
                Url = publicationModel.Url,
                Index = geosquareConfiguration.DefaultIndex,
                AdditionalLinks = publicationModel.AdditionalLinks,
                CreateIndex = true,
                SubjectsList = publicationModel.Subjects?.Select(s => new Subject(s)).ToList(),
                ApiKey = geosquareConfiguration.ApiKey,
                AuthorizationHeader = geosquareConfiguration.AuthorizationHeader
            };
        }

        private KeyValuePair<string, string> CalculateHash(string input)
        {
            using (var algorithm = SHA256.Create()) //or MD5 SHA256 etc.
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                string hash = BitConverter.ToString(hashedBytes).ToLower();
                return new KeyValuePair<string, string>(string.Join("", hash.Split('-').Take(4)), hash.Replace("-", ""));
            }
        }

        private void InitRoutingTask()
        {
            routingService.Parameters = new RouterServiceParameters()
            {
                Recursivity = 4,
                SkipAssets = true
            };
            // routingService.OnRoutingException((route, router, exception, state) => PrintRouteInfo(route, router, exception, state));
            // routingService.OnBeforeBranching((node, router, state) => PrintBranchingNode(node, router, state));
            routingService.OnItem((node, router, state, ct) => PostItemToCatalog(node, router, state, ct));
            // routingService.OnBranching((parentRoute, route, siblings, state) => PrepareNewRoute(parentRoute, route, siblings, state));
        }

        public async Task<object> PostItemToCatalog(IItem itemNode, IRouter router, object state, CancellationToken ct)
        {
            GeosquarePublicationState catalogPublicationState = state as GeosquarePublicationState;
            AtomItemNode atomItemNode = null;
            try
            {
                atomItemNode = await translatorManager.TranslateAsync<AtomItemNode>(itemNode, ct);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Unable to translate item {0} to AtomItemNode", itemNode);
                if ( catalogPublicationState.GeosquarePublicationModel.ThrowPublicationException )
                    throw;
                
                atomItemNode = null;
            }
            if (atomItemNode == null) return state;

            atomItemNode.AtomItem.Identifier = catalogPublicationState.Hash.Key + "-" + atomItemNode.Identifier;
            atomItemNode.AtomItem.Categories.Add(new SyndicationCategory(catalogPublicationState.Hash.Value, "http://www.terradue.com/opensearch/hash", catalogPublicationState.Hash.Value));

            await PrepareAtomItem(atomItemNode.AtomItem, catalogPublicationState, itemNode);

            await PublishAtomFeed(atomItemNode.AtomItem.ToAtomFeed(), catalogPublicationState.GeosquarePublicationModel);

            return state;
        }

        public async Task PrepareAtomItem(AtomItem atomItem, GeosquarePublicationState geosquarePublicationState, IItem itemNode)
        {
            // remap all link
            foreach (var link in atomItem.Links)
            {
                link.Uri = geosquareConfiguration.MapUri(link.Uri);
                geosquarePublicationState.GeosquarePublicationModel.UpdateLink(link, atomItem, itemNode);
            }

            // create eventual opensearch link
            if (atomItem is StarsAtomItem)
            {
                await (atomItem as StarsAtomItem).CreateOpenSearchLinks(this.CreateOpenSearchLinkAsync, geosquarePublicationState);
            }

            //add links
            if (geosquarePublicationState.GeosquarePublicationModel != null && geosquarePublicationState.GeosquarePublicationModel.AdditionalLinks != null)
            {
                foreach (var link in geosquarePublicationState.GeosquarePublicationModel.AdditionalLinks)
                    atomItem.Links.Add(link.ToSyndicationLink());
            }

            //add categories
            if (geosquarePublicationState.GeosquarePublicationModel != null && geosquarePublicationState.GeosquarePublicationModel.SubjectsList != null)
            {
                foreach (var subject in geosquarePublicationState.GeosquarePublicationModel.SubjectsList)
                    atomItem.Categories.Add(subject.ToSyndicationCategory());
            }
        }

        public async Task CreateIndexIfNotExist(string index)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            var wr = await httpClient.GetAsync(new Uri(GeosquareConfiguration.BaseUri, index + "/_exists"));
            if (wr.StatusCode == HttpStatusCode.OK) return;

            logger.LogDebug("Creating index {0}", index);
            if ( !string.IsNullOrEmpty(GeosquareConfiguration.AuthorizationHeader))
                httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(GeosquareConfiguration.AuthorizationHeader);
            var webresponse = await httpClient.PutAsync(new Uri(GeosquareConfiguration.BaseUri, index), null);
            string response = await webresponse.Content.ReadAsStringAsync();
            logger.LogDebug(response);

        }

        public async Task<string> PublishAtomFeed(AtomFeed atomFeed, GeosquarePublicationModel pubModel)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            var content = GetAtomContent(atomFeed);
            httpClient.DefaultRequestHeaders.Authorization = pubModel.AuthorizationHeaderValue;
            UriBuilder postUri = new UriBuilder(new Uri(GeosquareConfiguration.BaseUri, pubModel.Index));
            if ( !string.IsNullOrEmpty(pubModel.ApiKey) ){
                postUri.Query = string.Format("apikey={0}", pubModel.ApiKey);
            }
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/atom+xml");
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var webresponse = await httpClient.PostAsync(postUri.Uri, content);
            string response = await webresponse.Content.ReadAsStringAsync();
            logger.LogDebug(response);

            return response;
        }

        public StacLink CreateOpenSearchDescriptionStacLinkFromCategoryName(string catName, string index)
        {
            return new StacLink(new Uri(GeosquareConfiguration.BaseUri,
                                        string.Format("{0}/cat/{1}/description", index, catName)),
                "search", "Opensearch description", "application/opensearchdescription+xml");
        }

        protected virtual StringContent GetAtomContent(AtomFeed atomFeed)
        {
            var stringContent = atomFeed.SerializeToString().Replace("utf-8", "utf-32");
            return new StringContent(stringContent, Encoding.UTF32);
        }

        public async Task<SyndicationLink> CreateOpenSearchLinkAsync(SyndicationLink link, object state)
        {
            GeosquarePublicationState catalogPublicationState = state as GeosquarePublicationState;
            try
            {
                var template = geosquareConfiguration.GetOpenSearchForUri(link.Uri);
                if (string.IsNullOrEmpty(template)) return null;
                var webRoute = await resourceServiceProvider.CreateStreamResourceAsync(new AtomResourceLink(link), System.Threading.CancellationToken.None);
                IStacObject linkedStacObject = StacConvert.Deserialize<IStacObject>(await webRoute.GetStreamAsync(System.Threading.CancellationToken.None));
                var osUrl = template.ReplaceMacro<IStacObject>("stacObject", linkedStacObject);
                osUrl = osUrl.ReplaceMacro<string>("index", catalogPublicationState.GeosquarePublicationModel.Index);
                var osUri = new Uri(osUrl);

                var relatedLink = new SyndicationLink(
                    osUri,
                    "results",
                    "Search for " + link.Title,
                    "application/atom+xml", 0
                );

            }
            catch { }
            return null;
        }
    }
}