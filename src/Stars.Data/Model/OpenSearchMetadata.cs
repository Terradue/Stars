// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: OpenSearchMetadata.cs

namespace Terradue.Stars.Data.Model
{
    public static class OpenSearchMetadata
    {
        // public static string CreateIdentifier(this IOpenSearchResultCollection collection)
        // {
        //     return collection.Id;
        // }


        // /// <summary>
        // /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md
        // /// </summary>
        // /// <param name="item"></param>
        // /// <returns></returns>
        // public static IDictionary<string, object> FindCommonProperties(this IOpenSearchResultItem item)
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>();

        //     FillBasicsProperties(item, properties);
        //     FillDateTimeProperties(item, properties);
        //     // TODO Licensing
        //     // TODO Provider
        //     FillInstrument(item, properties);

        //     return properties;

        // }



        // /// <summary>
        // /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#basics
        // /// </summary>
        // /// <param name="item"></param>
        // /// <param name="properties"></param>
        // private static void FillBasicsProperties(IOpenSearchResultItem item, Dictionary<string, object> properties)
        // {
        //     // title
        //     if (item.Title != null)
        //     {
        //         properties.Remove("title");
        //         properties.Add("title", item.Title.Text);
        //     }

        //     // description
        //     if (item.Summary != null)
        //     {
        //         properties.Remove("description");
        //         var summary = item.Summary.Text;
        //         if(item.Summary.Type.Contains("html", StringComparison.InvariantCultureIgnoreCase)){
        //             var converter = new Html2Markdown.Converter();
        //             summary = converter.Convert(item.Summary.Text);
        //         }
        //         properties.Add("description", summary);
        //     }
        // }

        // /// <summary>
        // /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#date-and-time
        // /// </summary>
        // /// <param name="item"></param>
        // /// <param name="properties"></param>
        // private static void FillDateTimeProperties(IOpenSearchResultItem item, Dictionary<string, object> properties)
        // {
        //     DateTime startDate = item.FindStartDate();
        //     DateTime endDate = item.FindEndDate();

        //     Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate);

        //     // remove previous values
        //     properties.Remove("datetime");
        //     properties.Remove("start_datetime");
        //     properties.Remove("end_datetime");

        //     // datetime, start_datetime, end_datetime
        //     if (dateInterval.IsAnytime)
        //     {
        //         properties.Add("datetime", null);
        //     }

        //     if (dateInterval.IsMoment)
        //     {
        //         properties.Add("datetime", dateInterval.Start);
        //     }
        //     else
        //     {
        //         properties.Add("datetime", null);
        //         properties.Add("start_datetime", dateInterval.Start);
        //         properties.Add("end_datetime", dateInterval.Start);
        //     }

        //     if (item.PublishDate.Ticks != 0)
        //     {
        //         properties.Remove("created");
        //         properties.Add("created", item.PublishDate.DateTime);
        //     }

        //     if (item.LastUpdatedTime.Ticks != 0)
        //     {
        //         properties.Remove("updated");
        //         properties.Add("updated", item.LastUpdatedTime.DateTime);
        //     }

        // }

        // /// <summary>
        // /// https://github.com/radiantearth/stac-spec/blob/master/item-spec/common-metadata.md#instrument
        // /// </summary>
        // /// <param name="item"></param>
        // /// <param name="properties"></param>
        // private static void FillInstrument(IOpenSearchResultItem item, Dictionary<string, object> properties)
        // {
        //     // platform & constellation
        //     var platformname = item.FindPlatformShortName();
        //     if (!string.IsNullOrEmpty(platformname))
        //     {
        //         platformname = platformname.Replace("S1", "sentinel-1").Replace("S2", "sentinel-2").Replace("S3", "sentinel-3").ToLower();
        //         properties.Remove("platform");
        //         properties.Add("platform", platformname);

        //         var constellationName = platformname.TrimEnd('a').TrimEnd('b');
        //         properties.Remove("constellation");
        //         properties.Add("constellation", constellationName);

        //         properties.Remove("mission");
        //         properties.Add("mission", constellationName);

        //     }

        //     // instruments
        //     var instrumentName = item.FindInstrumentShortName();
        //     if (!string.IsNullOrEmpty(instrumentName))
        //     {
        //         if (instrumentName == "https://stac-extensions.github.io/sar/v1.0.0/schema.json") instrumentName = "c-sar";
        //         instrumentName = instrumentName.ToLower();
        //         properties.Remove("instruments");
        //         properties.Add("instruments", new string[] { instrumentName });
        //     }

        //     // TODO gsd
        // }

    }
}
