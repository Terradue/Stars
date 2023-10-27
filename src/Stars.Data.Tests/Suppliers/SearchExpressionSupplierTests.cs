using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Stac.Extensions.Projection;
using Terradue.Stars.Data.Translators;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Model.Atom;
using System.Threading;
using System.Xml;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;
using System;
using Terradue.Metadata.EarthObservation.OpenSearch.Extensions;
using Terradue.Metadata.EarthObservation.Ogc.Extensions;
using System.Collections.Generic;
using Terradue.Stars.Interface.Supplier;
using System.IO;
using Newtonsoft.Json.Linq;
using Stac.Api.Models.Cql2;
using Xunit.Abstractions;
using Stac.Api.Converters;
using Amazon.Runtime.Internal.Util;
using Terradue.Stars.Data.Suppliers;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Services.Translator;
using Microsoft.Extensions.Logging;

namespace Terradue.Data.Tests.Suppliers
{
    public class SearchExpressionSupplierTests : TestBase
    {
        private readonly JsonSerializerSettings _settings;

        public SearchExpressionSupplierTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new BooleanExpressionConverter());
            // Collection.AddSingleton<ISupplier, OpenSearchableSupplier>();
            // Collection.AddTransient<OpenSearchableSupplier>(sp =>
            // {
            //     var supplier = new OpenSearchableSupplier(sp.GetRequiredService<ILogger<OpenSearchableSupplier>>(),
            //                                               sp.GetRequiredService<TranslatorManager>());
            //     supplier.Key = "OpenSearchable";
            //     return supplier;
            // });
        }

        public static IEnumerable<object[]> AllSuppliersTestsData
        {
            get
            {
                return new SuppliersTestsData();
            }
        }

        [Theory, MemberData("AllSuppliersTestsData", DisableDiscoveryEnumeration = true)]
        public async void AllSuppliersSearchExpression(string key, string file, ISupplier supplier)
        {
            string json = File.ReadAllText(file);
            JObject jObject = JObject.Parse(json);
            var be = JsonConvert.DeserializeObject<BooleanExpression>(jObject["filter"].ToString(), _settings);
            CQL2Expression cql = new CQL2Expression(be);

            var result = await supplier.InternalSearchExpressionAsync(cql, CancellationToken.None);

            var resultJson = JsonConvert.SerializeObject(result);

            JsonAssert.AreEqual(jObject["result"].ToString(), resultJson);

        }

    }

}
