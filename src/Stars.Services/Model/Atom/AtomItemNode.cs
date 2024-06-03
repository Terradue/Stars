// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AtomItemNode.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GeoJSON.Net.Geometry;
using Itenso.TimePeriod;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Atom
{
    public class AtomItemNode : IItem, IAssetsContainer, IStreamResource
    {
        private AtomItem item;
        private readonly Uri sourceUri;

        public AtomItemNode(AtomItem item, Uri sourceUri)
        {
            this.item = item;
            this.sourceUri = sourceUri;
        }

        public AtomItem AtomItem => item;

        public string Title => item.Title != null ? item.Title.Text : item.Id;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public Uri Uri => sourceUri ?? item.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ResourceType ResourceType => ResourceType.Item;

        public string Id => Identifier ?? item.Id.CleanIdentifier();

        public string Filename => Id + ".atom.xml";

        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(this.ReadAsStringAsync(CancellationToken.None).Result).Length);

        public bool IsCatalog => false;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };

        public IReadOnlyList<IResource> GetRoutes()
        {
            return new List<IResource>();
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return await Task<Stream>.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                var sw = XmlWriter.Create(ms);
                Atom10ItemFormatter atomFormatter = new Atom10ItemFormatter(item);
                atomFormatter.WriteTo(sw);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            }, ct);
        }



        public string Identifier
        {
            get
            {
                var identifier = item.ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? null : identifier[0];
            }
        }

        public IGeometryObject Geometry => item.FindGeometry();

        public IDictionary<string, object> Properties => item.GetCommonMetadata();

        public bool CanBeRanged => false;

        public IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                Dictionary<string, int> keysCount = item.Links.Where(link => !string.IsNullOrEmpty(link.RelationshipType)).GroupBy(l => l.RelationshipType).ToDictionary(g => g.Key, g => g.Count());
                Dictionary<string, int> keysIndex = item.Links.Where(link => !string.IsNullOrEmpty(link.RelationshipType)).GroupBy(l => l.RelationshipType).ToDictionary(g => g.Key, g => 1);
                foreach (var link in item.Links.Where(link => link.RelationshipType == "enclosure" || link.RelationshipType == "icon").OrderBy(i => i.RelationshipType))
                {
                    string key = link.RelationshipType;
                    if (keysCount[key] > 1)
                        key += "-" + keysIndex[key]++;
                    assets.Add(key, new AtomLinkAsset(link, AtomItem));
                }
                return assets;
            }
        }

        public ITimePeriod DateTime => new TimeInterval(item.PublishDate.DateTime);

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return item.Links.Where(link => link.RelationshipType != "enclosure" && link.RelationshipType != "icon").OrderBy(i => i.RelationshipType)
                             .Select(l => new AtomResourceLink(l))
                             .Concat(GenerateWebMapLinks())
                             .ToList();
        }

        private IEnumerable<AtomResourceLink> GenerateWebMapLinks()
        {
            List<AtomResourceLink> links = new List<AtomResourceLink>();
            var offerings = item.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));
            foreach (var offering in offerings)
            {
                // OGC WMS offering
                if (offering != null && offering.Code == "http://www.opengis.net/spec/owc-atom/1.0/req/wms")
                {
                    if (offering.Operations != null && offering.Operations.Count() > 0)
                    {
                        foreach (var operation in offering.Operations)
                        {
                            // WMS GetMap operation
                            if (operation != null && operation.Code == "GetMap")
                            {
                                links.Add(new AtomResourceLink(
                                    new SyndicationLink(new Uri(operation.Href), "wms", "WMS GetMap", operation.Type, 0)));
                            }
                        }
                    }
                }
                // Tile WebMap offering
                if (offering != null && offering.Code == "http://www.terradue.com/twm")
                {
                    if (offering.Operations != null && offering.Operations.Count() > 0)
                    {
                        foreach (var operation in offering.Operations)
                        {
                            // Tile GetMap operation
                            if (operation != null && operation.Code == "GetMap")
                            {
                                links.Add(new AtomResourceLink(
                                    new SyndicationLink(new Uri(operation.Href), "xyz", "Tile GetMap", operation.Type, 0)));
                            }
                        }
                    }
                }
            }
            return links;
        }
    }
}
