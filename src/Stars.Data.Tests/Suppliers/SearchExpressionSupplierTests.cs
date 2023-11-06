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
using System.Threading.Tasks;
using Google.Apis.Bigquery.v2.Data;
using Xunit.Sdk;

namespace Terradue.Data.Tests.Suppliers
{
    public class SearchExpressionSupplierTests : TestBase
    {
        private readonly JsonSerializerSettings _settings;

        public SearchExpressionSupplierTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new BooleanExpressionConverter());


        }

        public static IEnumerable<object[]> AllSuppliersTestsData
        {
            get
            {
                return new SuppliersTestsData();
            }
        }

        [Theory, MemberData("AllSuppliersTestsData", DisableDiscoveryEnumeration = true)]
        public async Task AllSuppliersSearchExpression(string key, ISupplier supplier, string file)
        {
            string json = File.ReadAllText(file);
            JObject jObject = JObject.Parse(json);
            var be = JsonConvert.DeserializeObject<BooleanExpression>(jObject["filter"].ToString(), _settings);
            CQL2Expression cql = new CQL2Expression(be);

            // Task<object> result = null;
            // get the result without the exception

            try
            {
                var task = await supplier.InternalSearchExpressionAsync(cql, CancellationToken.None);
                if (jObject["result"] != null)
                {
                    var resultJson = JsonConvert.SerializeObject(task);
                    JsonAssert.AreEqual(jObject["result"].ToString(), resultJson);
                }
            }
            catch (Exception e)
            {
                if (jObject["exception"] != null)
                {
                    Assert.Equal(Type.GetType(jObject["exception"].ToString()), e.GetType());
                }
                else
                {
                    throw new Exception($"{Path.GetFileName(file)}: {e.Message}", e);
                }
            }

        }

    }

}
