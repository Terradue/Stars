using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpKml.Engine;
using Stac;
using Stac.Extensions.Eo;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.NewSat
{
    public class NewSatMetadataExtractor : MetadataExtraction
    {
        public override string Label => "NewSat (Satellogic) contellation product metadata extractor";

        public NewSatMetadataExtractor(ILogger<NewSatMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            IAsset geojson = FindFirstAssetFromFileNameRegex(item, "^[0-9a-zA-Z_-]*(metadata_stac\\.geojson)$");
            if (geojson == null)
            {
                return false;
            }
            IStreamResource geoJsonFileStreamable = resourceServiceProvider.GetStreamResourceAsync(geojson, System.Threading.CancellationToken.None).Result;
            if (geoJsonFileStreamable == null)
            {
                return false;
            }
            try
            {
                DeserializeProductMetadata(geoJsonFileStreamable, item).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the geojson file in the product package");
            IAsset geojsonAsset = FindFirstAssetFromFileNameRegex(item, "^[0-9a-zA-Z_-]*(metadata_stac\\.geojson)$") ?? throw new FileNotFoundException(string.Format("Unable to find the geojson file asset"));
            logger.LogDebug(string.Format("geojson file is {0}", geojsonAsset.Uri));
            IStreamResource geoJsonFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(geojsonAsset, System.Threading.CancellationToken.None);
            if (geoJsonFileStreamable == null)
            {
                logger.LogError("geojson file asset is not streamable, skipping metadata extraction");
                return null;
            }

            // deserialize existing geojson
            StacItem stacItem = await DeserializeProductMetadata(geoJsonFileStreamable, item);

            // modifies the URI of the original geojson
            // and changes the vrt assets to tif assets
            FixAssetPaths(stacItem, item);

            // Adds missing properties
            FillMissingProperties(stacItem);

            // Adds other properties (provider)
            AddOtherProperties(stacItem);

            // changes unit of measures in band properties
            UpdateBandsValues(stacItem);

            // add L3 visual tif asset
            AddL3Assets(item, stacItem);

            return StacItemNode.Create(stacItem, item.Uri);
        }

        private void FixAssetPaths(StacItem stacItem, IItem item)
        {

            foreach (var stacItemAsset in stacItem.Assets)
            {
                string filename = Path.GetFileName(stacItemAsset.Value.Uri.GetPath());

                IAsset file;
                if (filename.EndsWith(".vrt"))
                {
                    var filenameNoExtension = filename.Replace(".vrt", "");
                    file = FindFirstAssetFromFileNameRegex(item, @".*" + filenameNoExtension + "_[0-9]\\.tif");
                    filename = Path.GetFileName(file.Uri.AbsolutePath);
                }
                else
                {
                    file = FindFirstAssetFromFileNameRegex(item, @".*" + filename);
                    if (file == null && stacItemAsset.Key == "preview")
                    {
                        file = FindFirstAssetFromFileNameRegex(item, @".*_preview.png");
                    }
                    else if (file == null && stacItemAsset.Key == "thumbnail")
                    {
                        file = FindFirstAssetFromFileNameRegex(item, @".*_thumbnail.png");
                    }
                    else if (file == null)
                    {
                        throw new FileNotFoundException("Unable to find " + stacItemAsset.Value.Uri);
                    }
                }
                stacItemAsset.Value.Uri = file.Uri;
                stacItemAsset.Value.SetProperty("filename", filename);
            }
        }


        private static async Task<StacItem> DeserializeProductMetadata(IStreamResource geoJsonFileStreamable,
            IItem item)
        {
            StacItem stacItem;
            using (var stream = new StreamReader(geoJsonFileStreamable.Uri.AbsolutePath, Encoding.UTF8, true))
            {
                var json = await stream.ReadToEndAsync();

                // before deserializing we modify the instruments type from string to array of strings
                dynamic featureCollection = JsonConvert.DeserializeObject(json);

                // Transform instrument into array if it is a string
                if (featureCollection.features[0].properties.instruments is JValue)
                {
                    string instruments = featureCollection.features[0].properties.instruments;

                    featureCollection.features[0].properties.instruments = new JArray() as dynamic;
                    featureCollection.features[0].properties.instruments.Add(instruments.ToLower());
                }

                // retrieving first statItem of the collection
                string featureJson = featureCollection.features[0].ToString();

                // deserializing stacItem
                stacItem = StacConvert.Deserialize<StacItem>(featureJson);
            }

            return stacItem;
        }

        private void FillMissingProperties(StacItem stacItem)
        {
            RemoveUnusedProperties(stacItem);
            FillProcessingLevelProperty(stacItem);
            FillTitleProperty(stacItem);
            FillGsdProperty(stacItem);
            FillMissionProperty(stacItem);
        }

        private void RemoveUnusedProperties(StacItem stacItem)
        {
            stacItem.Properties.Remove("license");
            stacItem.Properties.Remove("created");
        }

        private void FillTitleProperty(StacItem stacItem)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            var properties = stacItem.Properties;
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)));
        }


        private void FillProcessingLevelProperty(StacItem stacItem)
        {
            string itemId = stacItem.Id;
            string processingLevel = itemId.Split('_')[3].ToLower();
            stacItem.Properties.Add("processing:level", processingLevel);
        }


        private void FillGsdProperty(StacItem stacItem)
        {
            string itemId = stacItem.Id;
            string resolution = itemId.Split('_')[4].ToLower();
            if (resolution.Equals("sr"))
            {
                stacItem.Gsd = 0.7;
            }
            else
            {
                stacItem.Gsd = 1;
            }
        }

        private void FillMissionProperty(StacItem stacItem)
        {
            stacItem.Mission = stacItem.Platform;
        }


        private void UpdateBandsValues(StacItem stacItem)
        {
            var bands = new Collection<EoBandObject>();
            stacItem.Assets["analytic"].EoExtension().Bands.ToList().ForEach(
                band =>
                {
                    band.CenterWavelength /= 1000;
                    band.FullWidthHalfMax /= 1000;
                    bands.Add(band);
                });
            stacItem.Assets["analytic"].EoExtension().Bands = bands.ToArray();
            stacItem.Assets["analytic"].Roles.Add("data");
            stacItem.Assets["analytic"].Roles.Add("reflectance");
            stacItem.Assets.Remove("quicklook");
        }

        private void AddOtherProperties(StacItem stacItem)
        {
            if (IncludeProviderProperty && !stacItem.Properties.ContainsKey("providers"))
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "Satellogic",
                    "Satellogic aims to provide real-time imaging of the entire planet on a daily basis.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://satellogic.com/technology/constellation/")
                );
            }
        }

        private void AddL3Assets(IItem item, StacItem stacItem)
        {
            IAsset l3Tif = FindFirstAssetFromFileNameRegex(item, @".*L3.*\.tif");

            var bands = new Collection<EoBandObject>();
            stacItem.Assets["analytic"].EoExtension().Bands.ToList().ForEach(
                band =>
                {
                    if (!band.Name.Equals("band4"))
                    {
                        bands.Add(band);
                    }
                });

            stacItem.Assets["visual"] = new StacAsset(stacItem, l3Tif.Uri);//stacItem.Assets["analytic"];
            stacItem.Assets["visual"].MediaType = new System.Net.Mime.ContentType("image/tiff");
            stacItem.Assets["visual"].EoExtension().Bands = bands.ToArray();
            stacItem.Assets["visual"].SetProperty("filename", Path.GetFileName(l3Tif.Uri.AbsolutePath));
            stacItem.Assets["visual"].Title = "3-Band Visual";
            stacItem.Assets["visual"].Roles.Add("visual");
        }

    }
}
