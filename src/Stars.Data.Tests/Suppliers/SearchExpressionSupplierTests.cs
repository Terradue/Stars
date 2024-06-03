using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Api.Converters;
using Stac.Api.Models.Cql2;
using Terradue.Stars.Interface.Supplier;
using Xunit;
using Xunit.Abstractions;

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
