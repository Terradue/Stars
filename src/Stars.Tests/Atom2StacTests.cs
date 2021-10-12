using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Data.Translators;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Translator;
using Xunit;

namespace Stars.Tests
{
    public class Atom2StacTests
    {
        [Fact]
        public void AutoAtom2Stac()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddTransient<ITranslator, StacLinkTranslator>();
            services.AddTransient<ITranslator, AtomToStacTranslator>();
            services.AddTransient<ITranslator, DefaultStacTranslator>();
            services.AddTransient<ICredentials, ConfigurationCredentialsManager>();
            var sp = services.BuildServiceProvider();

            AtomRouter router = new AtomRouter(null);
            var uri = new Uri("file://" + Path.Combine(Environment.CurrentDirectory, "../../../In/call864_S2B_MSIL1C_20210303T095029_N0209_R079_T33SWB_20210303T105137.atom"));
            AtomFeed atomFeed = AtomFeed.Load(XmlReader.Create(uri.AbsolutePath));
            AtomItemNode item = new AtomItemNode(atomFeed.Items.First() as AtomItem, uri, sp.GetService<ICredentials>());
            TranslatorManager translatorManager = new TranslatorManager(sp.GetService<ILogger<TranslatorManager>>(), sp);
            var stacNode = translatorManager.Translate<StacItemNode>(item).GetAwaiter().GetResult();
            var stacItem = stacNode.StacItem;
            Assert.Equal("S2B_MSIL1C_20210303T095029_N0209_R079_T33SWB_20210303T105137", stacItem.Id);
        }
    }
}
