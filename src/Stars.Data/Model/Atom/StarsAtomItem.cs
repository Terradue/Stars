using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using Humanizer;
using Markdig;
using Stac;
using Stac.Extensions.Projection;
using Terradue.Stars.Data.ThirdParty.Geosquare;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Ogc.Eop21;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Geometry.Wkt;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.ThirdParty.Titiler;
using Stac.Extensions.File;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.ThirdParty.Egms;
using Terradue.Stars.Interface;
using Stac.Extensions.Eo;

namespace Terradue.Stars.Data.Model.Atom
{

    public class StarsAtomItem : AtomItem
    {

        public StarsAtomItem(AtomItem item) : base(item)
        {
        }

        public static StarsAtomItem Create(StacItem stacItem, Uri stacItemUri)
        {
            AtomItem item = new AtomItem(stacItem.Title,
                                 stacItem.Description,
                                 null,
                                 stacItem.Id,
                                 new DateTimeOffset(stacItem.DateTime.Start.ToUniversalTime(), new TimeSpan(0))
                                );

            StarsAtomItem starsAtomItem = new StarsAtomItem(item);
            if (!string.IsNullOrEmpty(stacItem.Description))
                starsAtomItem.Summary = new TextSyndicationContent(Markdown.ToHtml(stacItem.Description, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()), TextSyndicationContentKind.Html);
            starsAtomItem.Title = new TextSyndicationContent(stacItem.Title, TextSyndicationContentKind.Plaintext);
            starsAtomItem.Identifier = stacItem.Id;

            // Links are tricky because they can be relative and we must render them absolute in Atom
            foreach (var link in stacItem.Links)
            {
                if (link.RelationshipType == "self")
                    continue;
                Uri linkUri = link.Uri;
                try
                {
                    // if relative
                    if (!linkUri.IsAbsoluteUri)
                        // then make absolute from item
                        linkUri = new Uri(stacItemUri, linkUri);
                    // then make sure this is the from uri
                    linkUri = linkUri;
                }
                catch { continue; }

                var rel = link.RelationshipType;
                var mediatype = link.ContentType ?? new ContentType(MimeTypes.GetMimeType(link.Uri.ToString()));

                starsAtomItem.Links.Add(new SyndicationLink(linkUri,
                                                                  rel,
                                                                  link.Title,
                                                                  mediatype.ToString(),
                                                                  Convert.ToInt64(link.Length)));
            }

            // Add Alternate
            starsAtomItem.Links.Add(new SyndicationLink(stacItemUri,
                                                              "alternate",
                                                              "Stac Item",
                                                              stacItem.MediaType.ToString(),
                                                              0));

            // Add Assets
            // Same problem as per link, make uri absolute
            foreach (var asset in stacItem.Assets)
            {
                Uri assetUri = GetAssetUri(stacItemUri, asset.Value);
                var atitle = string.IsNullOrEmpty(asset.Value.Title) ? asset.Key.Split('!')[0] : asset.Value.Title;
                var type = asset.Value.MediaType.ToString();
                long length = Convert.ToInt64(asset.Value.FileExtension().Size);
                starsAtomItem.Links.Add(new SyndicationLink(assetUri, "enclosure", atitle, type, length));
            }

            // Add functional links
            starsAtomItem.Links.AddRange(GetFunctionalLinks(stacItem.Assets, stacItemUri));

            var datestr = GetDcDateFormat(stacItem);
            if (datestr != null)
            {
                starsAtomItem.ElementExtensions.Add("date", "http://purl.org/dc/elements/1.1/", datestr);
            }
            var geom = stacItem.Geometry;
            if (geom != null)
            {
                starsAtomItem.ElementExtensions.Add("spatial", "http://purl.org/dc/terms/", geom.ToWkt());
            }

            starsAtomItem.LastUpdatedTime = new DateTimeOffset(stacItem.Updated, new TimeSpan(0));
            starsAtomItem.PublishDate = new DateTimeOffset(stacItem.Created, new TimeSpan(0));

            return starsAtomItem;
        }

