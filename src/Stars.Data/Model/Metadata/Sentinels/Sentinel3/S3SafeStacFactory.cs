// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3SafeStacFactory.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stac;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel3
{
    public class S3SafeStacFactory : SentinelSafeStacFactory
    {

        public S3SafeStacFactory(XFDUType xfdu, IItem route, string identifier) : base(xfdu, route, identifier)
        {
        }

        public static S3SafeStacFactory Create(XFDUType xfdu, IItem route, string identifier)
        {
            return new S3SafeStacFactory(xfdu, route, identifier);
        }

        protected override string GetTitle(IDictionary<string, object> properties)
        {
            return string.Format("{0} {1} {2}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                GetDerivedProductType(),
                GetProcessingLevel(),
                properties.GetProperty<int>("sat:relative_orbit")
            );
        }

        public string GetInstrumentMode()
        {
            var platformMetadata = xfdu.metadataSection.FirstOrDefault(m => m.ID == "platform") ?? throw new ArgumentException("No platform metadata found in the manifest of " + xfdu.ID);
            try
            {
                return platformMetadata.metadataWrap.xmlData.platform.instrument.mode.identifier;
            }
            catch { }

            return null;
        }

        public override string GetProductType()
        {
            var generalProductInformation = xfdu.metadataSection.FirstOrDefault(m => m.ID == "generalProductInformation") ?? throw new InvalidDataException("No generalProductInformation metadata found in the manifest of " + xfdu.ID);
            try
            {
                return generalProductInformation.metadataWrap.xmlData.generalProductInformation.productTypeS31.ToString();
            }
            catch { }

            return "RAW";
        }

        public override string GetDerivedProductType()
        {
            return GetProductType();
        }

        protected override string GetProcessingLevel()
        {
            string productType = GetProductType();
            if (productType != null && productType.Length > 4)
            {
                return String.Format("L{0}", productType[3]);
            }
            return null;
        }

        protected override string GetSensorType(metadataObjectType platformMetadata)
        {
            string instrument = GetInstrumentName(platformMetadata);
            if (instrument == "SRAL") return "radar";
            else return "optical";
        }

        public override double GetGroundSampleDistance() => 1000;

        protected override void AddProjectionStacExtension(StacItem stacItem)
        {
            stacItem.ProjectionExtension().Epsg = null;
        }

        public string GetAssetTitle(string filename)
        {
            foreach (var d in xfdu.dataObjectSection ?? new dataObjectType[0])
            {
                foreach (var b in d.byteStream ?? new byteStreamType[0])
                {
                    if (b.fileLocation.Length != 0 && b.fileLocation[0].href.Contains(filename))
                    {
                        return b.fileLocation[0].textInfo;
                    }
                    continue;
                }
            }
            return filename;
        }
    }
}
