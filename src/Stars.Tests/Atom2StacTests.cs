using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Data.Translators;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Translator;
using Xunit;

namespace Stars.Tests
{
    [Collection(nameof(StarsCollection))]
    public class Atom2StacTests
    {
        private readonly IServiceProvider serviceProvider;

        public Atom2StacTests(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Fact]
        public void AutoAtom2Stac()
        {
            AtomRouter router = new AtomRouter(null);
            var uri = new Uri("file://" + Path.Combine(Environment.CurrentDirectory, "../../../In/call864_S2B_MSIL1C_20210303T095029_N0209_R079_T33SWB_20210303T105137.atom"));
            AtomFeed atomFeed = AtomFeed.Load(XmlReader.Create(uri.AbsolutePath));
            AtomItemNode item = new AtomItemNode(atomFeed.Items.First() as AtomItem, uri);
            TranslatorManager translatorManager = new TranslatorManager(serviceProvider.GetService<ILogger<TranslatorManager>>(), serviceProvider);
            var stacNode = translatorManager.TranslateAsync<StacItemNode>(item, CancellationToken.None).GetAwaiter().GetResult();
            var stacItem = stacNode.StacItem;
            Assert.Equal("call864_S2B_MSIL1C_20210303T095029_N0209_R079_T33SWB_20210303T105137", stacItem.Id);
            Assert.Equal("optical", stacItem.GetProperty("sensor_type"));
        }

        [Fact]
        public async Task RemoteAtom2Stac()
        {
            var routersManager = serviceProvider.GetService<RoutersManager>();
            var translatorManager = serviceProvider.GetService<TranslatorManager>();
            var resourceServiceProvider = serviceProvider.GetService<IResourceServiceProvider>();
            var route = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri("https://catalog.terradue.com/sentinel1/search?format=atom&uid=S1A_IW_GRDH_1SDV_20211018T111323_20211018T111348_040173_04C21B_421A&do=[terradue]")), CancellationToken.None);
            var router = await routersManager.GetRouterAsync(route);
            var resource = await router.RouteAsync(route, CancellationToken.None);
            while (resource is ICatalog)
            {
                var children = (resource as ICatalog).GetRoutes(null);
                if (children.Count() > 1)
                {
                    throw new NotSupportedException($"multiple items are not supported to be an acquisition");
                }
                resource = children.First();
            }
            var stacItemNode = await translatorManager.TranslateAsync<StacItemNode>(resource, CancellationToken.None);
            Assert.Equal("S1A_IW_GRDH_1SDV_20211018T111323_20211018T111348_040173_04C21B_421A", stacItemNode.StacItem.Id);
            Assert.Equal(2, stacItemNode.StacItem.Assets.Count);
            Assert.Equal(1, stacItemNode.StacItem.Assets.Where(a => a.Value.Roles.Contains("data")).Count());
            Assert.Equal(1, stacItemNode.StacItem.Assets.Where(a => a.Value.Roles.Contains("thumbnail")).Count());
            Assert.Equal("radar", stacItemNode.StacItem.GetProperty("sensor_type"));
        }

        [Fact]
        public void AutoAtom2Stac2()
        {
            AtomRouter router = new AtomRouter(null);
            var uri = new Uri("file://" + Path.Combine(Environment.CurrentDirectory, "../../../In/call922_S2B_MSIL1C_20230503T073619_N0509_R092_T36JVR_20230503T112437-calibrated.atom"));
            AtomFeed atomFeed = AtomFeed.Load(XmlReader.Create(uri.AbsolutePath));
            AtomItemNode item = new AtomItemNode(atomFeed.Items.First() as AtomItem, uri);
            TranslatorManager translatorManager = new TranslatorManager(serviceProvider.GetService<ILogger<TranslatorManager>>(), serviceProvider);
            var stacNode = translatorManager.TranslateAsync<StacItemNode>(item, CancellationToken.None).GetAwaiter().GetResult();
            var stacItem = stacNode.StacItem;
            Assert.Equal("call922_S2B_MSIL1C_20230503T073619_N0509_R092_T36JVR_20230503T112437-calibrated", stacItem.Id);
            Assert.Equal("optical", stacItem.GetProperty("sensor_type"));
            Assert.Contains(stacItem.Links, l => l.RelationshipType == "xyz" && l.Uri == new Uri("https://titiler.disasterscharter.org/stac/tiles/WebMercatorQuad/{z}/{x}/{y}.png?url=s3://cpe-operations-catalog/calls/call-922/calibratedDatasets/S2B_MSIL1C_20230503T073619_N0509_R092_T36JVR_20230503T112437-calibrated/S2B_MSIL1C_20230503T073619_N0509_R092_T36JVR_20230503T112437-calibrated.json&assets=overview-trc&rescale=0,255&color_formula=&resampling_method=average"));
            File.WriteAllText("../../../Out/call922_S2B_MSIL1C_20230503T073619_N0509_R092_T36JVR_20230503T112437-calibrated.json", StacConvert.Serialize(stacItem));
        }

        
    }
}
