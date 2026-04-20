using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Terradue.Stars.Data.Model.Metadata.TerrasarX;
using Xunit;

namespace Terradue.Data.Tests.Harvesters
{
    public class TerrasarXMetadataExtractorTests
    {
        [Fact]
        public void GetProcessingLevel_ParsesLegacyOrderTypePrefix()
        {
            var metadata = BuildMetadata("L1B-SAR", level1ProductProductInfoProductVariantInfoProductVariant.EEC);

            string level = InvokeGetProcessingLevel(metadata);

            Assert.Equal("L1B", level);
        }

        [Fact]
        public void GetProcessingLevel_FallsBackToProductVariant_WhenOrderTypeHasNoSeparator()
        {
            var metadata = BuildMetadata("NRT", level1ProductProductInfoProductVariantInfoProductVariant.EEC);

            string level = InvokeGetProcessingLevel(metadata);

            Assert.Equal("EEC", level);
        }

        private static string InvokeGetProcessingLevel(level1Product metadata)
        {
            var extractor = new TerrasarXMetadataExtractor(NullLogger<TerrasarXMetadataExtractor>.Instance, null);
            MethodInfo method = typeof(TerrasarXMetadataExtractor).GetMethod("GetProcessingLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            object result = method.Invoke(extractor, new object[] { metadata });
            return Assert.IsType<string>(result);
        }

        private static level1Product BuildMetadata(string orderType, level1ProductProductInfoProductVariantInfoProductVariant productVariant)
        {
            return new level1Product
            {
                setup = new level1ProductSetup
                {
                    orderInfo = new level1ProductSetupOrderInfo
                    {
                        orderType = orderType
                    }
                },
                productInfo = new level1ProductProductInfo
                {
                    productVariantInfo = new level1ProductProductInfoProductVariantInfo
                    {
                        productVariant = productVariant
                    }
                }
            };
        }
    }
}