        public static StarsAtomItem Create(StacCollection stacCollection, Uri stacCollectionUri)
        {
            AtomItem item = new AtomItem(stacCollection.Title,
                                 stacCollection.Description,
                                 null,
                                 stacCollection.Id,
                                 new DateTimeOffset(stacCollection.Extent.Temporal.Interval[0][0].Value.ToUniversalTime(), new TimeSpan(0))
                                );

            StarsAtomItem starsAtomItem = new StarsAtomItem(item);
            if (!string.IsNullOrEmpty(stacCollection.Description))
                starsAtomItem.Summary = new TextSyndicationContent(Markdown.ToHtml(stacCollection.Description, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()), TextSyndicationContentKind.Html);
            starsAtomItem.Title = new TextSyndicationContent(stacCollection.Title, TextSyndicationContentKind.Plaintext);
            starsAtomItem.Identifier = stacCollection.Id;

            // Links are tricky because they can be relative and we must render them absolute in Atom
            foreach (var link in stacCollection.Links)
            {
                if (link.RelationshipType == "self")
                    continue;
                Uri linkUri = link.Uri;
                try
                {
                    // if relative
                    if (!linkUri.IsAbsoluteUri)
                        // then make absolute from item
                        linkUri = new Uri(stacCollectionUri, linkUri);
                    // then make sure this is the from uri
                    linkUri = linkUri;
                }
                catch { continue; }

                var rel = link.RelationshipType;
                var mediatype = link.ContentType ?? new ContentType(MimeTypes.GetMimeType(link.Uri.ToString()));

                starsAtomItem.Links.Add(new SyndicationLink(linkUri,
                                                                  rel,
                                                                  link.Title,
                                                                  mediatype.ToString(),
                                                                  Convert.ToInt64(link.Length)));
            }

            // Add Alternate
            starsAtomItem.Links.Add(new SyndicationLink(stacCollectionUri,
                                                              "alternate",
                                                              "Stac Collection",
                                                              stacCollection.MediaType.ToString(),
                                                              0));

            // Add Assets
            // Same problem as per link, make uri absolute
            foreach (var asset in stacCollection.Assets)
            {
                Uri assetUri = GetAssetUri(stacCollectionUri, asset.Value);
                var atitle = string.IsNullOrEmpty(asset.Value.Title) ? asset.Key.Split('!')[0] : asset.Value.Title;
                var type = asset.Value.MediaType.ToString();
                long length = Convert.ToInt64(asset.Value.FileExtension().Size);
                starsAtomItem.Links.Add(new SyndicationLink(assetUri, "enclosure", atitle, type, length));
            }

            // Add functional links
            starsAtomItem.Links.AddRange(GetFunctionalLinks(stacCollection.Assets, stacCollectionUri));

            var datestr = GetDcDateFormat(stacCollection);
            if (datestr != null)
            {
                starsAtomItem.ElementExtensions.Add("date", "http://purl.org/dc/elements/1.1/", datestr);
            }
            var geom = stacCollection.Extent.Spatial;
            if (geom != null)
            {
                starsAtomItem.ElementExtensions.Add("spatial", "http://purl.org/dc/terms/", geom.ToWkt());
            }

            starsAtomItem.LastUpdatedTime = new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0));

