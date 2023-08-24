// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: SentinelSafeStacFactory.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GeoJSON.Net.Geometry;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Sat;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Geometry.Gml321;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels
{
    public abstract class SentinelSafeStacFactory
    {
        protected XFDUType xfdu;
        protected IItem item;
        private readonly string identifier;

        protected SentinelSafeStacFactory(XFDUType xfdu, IItem item, string identifier)
        {
            this.xfdu = xfdu;
            this.item = item;
            this.identifier = identifier;
        }

        public virtual string Identifier => identifier;

        public XFDUType Manifest => xfdu;

        internal virtual StacItem CreateStacItem()
        {
            StacItem stacItem = new StacItem(Identifier, GetGeometry(), GetCommonMetadata());
            AddSatStacExtension(stacItem);
            AddProcessingStacExtension(stacItem);
            AddProjectionStacExtension(stacItem);
            FillBasicsProperties(stacItem.Properties);
            return stacItem;
        }

        protected abstract void AddProjectionStacExtension(StacItem stacItem);

        private void AddSatStacExtension(StacItem stacItem)
        {
            metadataObjectType measurementOrbitReference = xfdu.metadataSection.First(m => m.ID == "measurementOrbitReference");

            var sat = new SatStacExtension(stacItem);
            sat.OrbitState = GetOrbitState();
            var anx = GetAscendingNodeCrossingDateTime();
            if (anx.Ticks > 0)
                sat.AscendingNodeCrossingDateTime = anx;
            if (measurementOrbitReference != null && measurementOrbitReference.metadataWrap.xmlData.orbitReference != null)
            {
                sat.AbsoluteOrbit = Convert.ToInt32(measurementOrbitReference.metadataWrap.xmlData.orbitReference.orbitNumber.First().Value);
                sat.RelativeOrbit = Convert.ToInt32(measurementOrbitReference.metadataWrap.xmlData.orbitReference.relativeOrbitNumber.First().Value);
            }
            if (measurementOrbitReference != null && measurementOrbitReference.metadataWrap.xmlData.orbitReference11 != null)
            {
                sat.AbsoluteOrbit = Convert.ToInt32(measurementOrbitReference.metadataWrap.xmlData.orbitReference11.orbitNumber.First().Value);
                sat.RelativeOrbit = Convert.ToInt32(measurementOrbitReference.metadataWrap.xmlData.orbitReference11.relativeOrbitNumber.First().Value);
            }
            metadataObjectType platformMetadata = xfdu.metadataSection.First(m => m.ID == "platform");

            // platform & constellation
            if (platformMetadata.metadataWrap.xmlData.platform != null)
            {
                sat.PlatformInternationalDesignator = platformMetadata.metadataWrap.xmlData.platform.nssdcIdentifier;
            }
            if (platformMetadata.metadataWrap.xmlData.platform11 != null)
            {
                sat.PlatformInternationalDesignator = platformMetadata.metadataWrap.xmlData.platform11.nssdcIdentifier;
            }

        }

        private void AddProcessingStacExtension(StacItem stacItem)
        {
            var processing = xfdu.metadataSection.FirstOrDefault(ms => ms.ID == "processing");
            if (processing == null) return;

            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel();

            if (processing.metadataWrap.xmlData.processing != null)
            {
                proc.Lineage = processing.metadataWrap.xmlData.processing.name;
                if (processing.metadataWrap.xmlData.processing.Items != null)
                {
                    if (processing.metadataWrap.xmlData.processing.Items.FirstOrDefault(i => i is OpenSearch.Sentinel.Data.Safe.Sentinel10.facilityType) is OpenSearch.Sentinel.Data.Safe.Sentinel10.facilityType facility)
                    {
                        proc.Facility = facility.name;
                        if (facility.Items.FirstOrDefault(i => i is OpenSearch.Sentinel.Data.Safe.Sentinel10.softwareType) is OpenSearch.Sentinel.Data.Safe.Sentinel10.softwareType software)
                            proc.Software.Add(software.name, software.version);
                    }
                }
            }

            if (processing.metadataWrap.xmlData.processing11 != null)
            {
                proc.Lineage = processing.metadataWrap.xmlData.processing11.name;
                if (processing.metadataWrap.xmlData.processing11.Items != null)
                {
                    if (processing.metadataWrap.xmlData.processing11.Items.FirstOrDefault(i => i is OpenSearch.Sentinel.Data.Safe.Sentinel10.facilityType) is OpenSearch.Sentinel.Data.Safe.Sentinel10.facilityType facility)
                    {
                        proc.Facility = facility.name;
                        if (facility.Items.FirstOrDefault(i => i is OpenSearch.Sentinel.Data.Safe.Sentinel10.softwareType) is OpenSearch.Sentinel.Data.Safe.Sentinel10.softwareType software)
                            proc.Software.Add(software.name, software.version);
                    }
                }
            }
        }

        protected abstract string GetProcessingLevel();

        protected virtual string GetOrbitState()
        {
            metadataObjectType measurementOrbitReference = xfdu.metadataSection.First(m => m.ID == "measurementOrbitReference");
            if (measurementOrbitReference != null && measurementOrbitReference.metadataWrap.xmlData.orbitReference != null
                && measurementOrbitReference.metadataWrap.xmlData.orbitReference.extension != null)
                return measurementOrbitReference.metadataWrap.xmlData.orbitReference.extension.s1OrbitProperties.pass.ToString().ToLower();
            if (measurementOrbitReference != null && measurementOrbitReference.metadataWrap.xmlData.orbitReference11 != null)
                return measurementOrbitReference.metadataWrap.xmlData.orbitReference11.orbitNumber[0].groundTrackDirection.ToString().ToLower();

            return null;
        }

        protected virtual DateTime GetAscendingNodeCrossingDateTime()
        {
            metadataObjectType measurementOrbitReference = xfdu.metadataSection.First(m => m.ID == "measurementOrbitReference");
            if (measurementOrbitReference != null && measurementOrbitReference.metadataWrap.xmlData.orbitReference != null
                && measurementOrbitReference.metadataWrap.xmlData.orbitReference.extension != null)
                return DateTime.SpecifyKind(measurementOrbitReference.metadataWrap.xmlData.orbitReference.extension.s1OrbitProperties.ascendingNodeTime, DateTimeKind.Utc);
            return DateTime.MinValue;
        }

        private void FillDateTimeProperties(IDictionary<string, object> properties)
        {
            DateTime startDate = FindBeginPosition();
            DateTime endDate = FindEndPosition();

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate.Ticks == 0 ? startDate : endDate);

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            // datetime, start_datetime, end_datetime
            if (dateInterval.IsAnytime)
            {
                properties.Add("datetime", null);
            }

            if (dateInterval.IsMoment)
            {
                properties.Add("datetime", dateInterval.Start);
            }
            else
            {
                properties.Add("datetime", dateInterval.Start);
                properties.Add("start_datetime", dateInterval.Start);
                properties.Add("end_datetime", dateInterval.End);
            }

            DateTime createdDate = FindCreationDate();

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate);
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        protected virtual DateTime FindCreationDate()
        {
            var processing = xfdu.metadataSection.FirstOrDefault(ms => ms.ID == "processing");
            if (processing == null) return DateTime.MinValue;

            if (processing.metadataWrap.xmlData.processing != null && processing.metadataWrap.xmlData.processing.stopSpecified)
                return DateTime.SpecifyKind(processing.metadataWrap.xmlData.processing.stop, DateTimeKind.Utc);

            if (processing.metadataWrap.xmlData.processing11 != null && processing.metadataWrap.xmlData.processing11.stopSpecified)
                return DateTime.SpecifyKind(processing.metadataWrap.xmlData.processing11.stop, DateTimeKind.Utc);

            if (processing.metadataWrap.xmlData.processing != null && processing.metadataWrap.xmlData.processing.startSpecified)
                return DateTime.SpecifyKind(processing.metadataWrap.xmlData.processing11.start, DateTimeKind.Utc);

            if (processing.metadataWrap.xmlData.processing11 != null && processing.metadataWrap.xmlData.processing11.startSpecified)
                return DateTime.SpecifyKind(processing.metadataWrap.xmlData.processing11.start, DateTimeKind.Utc);

            return DateTime.MinValue;
        }


        public virtual DateTime FindBeginPosition()
        {
            var acquisitionPeriodMetadata = xfdu.metadataSection.First(m => m.ID == "acquisitionPeriod");

            // S1
            if (acquisitionPeriodMetadata != null && acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod != null)
                return DateTime.SpecifyKind(acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod.startTime, DateTimeKind.Utc);

            // S2 & S3
            if (acquisitionPeriodMetadata != null && acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod11 != null)
                return DateTime.SpecifyKind(acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod11.startTime, DateTimeKind.Utc);

            return DateTime.MinValue;
        }

        public virtual DateTime FindEndPosition()
        {
            var acquisitionPeriodMetadata = xfdu.metadataSection.First(m => m.ID == "acquisitionPeriod");

            // S1
            if (acquisitionPeriodMetadata != null && acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod != null)
                return DateTime.SpecifyKind(acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod.stopTime, DateTimeKind.Utc);

            // S2 & S3
            if (acquisitionPeriodMetadata != null && acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod11 != null)
                return DateTime.SpecifyKind(acquisitionPeriodMetadata.metadataWrap.xmlData.acquisitionPeriod11.stopTime, DateTimeKind.Utc);

            return DateTime.MaxValue;
        }

        private IDictionary<string, object> GetCommonMetadata()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(properties);



            return properties;
        }

        protected virtual void FillBasicsProperties(IDictionary<string, object> properties)
        {

            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1}",
                                                  GetTitle(properties),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)));
        }

        protected abstract string GetTitle(IDictionary<string, object> properties);

        protected string StylePlatform(string v)
        {
            string styled = v;
            if (v.Length == 1)
                styled = char.ToUpper(v[0]) + "";
            else
                styled = char.ToUpper(v[0]) + v.Substring(1);

            if (styled.Contains("-"))
                styled = styled.Substring(0, styled.Length - 1) + char.ToUpper(styled.Last());

            return styled;
        }

        protected virtual void FillInstrument(Dictionary<string, object> properties)
        {
            metadataObjectType platformMetadata = xfdu.metadataSection.First(m => m.ID == "platform");

            // platform & constellation
            string platformname = null;
            string platformnumber = "";
            if (platformMetadata.metadataWrap.xmlData.platform != null)
            {
                platformname = platformMetadata.metadataWrap.xmlData.platform.familyName.ToLower();
                platformnumber = platformMetadata.metadataWrap.xmlData.platform.number.ToLower();
            }
            if (platformMetadata.metadataWrap.xmlData.platform11 != null)
            {
                platformname = platformMetadata.metadataWrap.xmlData.platform11.familyName.ToLower();
                platformnumber = platformMetadata.metadataWrap.xmlData.platform11.number.ToLower();
            }
            if (!string.IsNullOrEmpty(platformname))
            {
                var numAlpha = new Regex("sentinel(?<platform>[0-9]*)(?<code>[a-z])");
                var match = numAlpha.Match((platformname + platformnumber).Replace("-", ""));

                properties.Remove("platform");
                properties.Add("platform", "sentinel" + "-" + match.Groups["platform"].Value + match.Groups["code"].Value);

                properties.Remove("constellation");
                properties.Add("constellation", "sentinel" + "-" + match.Groups["platform"].Value);

                properties.Remove("mission");
                properties.Add("mission", "sentinel" + "-" + match.Groups["platform"].Value);
            }

            // instruments
            var instrumentName = GetInstrumentName(platformMetadata);

            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }

            var sensor_type = GetSensorType(platformMetadata);

            if (!string.IsNullOrEmpty(sensor_type))
            {
                properties.Remove("sensor_type");
                properties.Add("sensor_type", sensor_type);
            }

            double gsd = GetGroundSampleDistance();
            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        protected abstract string GetSensorType(metadataObjectType platformMetadata);
        public abstract double GetGroundSampleDistance();

        public virtual string GetInstrumentName(metadataObjectType platformMetadata)
        {
            string instrumentName = "";
            if (platformMetadata.metadataWrap.xmlData.platform != null)
            {
                instrumentName = platformMetadata.metadataWrap.xmlData.platform.instrument.familyName.abbreviation.ToLower();
            }
            if (platformMetadata.metadataWrap.xmlData.platform11 != null)
            {
                instrumentName = platformMetadata.metadataWrap.xmlData.platform11.instrument.familyName.abbreviation.ToLower();
            }
            return instrumentName;
        }

        public virtual IGeometryObject GetGeometry()
        {
            var measurementFrameSet = xfdu.metadataSection.First(m => m.ID == "measurementFrameSet");

            if (measurementFrameSet.metadataWrap.xmlData.frameSet != null && measurementFrameSet.metadataWrap.xmlData.frameSet.frame[0].footPrint != null &&
                    measurementFrameSet.metadataWrap.xmlData.frameSet.frame[0].footPrint.Items1[0] is ServiceModel.Ogc.Gml311.CoordinatesType coordinates)
            {
                var polygon = new ServiceModel.Ogc.Gml321.PolygonType();
                polygon.exterior = new ServiceModel.Ogc.Gml321.AbstractRingPropertyType();
                var linearRing = new ServiceModel.Ogc.Gml321.LinearRingType();
                var posList = new List<ServiceModel.Ogc.Gml321.DirectPositionListType>();

                posList.Add(new ServiceModel.Ogc.Gml321.DirectPositionListType() { Text = coordinates.Value.Replace(",", " ") });
                if (posList[0].Text.Split(' ')[1] != posList[0].Text.Split(' ').Last())
                {
                    posList[0].Text += " " + posList[0].Text.Split(' ')[0] + " " + posList[0].Text.Split(' ')[1];
                }
                linearRing.Items = posList.ToArray();
                linearRing.ItemsElementName = new ServiceModel.Ogc.Gml321.ItemsChoiceType6[1] { ServiceModel.Ogc.Gml321.ItemsChoiceType6.posList };
                polygon.exterior.Item = linearRing;

                return polygon.ToGeometry().NormalizePolygon();
            }

            if (measurementFrameSet.metadataWrap.xmlData.frameSet11 != null && measurementFrameSet.metadataWrap.xmlData.frameSet11.footPrint != null &&
                    measurementFrameSet.metadataWrap.xmlData.frameSet11.footPrint.Items1[0] is ServiceModel.Ogc.Gml311.CoordinatesType coordinates1)
            {
                var polygon = new ServiceModel.Ogc.Gml321.PolygonType();
                polygon.exterior = new ServiceModel.Ogc.Gml321.AbstractRingPropertyType();
                var linearRing = new ServiceModel.Ogc.Gml321.LinearRingType();
                var posList = new List<ServiceModel.Ogc.Gml321.DirectPositionListType>();

                posList.Add(new ServiceModel.Ogc.Gml321.DirectPositionListType() { Text = coordinates1.Value.Replace(",", " ") });
                if (posList[0].Text.Split(' ')[1] != posList[0].Text.Split(' ').Last())
                {
                    posList[0].Text += " " + posList[0].Text.Split(' ')[0] + " " + posList[0].Text.Split(' ')[1];
                }
                linearRing.Items = posList.ToArray();
                linearRing.ItemsElementName = new ServiceModel.Ogc.Gml321.ItemsChoiceType6[1] { ServiceModel.Ogc.Gml321.ItemsChoiceType6.posList };
                polygon.exterior.Item = linearRing;

                return polygon.ToGeometry().NormalizePolygon();
            }

            return null;

        }

        public abstract string GetProductType();
    }
}
