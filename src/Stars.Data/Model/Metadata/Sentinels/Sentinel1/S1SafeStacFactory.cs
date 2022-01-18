using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stac;
using Stac.Extensions.Sar;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Stac.Extensions.Projection;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel1
{
    public class S1SafeStacFactory : SentinelSafeStacFactory
    {

        public S1SafeStacFactory(XFDUType xfdu, IItem route, string identifier) : base(xfdu, route, identifier)
        {
        }

        public static S1SafeStacFactory Create(XFDUType xfdu, IItem route, string identifier)
        {
            return new S1SafeStacFactory(xfdu, route, identifier);
        }

        internal override StacItem CreateStacItem()
        {
            StacItem stacItem = base.CreateStacItem();
            AddSarStacExtension(stacItem);
            return stacItem;
        }

        private void AddSarStacExtension(StacItem stacItem)
        {
            stacItem.SarExtension().Required (
                GetInstrumentMode(),
                GetFrequencyBand(),
                GetPolarizations(),
                GetProductType()
            );
        }

        protected override string GetTitle(IDictionary<string, object> properties)
        {
            return string.Format("{0} {1} {2} {3}", StylePlatform(properties.GetProperty<string>("platform")),
                                                  GetProductType(),
                                                  string.Join("/", GetPolarizations()),
                                                  properties.GetProperty<int>("sat:relative_orbit")
                                                  );
        }

        private string[] GetPolarizations()
        {
            var generalProductInformation = xfdu.metadataSection.First(m => m.ID == "generalProductInformation");
            if (generalProductInformation.metadataWrap.xmlData.s1SarL0GeneralProductInformation != null)
                return generalProductInformation.metadataWrap.xmlData.
                s1SarL0GeneralProductInformation.transmitterReceiverPolarisation.Select(pol => pol.Value.ToString()).ToArray();
            if (generalProductInformation.metadataWrap.xmlData.s1SarL1GeneralProductInformation != null)
                return generalProductInformation.metadataWrap.xmlData.
                s1SarL1GeneralProductInformation.transmitterReceiverPolarisation.Select(pol => pol.ToString()).ToArray();
            if (generalProductInformation.metadataWrap.xmlData.s1SarL2GeneralProductInformation != null)
                return generalProductInformation.metadataWrap.xmlData.
                s1SarL2GeneralProductInformation.transmitterReceiverPolarisation.Select(pol => pol.ToString()).ToArray();
            return new string[0];
        }

        private SarCommonFrequencyBandName GetFrequencyBand() => SarCommonFrequencyBandName.C;

        public override string GetInstrumentName(metadataObjectType platformMetadata) => "c-sar";

        public override double GetGroundSampleDistance()
        {
            return GetGroundSampleDistance(GetProductType(), GetInstrumentMode(), GetResolution());
        }

        private char GetResolution()
        {
            return Identifier.ToUpper()[10];
        }

        public string GetInstrumentMode()
        {
            var platformMetadata = xfdu.metadataSection.FirstOrDefault(m => m.ID == "platform");

            if (platformMetadata == null)
            {
                throw new ArgumentException("No platform metadata found in the manifest of " + xfdu.ID);
            }

            try
            {
                return platformMetadata.metadataWrap.xmlData.platform.instrument.extension.s1SarL0instrumentMode.mode.ToString();
            }
            catch { }

            try
            {
                return platformMetadata.metadataWrap.xmlData.platform.instrument.extension.s1SarL1instrumentMode.mode.ToString();
            }
            catch { }

            try
            {
                return platformMetadata.metadataWrap.xmlData.platform.instrument.extension.s1SarL2instrumentMode.mode.ToString();
            }
            catch { }

            return null;
        }

        private double GetGroundSampleDistance(string pt, string mode, char res)
        {
            if (pt == "RAW") return 0;
            if (pt == "GRD")
            {
                if (res == 'F' && mode == "SM") return 9;
                if (res == 'H')
                {
                    if (mode == "SM") return 23;
                    if (mode == "IW") return 22;
                    if (mode == "EW") return 50;
                }
                if (res == 'M')
                {
                    if (mode == "WV") return 25;
                    return 40;
                }
            }
            if (pt == "SLC")
            {
                if (mode == "SM") return 4.9;
                if (mode == "IW") return 22;
                if (mode == "EW") return 43;
                if (mode == "WV") return 4.8;
            }
            if (pt == "OCN") return 20000;

            return 0;
        }

        public override string GetProductType()
        {
            var generalProductInformation = xfdu.metadataSection.FirstOrDefault(m => m.ID == "generalProductInformation");
            if (generalProductInformation == null)
            {
                throw new InvalidDataException("No generalProductInformation metadata found in the manifest of " + xfdu.ID);
            }

            try
            {
                return generalProductInformation.metadataWrap.xmlData.s1SarL1GeneralProductInformation.productType.ToString();
            }
            catch { }

            try
            {
                return generalProductInformation.metadataWrap.xmlData.s1SarL2GeneralProductInformation.productType.ToString();
            }
            catch { }

            return "RAW";
        }

        protected override string GetProcessingLevel()
        {
            switch (GetProductType())
            {
                case "RAW":
                    return "RAW";
                case "SLC":
                case "GRD":
                    return "L1";
                case "OCN":
                    return "L2";
            }
            return null;
        }

        protected override string GetSensorType(metadataObjectType platformMetadata)
        {
            return "radar";
        }

        protected override void AddProjectionStacExtension(StacItem stacItem)
        {
            stacItem.ProjectionExtension().Epsg = null;
        }
    }
}