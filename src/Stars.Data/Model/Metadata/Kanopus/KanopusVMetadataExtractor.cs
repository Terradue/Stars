using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Kanopus
{
    public class KanopusVMetadataExtractor : MetadataExtraction
    {
        public override string Label => "Kanopus-V(-IK) (Roscosmos) missions product metadata extractor";
        public KanopusVMetadataExtractor(ILogger<KanopusVMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                KanopusVMetadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                return metadata.GetString("<PASP_ROOT>/cDataFileName").StartsWith("KV") || metadata.GetString("<PASP_ROOT>/cDataFileName").StartsWith("fr_KV");
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            KanopusVMetadata metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata);

            AddAssets(stacItem, item, metadata);

            return StacItemNode.Create(stacItem, item.Uri); ;
        }


        internal virtual StacItem CreateStacItem(KanopusVMetadata metadata)
        {
            var items = metadata.GetString("<PASP_ROOT>/cDataFileName").Split('_');
            string identifier = string.Join("_", items.Take(7));
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            AddProjStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            AddEoStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            AddOtherProperties(metadata, stacItem.Properties);

            return stacItem;
        }

        private void AddEoStacExtension(KanopusVMetadata metadata, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }


        private void AddProjStacExtension(KanopusVMetadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = metadata.GetLong("<GeoCoding>/nCoordSystCode");
            proj.Shape = new int[2] { metadata.GetInt("<Matrix>/nWidth"), metadata.GetInt("<Matrix>/nHeight") };
        }

        private void AddProcessingStacExtension(KanopusVMetadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }

        private string GetProcessingLevel(KanopusVMetadata metadata)
        {
            return string.Format("L{0}", metadata.GetString("<PASP_ROOT>/cProcLevel"));
        }

        private IDictionary<string, object> GetCommonMetadata(KanopusVMetadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillPlatformDefinition(metadata, properties);


            return properties;
        }

        private void FillBasicsProperties(KanopusVMetadata metadata, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture))
            );
        }

        private void AddOtherProperties(KanopusVMetadata metadata, IDictionary<string, object> properties)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    properties,
                    "Roscosmos",
                    "Kanopus-V is an Earth observation minisatellite mission of the Russian Space Agency, Roscosmos and ROSHYDROMET/Planeta. The overall objective is to monitor Earth's surface, the atmosphere, ionosphere, and magnetosphere to detect and study the probability of strong earthquake occurrence.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.eoportal.org/satellite-missions/kanopus-v-1")
                );
            }
        }


        private void FillDateTimeProperties(KanopusVMetadata metadata, Dictionary<string, object> properties)
        {
            var format = "yyyy-dd-MMTHH:mm:ssZ";

            DateTime dt = DateTime.MinValue;

            if (!DateTime.TryParseExact(metadata.GetString("<PASP_ROOT>/dSessionDateUTC") + " " + metadata.GetString("<PASP_ROOT>/tSessionTimeUTC"),
                format, null, DateTimeStyles.AssumeUniversal, out dt))
            {
                int[] date = Array.ConvertAll(metadata.GetString("<PASP_ROOT>/dSessionDateUTC").Split('/'),
                    s => int.Parse(s));
                string stime = metadata.GetString("<PASP_ROOT>/tSessionTimeUTC").Split('.')[0];
                int milli = int.Parse(metadata.GetString("<PASP_ROOT>/tSessionTimeUTC").Split('.')[1]);
                int[] time = Array.ConvertAll(stime.Split(':'),
                    s => int.Parse(s));
                dt = new DateTime(date[2], date[1], date[0], time[0], time[1], time[2], milli, DateTimeKind.Utc);
            }

            properties["datetime"] = dt.ToUniversalTime().ToString("O");
        }

        private void FillPlatformDefinition(KanopusVMetadata metadata, Dictionary<string, object> properties)
        {
            string platform = "kanopus-v";
            if (metadata.GetString("<PASP_ROOT>/cDataFileName").StartsWith("KVIK", true, CultureInfo.InvariantCulture))
                platform = "kanopus-v-ik";
            properties.Remove("mission");
            properties.Add("mission", platform);
            properties.Remove("platform");
            properties.Add("platform", platform);
            properties.Remove("constellation");
            properties.Add("constellation", "kanopus-v");
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { "mss-pss" });
            properties["sensor_type"] = "optical";
        }


        private IGeometryObject GetGeometry(KanopusVMetadata metadata)
        {
            List<Position> positions = new List<Position>();
            for (int i = 0; i < metadata.GetString("<Polygon>/bLat").Split(',').Length; i++)
            {
                positions.Add(new Position(
                    metadata.GetDouble("<Polygon>/bLat")[i],
                    metadata.GetDouble("<Polygon>/bLon")[i])
                );
            }
            positions.Add(positions.First());
            LineString lineString = new LineString(
                positions.ToArray()
            );
            return new Polygon(new LineString[] { lineString }).NormalizePolygon();
        }


        private StacAsset GetBandAsset(StacItem stacItem, string name, IAsset asset, KanopusVMetadata metadata)
        {
            var numberOfChannels = 0;
            foreach (var item in metadata.properties.Keys)
            {
                if (item.StartsWith("<Ch"))
                {
                    numberOfChannels++;
                }
            }

            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri, new ContentType("image/tiff; application=geotiff"));
            stacAsset.Properties.AddRange(asset.Properties);

            EoBandObject[] eoarr = new EoBandObject[numberOfChannels];
            double[] channelValues;
            double mean;

            if (name == "MSS")
            {
                for (int i = 0; i < numberOfChannels; i++)
                {
                    channelValues = Array.ConvertAll((metadata.GetString("<Ch" + (i + 1) + ">/bSpectralZone").Split(',')), (double.Parse));
                    mean = channelValues.Sum() / channelValues.Length;

                    if (mean < 0.69 && mean > 0.63)
                    {
                        eoarr[i] = new EoBandObject("red", EoBandCommonName.red);
                        eoarr[i].Properties.Add("description", "red");
                    }

                    else if (mean < 0.60 && mean > 0.52)
                    {
                        eoarr[i] = new EoBandObject("green", EoBandCommonName.green);
                        eoarr[i].Properties.Add("description", "green");
                    }

                    else if (mean < 0.52 && mean > 0.44)
                    {
                        eoarr[i] = new EoBandObject("blue", EoBandCommonName.blue);
                        eoarr[i].Properties.Add("description", "blue");
                    }

                    else if (mean < 0.84 && mean > 0.75)
                    {
                        eoarr[i] = new EoBandObject("nir", EoBandCommonName.nir);
                        eoarr[i].Properties.Add("description", "nir");
                    }

                }
                // Swap bands 1 and 3 (red and blue) if the channels are not in RGB order in the metadata
                if (numberOfChannels >= 3 && eoarr[0].CommonName == EoBandCommonName.blue && eoarr[2].CommonName == EoBandCommonName.red)
                {
                    (eoarr[2], eoarr[0]) = (eoarr[0], eoarr[2]);
                }
                stacAsset.EoExtension().Bands = eoarr;
            }
            else if (name == "PAN")
            {
                EoBandObject eoBandObject = new EoBandObject("pan", EoBandCommonName.pan);
                eoBandObject.Properties.Add("description", "pan");
                stacAsset.EoExtension().Bands = new EoBandObject[] { eoBandObject };
            }

            return stacAsset;
        }



        protected void AddAssets(StacItem stacItem, IItem item, KanopusVMetadata metadata)
        {
            IAsset mssAsset = FindFirstAssetFromFileNameRegex(item, @".*MSS.*\.tiff");
            IAsset sAsset = FindFirstAssetFromFileNameRegex(item, @".*_S_.*\.tiff");
            if (mssAsset != null)
            {
                var bandAsset = GetBandAsset(stacItem, "MSS", mssAsset, metadata);
                stacItem.Assets.Add("mss", bandAsset);
            }
            else if (sAsset != null)
            {
                var bandAsset = GetBandAsset(stacItem, "MSS", sAsset, metadata);
                stacItem.Assets.Add("mss", bandAsset);
            }


            IAsset pssAsset = FindFirstAssetFromFileNameRegex(item, @".*PSS.*\.tiff");
            if (pssAsset != null)
            {
                var bandAsset = GetBandAsset(stacItem, "PAN", pssAsset, metadata);
                stacItem.Assets.Add("pan", bandAsset);
            }

        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"^(KV|fr_KV).*\.xml");
            if (manifestAsset != null)
                return manifestAsset;
            manifestAsset = FindFirstAssetFromFileNameRegex(item, @".*MSS.*\.xml");
            if (manifestAsset != null) return manifestAsset;

            throw new FileNotFoundException(string.Format("Unable to find the summary file asset"));
        }

        public virtual async Task<KanopusVMetadata> ReadMetadata(IAsset manifestAsset)
        {
            KanopusVMetadata metadata = new KanopusVMetadata(manifestAsset);

            await metadata.ReadMetadata(resourceServiceProvider);

            return metadata;
        }

    }


    public class KanopusVMetadata
    {
        private IAsset summaryAsset;
        public Dictionary<string, Dictionary<string, string>> properties { get; set; }
        public List<string> Assets { get; set; }

        public KanopusVMetadata(IAsset summaryAsset)
        {
            this.summaryAsset = summaryAsset;
            properties = new Dictionary<string, Dictionary<string, string>>();
            Assets = new List<string>();
        }

        public async Task ReadMetadata(IResourceServiceProvider resourceServiceProvider)
        {
            string key = null;

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(summaryAsset, System.Threading.CancellationToken.None))
            {

                using (StreamReader reader = new StreamReader(stream))
                {

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line.Trim().StartsWith("<") && !line.Trim().StartsWith("</"))
                        {
                            key = line;
                            continue;
                        }
                        if (line.Trim().StartsWith("</")) { continue; }
                        if (properties.ContainsKey(key))
                        {
                            properties[key].Add(line.Split('=')[0].Trim(), line.Split('=')[1].Trim().Replace("\"", ""));
                        }
                        if (!properties.ContainsKey(key))
                        {
                            properties.Add(key, new Dictionary<string, string> { { line.Split('=')[0].Trim(), line.Split('=')[1].Trim().Replace("\"", "") } });
                        }
                    }
                }
            }
        }
        public string GetString(string key, bool throwIfMissing = true)
        {
            string[] keys = key.Split('/');
            if (properties[keys[0]].ContainsKey(keys[1]))
            {
                return properties[keys[0]][keys[1]];
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return null;
        }

        public long GetLong(string key, bool throwIfMissing = true)
        {
            string[] keys = key.Split('/');
            if (properties[keys[0]].ContainsKey(keys[1]))
            {
                if (long.TryParse(properties[keys[0]][keys[1]], out long value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not an int)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return 0;
        }

        public int GetInt(string key, bool throwIfMissing = true)
        {
            string[] keys = key.Split('/');
            if (properties[keys[0]].ContainsKey(keys[1]))
            {
                if (int.TryParse(properties[keys[0]][keys[1]], out int value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not an int)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return 0;
        }

        public double[] GetDouble(string key, bool throwIfMissing = true)
        {
            string[] keys = key.Split('/');
            if (properties[keys[0]].ContainsKey(keys[1]))
            {
                double[] value = Array.ConvertAll(properties[keys[0]][keys[1]].Split(','), (double.Parse));
                return value;
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return null;
        }
    }
}
