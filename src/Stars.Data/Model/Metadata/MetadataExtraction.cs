using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Sar;

namespace Terradue.Stars.Data.Model.Metadata
{
    public abstract class MetadataExtraction : IProcessing
    {
        protected ILogger logger;

        public MetadataExtraction(ILogger logger)
        {
            this.logger = logger;
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public ProcessingType ProcessingType => ProcessingType.MetadataExtractor;

        public abstract string Label { get; }

        public abstract bool CanProcess(IResource route, IDestination destination);

        public string GetRelativePath(IResource route, IDestination destination)
        {
            return "";
        }

        public async Task<IResource> Process(IResource resource, IDestination destination, string suffix = null)
        {
            IItem item = resource as IItem;
            if (item == null) return resource;

            return await ExtractMetadata(item, suffix);
        }

        protected abstract Task<StacNode> ExtractMetadata(IItem route, string suffix);

        internal IAsset FindFirstAssetFromFileNameRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.OrderBy(kvp => kvp.Key, StringComparer.InvariantCultureIgnoreCase).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Values.FirstOrDefault(a =>
            {
                return Regex.IsMatch(Path.GetFileName(a.Uri.ToString()), pattern);
            });
        }

        internal KeyValuePair<string, IAsset> FindFirstKeyAssetFromFileNameRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.OrderBy(kvp => kvp.Key, StringComparer.InvariantCultureIgnoreCase).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).FirstOrDefault(a =>
            {
                return Regex.IsMatch(Path.GetFileName(a.Value.Uri.ToString()), pattern);
            });
        }

         internal IEnumerable<IAsset> FindAssetsFromFileNameRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.Values.Where(a =>
            {
                return Regex.IsMatch(Path.GetFileName(a.Uri.ToString()), pattern);
            });
        }

        internal IAsset FindFirstAssetFromFilePathRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.Values.FirstOrDefault(a =>
            {
                return Regex.IsMatch(a.Uri.ToString(), pattern);
            });
        }
        
        protected Dictionary<string, IAsset> FindAllAssetsFromFileNameRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.Where(a =>
            {
                return Regex.IsMatch(Path.GetFileName(a.Value.Uri.ToString()), pattern);
            }).OrderBy(k => k.Key).ToDictionary(a => a.Key, a => a.Value);
        }

        public static string StylePlatform(string v)
        {
            string styled = v;
            if (v.Length == 1)
                styled = char.ToUpper(v[0]) + "";
            else
                styled = char.ToUpper(v[0]) + v.Substring(1);

            if (styled.Contains("-"))
                styled = styled.Split('-')[0] + "-" + string.Join("-", styled.Split('-').Skip(1).Select(s => s.ToUpper()));

            return styled;
        }

        protected EoBandCommonName GetEoCommonName(string imageColor)
        {
            EoBandCommonName eoBandCommonName = default(EoBandCommonName);

            if ( Enum.TryParse<EoBandCommonName>(imageColor, true, out eoBandCommonName))
                return eoBandCommonName;
                
            switch ( imageColor.ToLower()){
                case "near infrared":
                    return EoBandCommonName.nir;
            }

            return eoBandCommonName;
        }

        protected ObservationDirection? ParseObservationDirection(string lookDirection)
        {
            ObservationDirection observationDirection = ObservationDirection.Left;
            if ( Enum.TryParse<ObservationDirection>(lookDirection, out observationDirection) )
                return observationDirection;

            if (lookDirection.ToLower().StartsWith("r"))
                return ObservationDirection.Right;

            if (lookDirection.ToLower().StartsWith("l"))
                return ObservationDirection.Left;

            return null;
        }
    }
}