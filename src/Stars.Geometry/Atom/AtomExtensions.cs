using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Geometry.GeoRss;
using Terradue.Stars.Geometry.Wkt;
using Terradue.ServiceModel.Ogc.GeoRss.GeoRss;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Geometry.Atom
{
    public static class AtomExtensions
    {

        public static IGeometryObject FindGeometry(this SyndicationItem item)
		{

			IGeometryObject savegeom = null;

			if (item.ElementExtensions != null && item.ElementExtensions.Count > 0)
			{

				foreach (var ext in item.ElementExtensions)
				{

					XmlReader xr = ext.GetReader();

					switch (xr.NamespaceURI)
					{
						// 1) search for georss
						case "http://www.georss.org/georss":
                            var georss = ServiceModel.Ogc.GeoRss.GeoRss.GeoRssHelper.Deserialize(xr);
                            savegeom = georss.ToGeometry();
                            if (!(georss is ServiceModel.Ogc.GeoRss.GeoRss.GeoRssBox || georss is ServiceModel.Ogc.GeoRss.GeoRss.GeoRssPoint))
                            {
								return savegeom;
							}
							break;
						// 2) search for georss10
						case "http://www.georss.org/georss/10":
                            var georss10 = ServiceModel.Ogc.GeoRss.GeoRss10.GeoRss10Helper.Deserialize(xr);
                            savegeom = georss10.ToGeometry();
                            if (!(georss10 is GeoRssBox || georss10 is GeoRssPoint))
                            {
								return savegeom;
							}
							break;
						// 3) search for dct:spatial
						case "http://purl.org/dc/terms/":
							if (xr.LocalName == "spatial")
							{
								savegeom = WktExtensions.WktToGeometry(xr.ReadElementContentAsString());
							}
							if (!(savegeom is Point))
							{
								return savegeom;
							}
							break;
						default:
							continue;
					}

				}

			}

			return savegeom;

		}
    }
}
