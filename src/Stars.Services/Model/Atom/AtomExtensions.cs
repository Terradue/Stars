using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System;
using Terradue.OpenSearch.Result;
using Terradue.Metadata.EarthObservation.OpenSearch.Extensions;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Services.Model.EOP;

namespace Terradue.Stars.Services.Model.Atom
{
    public static class AtomExtension
    {
        public static string CreateIdentifier(this IOpenSearchResultCollection collection)
        {
            return collection.Id;
        }

        /// <summary>
        /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetCommonMetadata(this SyndicationItem item)
        {

            AtomItem atomItem = new AtomItem(item);

            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillBasicsProperties(atomItem, properties);
            FillDateTimeProperties(atomItem, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(atomItem, properties);

            return properties;
        }

        /// <summary>
        /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#basics
        /// </summary>
        /// <param name="item"></param>
        /// <param name="properties"></param>
        private static void FillBasicsProperties(AtomItem item, Dictionary<string, object> properties)
        {
            // title
            if (item.Title != null)
            {
                properties.Remove("title");
                properties.Add("title", item.Title.Text);
            }

            // description
            if (item.Summary != null)
            {
                properties.Remove("description");
                var summary = item.Summary.Text;
                if (item.Summary.Type.ToLower().Contains("html"))
                {
                    var converter = new Html2Markdown.Converter();
                    summary = converter.Convert(item.Summary.Text);
                }
                properties.Add("description", summary);
            }
        }

        /// <summary>
        /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#date-and-time
        /// </summary>
        /// <param name="item"></param>
        /// <param name="properties"></param>
        private static void FillDateTimeProperties(AtomItem item, Dictionary<string, object> properties)
        {
            DateTime startDate = item.FindStartDate();
            DateTime endDate = item.FindEndDate();

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate);

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            // datetime, start_datetime, end_datetime
            if (dateInterval.IsAnytime)
            {
                properties.Add("datetime", null);
            }
            else
            {
                if (dateInterval.IsMoment)
                {
                    properties.Add("datetime", dateInterval.Start);
                }
                else
                {
                    properties.Add("datetime", dateInterval.Start);
                    properties.Add("start_datetime", dateInterval.Start);
                    properties.Add("end_datetime", dateInterval.Start);
                }
            }

            if (item.PublishDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", item.PublishDate.DateTime);
            }

            if (item.LastUpdatedTime.Ticks != 0)
            {
                properties.Remove("updated");
                properties.Add("updated", item.LastUpdatedTime.DateTime);
            }

        }

        /// <summary>
        /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#instrument
        /// </summary>
        /// <param name="item"></param>
        /// <param name="properties"></param>
        private static void FillInstrument(AtomItem item, Dictionary<string, object> properties)
        {
            // platform & constellation
            var platformname = item.FindPlatformShortName();
            if (!string.IsNullOrEmpty(platformname))
            {
                platformname = platformname.Replace("S1", "sentinel-1").Replace("S2", "sentinel-2").Replace("S3", "sentinel-3").ToLower();
                properties.Remove("platform");
                properties.Add("platform", platformname);

                var constellationName = platformname.TrimEnd('a').TrimEnd('b');
                properties.Remove("constellation");
                properties.Add("constellation", constellationName);

                properties.Remove("mission");
                properties.Add("mission", constellationName);

            }

            // sensor type
            var sensorType = item.FindSensorType();
            if (!string.IsNullOrEmpty(sensorType))
            {
                properties.Remove("sensor_type");
                properties.Add("sensor_type", sensorType);
            }

            // instruments
            var instrumentName = item.FindInstrumentShortName();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                if (platformname.StartsWith("sentinel-1") && instrumentName == "SAR") instrumentName = "c-sar";
                instrumentName = instrumentName.ToLower();
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }

            // gsd
            var gsd = item.FindSensorResolution();
            if (gsd > 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        public static string FindSensorType(this AtomItem item)
        {
            try
            {
                var eo = item.GetEarthObservationProfile();
                return eo.procedure.Eop21EarthObservationEquipment.sensor.Sensor.sensorType.ToLower();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IGeometryObject FindGeometry(this AtomItem item)
        {

            IGeometryObject savegeom = null;

            savegeom = Terradue.Stars.Geometry.Atom.AtomExtensions.FindGeometry(item);

            if (savegeom == null)
            {
                var eop = item.GetEarthObservationProfile();

                if (eop != null)
                    savegeom = EoProfileExtensions.FindGeometry(eop);
            }

            return savegeom;
        }

        public static AtomFeed ToAtomFeed(this AtomItem atomItem)
        {
            AtomFeed feed = new AtomFeed();

            List<AtomItem> items = new List<AtomItem>();
            items.Add(atomItem);

            feed.Items = items.ToArray();

            return feed;
        }

    }
}