            return starsAtomItem;
        }

        private static object GetDcDateFormat(StacItem stacItem)
        {
            string datestr = stacItem.DateTime.Start.ToString("O");
            if (stacItem.DateTime.End != null && stacItem.DateTime.End != DateTime.MaxValue && stacItem.DateTime.End != stacItem.DateTime.Start)
                datestr += "/" + stacItem.DateTime.End.ToString("O");
            return datestr;
        }

        private static object GetDcDateFormat(StacCollection stacCollection)
        {
            string datestr = stacCollection.Extent.Temporal.Interval.Min(d => d[0] ?? DateTime.MinValue).ToString("O");
            datestr += "/" + stacCollection.Extent.Temporal.Interval.Max(d => d[1] ?? DateTime.MaxValue).ToString("O");
            return datestr;
        }

        private static IEnumerable<SyndicationLink> GetFunctionalLinks(IDictionary<string, StacAsset> assets, Uri stacObjectUri)
        {
            List<SyndicationLink> links = new List<SyndicationLink>();

            var overviews = assets.Where(a => a.Value.Roles.Contains("overview") || a.Value.Roles.Contains("thumbnail") || a.Value.Roles.Contains("legend"));
            links.AddRange(overviews.Select(o => new SyndicationLink(GetAssetUri(stacObjectUri, o.Value),
                                                                     GetRelationshipFromRoles(o.Value.Roles),
                                                                     GetTitleFromRoles(o),
                                                                     o.Value.MediaType.ToString(),
                                                                     Convert.ToInt64(o.Value.FileExtension().Size))));

            return links;
        }

        private static Uri GetAssetUri(Uri stacItemUri, StacAsset asset)
        {
            Uri assetUri = asset.Uri;
            try
            {
                // if relative
                if (!assetUri.IsAbsoluteUri)
                    // then make absolute from item
                    return new Uri(stacItemUri, asset.Uri);
            }
            catch { }
            return asset.Uri;
        }

        private static string GetRelationshipFromRoles(ICollection<string> semanticRoles)
        {
            if (semanticRoles.Any(r => r == "legend"))
                return "legend";
            if (semanticRoles.Any(r => r == "thumbnail" || r == "icon" || r == "overview"))
                return "icon";
            return "enclosure";
        }

        private static string GetTitleFromRoles(KeyValuePair<string, StacAsset> stacAsset)
        {
            if (stacAsset.Value.Roles.Any(r => r == "thumbnail"))
                return "Thumbnail";
            if (stacAsset.Value.Roles.Any(r => r == "legend"))
            {
                return stacAsset.Key;
            }
            if (stacAsset.Value.Roles.Any(r => r == "overview"))
                return "Browse";
            return string.IsNullOrEmpty(stacAsset.Value.Title) ? stacAsset.Key.Split('!')[0] : stacAsset.Value.Title;
        }

        public bool AddFilterCategory(StacItem stacItem, string propertyKey)
        {
            string propertyValue = stacItem.GetProperty<string>(propertyKey);
            if (!string.IsNullOrEmpty(propertyValue))
            {
                Categories.Add(new SyndicationCategory(propertyKey.Underscore().ToLower().Replace(":", "_") + "_" + propertyValue.Underscore().ToLower(),
                                                       "https://www.terradue.com",
                                                       propertyKey.Replace(":", "_").Titleize() + ":" + propertyValue.Titleize()));
                return true;
            }
            return false;
        }

        public bool TryAddEarthObservationProfile(StacItem stacItem)
        {
            EarthObservationType eop = CreateEOProfileFromStacItem(stacItem);
            if (eop != null)
            {
                EarthObservationExtension = new SyndicationElementExtension(Terradue.ServiceModel.Ogc.OgcHelpers.CreateReader(eop));
                return true;
            }
            return false;
        }

        public bool TryAddTitilerOffering(StacItemNode stacItemNode, TitilerService titilerService)
        {
            var overviewAssets = titilerService.SelectOverviewCombinationAssets(stacItemNode.StacItem);
            if (overviewAssets.Count() > 0)
            {
                var stacItemUri = stacItemNode.Uri;
                Uri titilerServiceUri = titilerService.BuildServiceUri(stacItemUri, overviewAssets);
                ElementExtensions.Add(new OwcOffering()
                {
                    Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = titilerServiceUri.ToString(),
                            Type = "image/png",
                            Code = "GetMap",
                            Method = "GET"
                        }},
                    Code = "http://www.terradue.com/twm"
                }.CreateReader());
                return true;
            }
            return false;
        }

        public bool TryAddTitilerOffering(StacCollectionNode stacCollectionNode, TitilerService titilerService)
        {
            var overviewAssets = titilerService.SelectOverviewCombinationAssets(stacCollectionNode.StacCollection);
            if (overviewAssets.Count() > 0)
            {
                var stacItemUri = stacCollectionNode.Uri;
                Uri titilerServiceUri = titilerService.BuildServiceUri(stacItemUri, overviewAssets);
                ElementExtensions.Add(new OwcOffering()
                {
                    Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = titilerServiceUri.ToString(),
                            Type = "image/png",
                            Code = "GetMap",
                            Method = "GET"
                        }},
                    Code = "http://www.terradue.com/twm"
                }.CreateReader());
                return true;
            }
            return false;
        }

        public bool AddImageOverlayOffering(StacItemNode stacItemNode)
        {
            var imageOverview = SelectOverlayOverviewAssets(stacItemNode.StacItem).FirstOrDefault();
            if (imageOverview.Key != null)
            {
                Uri assetUri = imageOverview.Value.Uri;
                try
                {
                    // if relative
                    if (!assetUri.IsAbsoluteUri)
                        // then make absolute from item
                        assetUri = new Uri(stacItemNode.Uri, assetUri);
                }
                catch { return false; }
                ElementExtensions.Add(new OwcOffering()
                {

                    Contents = new OwcContent[]{new OwcContent()
                        {
                        Url = assetUri,
                        Type = imageOverview.Value.MediaType.ToString()
                        }},
                    Code = "http://www.opengis.net/spec/owc-atom/1.0/req/img"
                }.CreateReader());
                return true;
            }
            return false;
        }

        public bool TryAddEGMSOffering(StacCollectionNode stacCollectionNode, EgmsService egmsService)
        {
            var egmsServiceUri = stacCollectionNode.StacCollection.Links.FirstOrDefault(l => l.RelationshipType == "self").Uri;
            if (egmsServiceUri != null)
            {
                ElementExtensions.Add(new OwcOffering()
                {
                    Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = egmsServiceUri.ToString(),
                            Type = "application/json",
                            Code = "egms",
                            Method = "GET"
                        }},
                    Code = "http://www.terradue.com/egms"
                }.CreateReader());
                return true;
            }
            return false;
        }

        public void AddWMSOffering(StacLink link)
        {
            if (link != null)
            {
                // Find another offering first
                var offering = ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)))
                                                .FirstOrDefault(o => o.Code == "http://www.opengis.net/spec/owc-atom/1.0/req/wms");
                if (offering == null)
                {
                    ElementExtensions.Add(new OwcOffering()
                    {
                        Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = link.Uri.ToString(),
                            Type = link.ContentType.ToString(),
                            Code = "GetMap",
                            Method = "GET"
                        }},
                        Code = "http://www.opengis.net/spec/owc-atom/1.0/req/wms"
                    }.CreateReader());
                }
                else
                {
                    offering.Operations = offering.Operations.Concat(new OwcOperation[]{new OwcOperation()
                        {
                            Href = link.Uri.ToString(),
                            Type = link.ContentType.ToString(),
                            Code = "GetMap",
                            Method = "GET"
                        }}).ToArray();
                }
            }
        }

        public void AddTWMOffering(StacLink link)
        {
            if (link != null)
            {
                // Find another offering first
                var offering = ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)))
                                                .FirstOrDefault(o => o.Code == "http://www.terradue.com/twm");
                if (offering == null)
                {
                    ElementExtensions.Add(new OwcOffering()
                    {
                        Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = link.Uri.ToString(),
                            Type = link.ContentType.ToString(),
                            Code = "GetMap",
                            Method = "GET"
                        }},
                        Code = "http://www.terradue.com/twm"
                    }.CreateReader());
                }
                else
                {
                    offering.Operations = offering.Operations.Concat(new OwcOperation[]{new OwcOperation()
                        {
                            Href = link.Uri.ToString(),
                            Type = link.ContentType.ToString(),
                            Code = "GetMap",
                            Method = "GET"
                        }}).ToArray();
                }
            }
        }

        public bool TryAddVectorOffering(StacItemNode stacItemNode, IVectorService vectorService)
        {
            var vectorAssets = vectorService.SelectVectorAssets(stacItemNode.StacItem);
            foreach (var asset in vectorAssets)
            {
                var stacItemUri = stacItemNode.Uri;
                Uri titilerServiceUri = vectorService.BuildServiceUri(stacItemNode, asset);
                ElementExtensions.Add(new OwcOffering()
                {
                    Operations = new OwcOperation[]{new OwcOperation()
                        {
                            Href = titilerServiceUri.ToString(),
                            Type = asset.Value.MediaType.ToString(),
                            Code = "GetMap",
                            Method = "GET"
                        }},
                    Code = "http://www.terradue.com/fgb"
                }.CreateReader());
            }
            return vectorAssets.Count() > 0;
        }



        public async Task CreateOpenSearchLinks(Func<SyndicationLink, object, Task<SyndicationLink>> mapOpensearchUri, object state)
        {
            foreach (var link in Links.Where(l =>
            {
                ContentType contentType = new ContentType(l.MediaType);
                return contentType.MediaType == "application/geo+json";
            }).ToArray())
            {
                var osLink = await mapOpensearchUri(link, state);

                if (osLink != null && (link.RelationshipType == "related" || link.RelationshipType == "derived_from"))
                {
                    Links.Remove(link);

                    // show related button
                    osLink.AttributeExtensions.Add(new XmlQualifiedName("level"), "primary");
                    this.Links.Add(osLink);
                }
            }
        }

        private static Dictionary<string, StacAsset> SelectOverlayOverviewAssets(StacItem stacItem)
        {
            var ext = stacItem.ProjectionExtension();
            if (ext == null)
                return new Dictionary<string, StacAsset>();

            Dictionary<string, StacAsset> overviewAssets = stacItem.Assets
                                                            .Where(a => a.Value.Roles.Contains("overview") ||
                                                                        a.Value.Roles.Contains("visual"))
                                                            .OrderBy(a => a.Value.GetProperty<long>("size"))
                                                            .Where(a => a.Value.MediaType.MediaType.StartsWith("image/") &&
                                                                        a.Value.FileExtension().Size < 20971520)
                                                            .Take(1)
                                                            .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            return new Dictionary<string, StacAsset>();
        }

        SyndicationElementExtension earthObservationExtension = null;

        public SyndicationElementExtension EarthObservationExtension
        {
            get
            {
                return earthObservationExtension;
            }
            set
            {
                this.ElementExtensions.Remove(earthObservationExtension);
                earthObservationExtension = value;
                this.ElementExtensions.Add(earthObservationExtension);
            }
        }

        private static EarthObservationType CreateEOProfileFromStacItem(StacItem stacItem)
        {
            if (string.IsNullOrEmpty(stacItem.GetProperty<string>("platform"))) return null;
            EarthObservationType eop = new EarthObservationType();
            eop.EopMetaDataProperty = new EarthObservationMetaDataPropertyType();
            eop.EopMetaDataProperty.EarthObservationMetaData = new EarthObservationMetaDataType();
            eop.EopMetaDataProperty.EarthObservationMetaData.identifier = stacItem.Id;
            string[] instruments = stacItem.GetProperty<string[]>("instruments");
            string platform = stacItem.GetProperty<string>("platform");
            string[] instrument_types = stacItem.GetProperty<string[]>("instrument_types");
            if (instruments != null ||
                !string.IsNullOrEmpty(platform) ||
                instrument_types != null)
            {
                eop.procedure = new ServiceModel.Ogc.Om20.OM_ProcessPropertyType();
                eop.procedure.Eop21EarthObservationEquipment = new EarthObservationEquipmentType();
                eop.EopMetaDataProperty.EarthObservationMetaData.parentIdentifier = stacItem.Mission;
                if (!string.IsNullOrEmpty(platform))
                {
                    eop.procedure.Eop21EarthObservationEquipment.platform = new PlatformPropertyType();
                    eop.procedure.Eop21EarthObservationEquipment.platform.Platform = new PlatformType();
                    eop.procedure.Eop21EarthObservationEquipment.platform.Platform.shortName = platform;
                }

                if (instruments != null && instruments.Count() > 0)
                {
                    eop.procedure.Eop21EarthObservationEquipment.instrument = new InstrumentPropertyType();
                    eop.procedure.Eop21EarthObservationEquipment.instrument.Instrument = new InstrumentType();
                    eop.procedure.Eop21EarthObservationEquipment.instrument.Instrument.shortName = string.Join(",", instruments);
                }
                if (instrument_types != null && instrument_types.Count() > 0)
                {
                    eop.procedure.Eop21EarthObservationEquipment.sensor = new SensorPropertyType();
                    eop.procedure.Eop21EarthObservationEquipment.sensor.Sensor = new SensorType();
                    eop.procedure.Eop21EarthObservationEquipment.sensor.Sensor.sensorType = string.Join(",", instrument_types);
                    try
                    {
                        eop.procedure.Eop21EarthObservationEquipment.sensor.Sensor.resolution = new ServiceModel.Ogc.Gml321.MeasureType()
                        {
                            Value = stacItem.Gsd.Value
                        };
                    }
                    catch { }
                }
                // Cloud cover
                if ( stacItem.EoExtension().CloudCover != null)
                {
                    eop.result.Opt21EarthObservationResult.cloudCoverPercentage = new ServiceModel.Ogc.Gml321.MeasureType()
                    {
                        Value = stacItem.EoExtension().CloudCover.Value
                    };
                }
            }

            return eop;
        }

    }
}