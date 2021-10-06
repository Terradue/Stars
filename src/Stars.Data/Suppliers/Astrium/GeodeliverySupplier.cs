using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface;
using System.Text.RegularExpressions;
using Stac;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Terradue.Stars.Services.Supplier;
using System.Runtime.ExceptionServices;
using System.Threading;
using Terradue.Stars.Services.Translator;
using System.Linq;
using Stac.Extensions.File;

namespace Terradue.Stars.Data.Suppliers.Astrium
{
    public class GeodeliverySupplier : ISupplier
    {
        protected ILogger logger;
        private readonly TranslatorManager translatorManager;
        private ICredentials credentials;

        public GeodeliverySupplier(ILogger<GeodeliverySupplier> logger, TranslatorManager translatorManager, ICredentials credentials)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.credentials = credentials;
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public string Id => "Astrium Geodelivery";

        public string Label => "Astrium Geodelivery (FTP)";

        public async Task<IResource> SearchFor(IResource node)
        {
            // TEMP skipping catalog for the moment
            if (!(node is IItem)) return null;

            // Let's translate the node to STAC
            var stacNode = await translatorManager.Translate<StacNode>(node);
            if (stacNode == null || !(stacNode is StacItemNode)) return null;
            StacItemNode stacItemNode = stacNode as StacItemNode;

            int[] callids = stacItemNode.Properties.GetProperty<int[]>("disaster:call_ids");

            if (callids == null)
            {
                try
                {
                    var callidstr = stacItemNode.Id.Split('-').First();
                    callids = new int[] { int.Parse(callidstr) };
                }
                catch { }
            }

            if (callids == null) return null;

            Match match = Regex.Match(stacItemNode.Id, @".*urn[-_]ogc[-_]def[-_]EOP[-_].*[-_](?'pattern'DS_.*)");

            if (!match.Success)
                return null;

            string pattern = match.Groups["pattern"].Value;

            IDictionary<string, IAsset> assets = new Dictionary<string, IAsset>();

            foreach (var callid in callids)
            {
                logger.LogDebug("Searching for {0} for call {1}", pattern, callid);
                await AddAssets(pattern, callid, stacItemNode);
            }

            if (assets.Count == 0) return null;

            return stacItemNode;
        }

        private async Task AddAssets(string pattern, int callid, StacItemNode itemNode)
        {
            string username = string.Format("CHARTER_END{0}", callid.ToString().Last());
            var ftpUri = new Uri(string.Format("ftp://{0}@geodelivery.astrium-geo.com/", username));
            List<string> allLines = new List<string>();
            for (int i = 1; i < 30; i++)
            {
                try
                {
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(ftpUri);
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = credentials;
                    reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    reqFTP.Timeout = System.Threading.Timeout.Infinite;
                    reqFTP.Proxy = null;
                    reqFTP.KeepAlive = true;
                    reqFTP.UsePassive = true;
                    using (FtpWebResponse response = (FtpWebResponse)await reqFTP.GetResponseAsync())
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string line = reader.ReadLine();
                            while (line != null)
                            {
                                Regex regex = new Regex ( @"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace );
                                Match detail = regex.Match(line);
                                if ( !detail.Success ) continue;
                                ulong contentLength = ulong.Parse(detail.Groups[3].Value);
                                allLines.Add(detail.Groups[6].Value);
                                Match match = Regex.Match(detail.Groups[6].Value, string.Format("CHARTER_ID{0}_(?'aoi'.+)_{1}.*\\.zip$", callid, pattern));
                                if (match.Success)
                                {
                                    logger.LogDebug("Asset found {0}", detail.Groups[6].Value);
                                    Uri enclosure = new Uri(string.Format("ftp://{0}@geodelivery.astrium-geo.com/{1}", username, detail.Groups[6].Value));
                                    itemNode.StacItem.Assets.Add(match.Groups["aoi"].Value,
                                        new StacAsset(itemNode.StacItem, enclosure, new string[] { "data" },
                                                                string.Format("Astrium Charter data {0} for call {1}", match.Groups["aoi"].Value, callid),
                                                                 new System.Net.Mime.ContentType("application/zip")));
                                    itemNode.StacItem.Assets[match.Groups["aoi"].Value].FileExtension().Size = contentLength;
                                }
                                line = reader.ReadLine();
                            }
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("try again"))
                        logger.LogWarning("Server returned an error but ask to try again : {0}", e.Message);
                    else
                        ExceptionDispatchInfo.Capture(e).Throw();
                }
                Thread.Sleep(Convert.ToUInt16(i * 10 * 1000));
            }
        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }
    }
}