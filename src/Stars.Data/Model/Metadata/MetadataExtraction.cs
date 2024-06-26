﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: MetadataExtraction.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Sar;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata
{
    public abstract class MetadataExtraction : IProcessing
    {
        protected ILogger logger;
        protected readonly IResourceServiceProvider resourceServiceProvider;

        public MetadataExtraction(ILogger logger,
                                  IResourceServiceProvider resourceServiceProvider)
        {
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            IncludeProviderProperty = true;
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public ProcessingType ProcessingType => ProcessingType.MetadataExtractor;

        public abstract string Label { get; }

        public virtual bool IncludeProviderProperty { get; set; }

        public abstract bool CanProcess(IResource route, IDestination destination);

        public string GetRelativePath(IResource route, IDestination destination)
        {
            return "";
        }

        public async Task<IResource> ProcessAsync(IResource resource, IDestination destination, CancellationToken ct, string suffix = null)
        {
            if (!(resource is IItem item)) return resource;

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

        protected IEnumerable<IAsset> FindAssetsFromFileNameRegex(IAssetsContainer assetsContainer, string pattern)
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

        protected IEnumerable<IAsset> FindAssetsFromFilePathRegex(IAssetsContainer assetsContainer, string pattern)
        {
            return assetsContainer.Assets.Values.Where(a =>
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

            if (Enum.TryParse(imageColor, true, out EoBandCommonName eoBandCommonName))
                return eoBandCommonName;

            switch (imageColor.ToLower())
            {
                case "near infrared":
                    return EoBandCommonName.nir;
            }

            return eoBandCommonName;
        }

        protected ObservationDirection? ParseObservationDirection(string lookDirection)
        {
            ObservationDirection observationDirection = ObservationDirection.Left;
            if (Enum.TryParse(lookDirection, out observationDirection))
                return observationDirection;

            if (lookDirection.ToLower().StartsWith("r"))
                return ObservationDirection.Right;

            if (lookDirection.ToLower().StartsWith("l"))
                return ObservationDirection.Left;

            return null;
        }

        protected void AddSingleProvider(IDictionary<string, object> properties, string name, string description, IEnumerable<StacProviderRole> roles, Uri uri)
        {
            StacProvider provider = new StacProvider(name, roles);
            provider.Description = description;
            provider.Uri = uri;
            properties.Add("providers", new StacProvider[] { provider });
        }
    }
}
