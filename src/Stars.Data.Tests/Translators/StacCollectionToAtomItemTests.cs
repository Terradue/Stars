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
using System.IO;

namespace Terradue.Data.Tests.Translators
{
    public class StacCollectionToAtomItemTests : TestBase
    {
        [Fact]
        public async System.Threading.Tasks.Task EGMS()
        {
            string json = GetJson("Translators");

            ValidateJson(json);

            StacCollection stacCollection = StacConvert.Deserialize<StacCollection>(json);

            StacCollectionToAtomItemTranslator stacCollectionToAtomItemTranslator = new StacCollectionToAtomItemTranslator(ServiceProvider);

            StacCollectionNode stacItemNode = new StacCollectionNode(stacCollection, new System.Uri("https://localhost:5001/api/ns/emathot/cs/0000024-220301000006171-oozie-oozi-W"));

            AtomItemNode atomItemNode = await stacCollectionToAtomItemTranslator.TranslateAsync<AtomItemNode>(stacItemNode, CancellationToken.None);

            var fs = File.OpenWrite("egms.atom");
            atomItemNode.GetStreamAsync(CancellationToken.None).Result.CopyTo(fs);
            fs.Close();

            bool egmsIsPresent = false;
            bool wmsIsPresent = false;
            if (atomItemNode.AtomItem.ElementExtensions != null && atomItemNode.AtomItem.ElementExtensions.Count > 0)
			{
                var offerings = atomItemNode.AtomItem.ElementExtensions.ReadElementExtensions<OwcOffering>("offering", OwcNamespaces.Owc, new System.Xml.Serialization.XmlSerializer(typeof(OwcOffering)));

				foreach (var offering in offerings)
				{
                    if(offering != null && offering.Code == "http://www.terradue.com/egms") egmsIsPresent = true;
                    if(offering != null && offering.Code == "http://www.opengis.net/spec/owc-atom/1.0/req/wms") wmsIsPresent = true;
                }
              
            }

            Assert.True(egmsIsPresent);   
            Assert.True(wmsIsPresent);           
        }

    }

}
