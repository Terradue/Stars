//
//  FeatureExtensions.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;
using Stac.Collection;

namespace Terradue.Stars.Geometry.Wkt
{
    public static class WktExtensions
    {

        static readonly IFormatProvider ci = CultureInfo.InvariantCulture;

        public static string ToWkt(this Feature feature)
        {
            return ToWkt(feature.Geometry);
        }

        public static string ToWkt(this StacSpatialExtent extent)
        {
            Polygon polygon = new Polygon(new List<LineString>() { new LineString(
                new List<Position>() {
                    new Position(extent.BoundingBoxes[0][1], extent.BoundingBoxes[0][0]),
                    new Position(extent.BoundingBoxes[0][1], extent.BoundingBoxes[0][2]),
                    new Position(extent.BoundingBoxes[0][3], extent.BoundingBoxes[0][2]),
                    new Position(extent.BoundingBoxes[0][3], extent.BoundingBoxes[0][0]),
                    new Position(extent.BoundingBoxes[0][1], extent.BoundingBoxes[0][0])
                }
            )});

            var polygonWkt = GeometryToWktString(polygon);

            return string.Format("POLYGON{0}", polygonWkt);
        }

        public static string ToWkt(this IGeometryObject geometry)
        {
            if (geometry is Point)
            {
                var point = GeometryToWktString((Point)geometry);

                return string.Format("POINT({0})", point);
            }

            if (geometry is MultiPoint)
            {
                var point = GeometryToWktString((MultiPoint)geometry);

                return string.Format("MULTIPOINT{0}", point);
            }

            if (geometry is LineString)
            {
                var linestring = GeometryToWktString((LineString)geometry);

                return string.Format("LINESTRING{0}", linestring);
            }

            if (geometry is Polygon)
            {
                var polygon = GeometryToWktString((Polygon)geometry);

                return string.Format("POLYGON{0}", polygon);
            }

            if (geometry is MultiPolygon)
            {
                var multiPolygon = GeometryToWktString((MultiPolygon)geometry);

                return string.Format("MULTIPOLYGON{0}", multiPolygon);
            }

            if (geometry is MultiLineString)
            {
                var multiLineString = GeometryToWktString((MultiLineString)geometry);

                return string.Format("MULTILINESTRING{0}", multiLineString);
            }

            return null;
        }

        static string GeometryToWktString(Point point)
        {
            return GeometryToWktString(point.Coordinates);
        }

        static string GeometryToWktString(MultiPoint multiPoint)
        {
            return string.Format("({0})", string.Join(",", multiPoint.Coordinates.Select(GeometryToWktString)));
        }

        static string GeometryToWktString(IPosition position)
        {

            if (position is Position)
                return string.Format(ci, "{0} {1}", ((Position)position).Longitude, ((Position)position).Latitude);

            return "";
        }

        static string GeometryToWktString(LineString lineString)
        {
            return string.Format("({0})", string.Join(",", lineString.Coordinates.Select(GeometryToWktString)));
        }

        static string GeometryToWktString(Polygon polygon)
        {
            return string.Format("({0})", string.Join(",", polygon.Coordinates.Select(GeometryToWktString)));
        }

        static string GeometryToWktString(MultiPolygon multiPolygon)
        {
            return string.Format("({0})", string.Join(",", multiPolygon.Coordinates.Select(GeometryToWktString)));
        }

        static string GeometryToWktString(MultiLineString multiLineString)
        {
            return string.Format("({0})", string.Join(",", multiLineString.Coordinates.Select(GeometryToWktString)));
        }

        /// <summary>
        /// Initialize a new IGemotry object from a standard WKT geometry
        /// </summary>
        /// <param name="wkt">The geometry in WKT to convert</param>
        public static IGeometryObject WktToGeometry(string wkt)
        {
            wkt = wkt.Trim().Replace(", ", ",");
            var match = Regex.Match(wkt, @"([A-Z]+)\s*[(]\s*(\(*.+\)*)\s*[)]");
            if (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "MULTIPOLYGON":
                        return MultiPolygonFromWKT(match.Groups[2].Value);
                    case "POLYGON":
                        return PolygonFromWKT(match.Groups[2].Value);
                    case "MULTILINESTRING":
                        return MultiLineStringFromWKT(match.Groups[2].Value);
                    case "LINESTRING":
                        return LineStringFromWKT(match.Groups[2].Value);
                    case "MULTIPOINT":
                        return MultiPointFromWKT(match.Groups[2].Value);
                    case "POINT":
                        return PointFromWKT(match.Groups[2].Value);
                }
            }
            throw new NotImplementedException(wkt);

        }

        /// <summary>
        /// MultiPolygon from WK.
        /// </summary>
        /// <returns>The MultiPolygon</returns>
        /// <param name="wkt">WKT.</param>
        public static MultiPolygon MultiPolygonFromWKT(string wkt)
        {
            var matches = Regex.Matches(wkt, @"([(](?:\ *[(]\ *(?:\ *(?:[0-9-.Ee]+[ ]+[0-9-.Ee]+)[, ]*\ *)*\ *[)][, ]*)*[)])");
            var polygons = new List<Polygon>(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var polygon = PolygonFromWKT(matches[i].Groups[1].Value);
                polygons.Add(polygon);
            }

            return new MultiPolygon(polygons);
        }

        /// <summary>
        /// Polygon from WKT.
        /// </summary>
        /// <returns>The Polygon</returns>
        /// <param name="wkt">WKT.</param>
        public static Polygon PolygonFromWKT(string wkt)
        {
            var matches = Regex.Matches(wkt, @"(\ *[(]\ *(?:\ *(?:[0-9-.Ee]+[ ]+[0-9-.Ee]+)[, ]*\ *)*\ *[)])[, ]*");
            var linestrings = new List<LineString>(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var linestring = LineStringFromWKT(matches[i].Groups[1].Value);
                linestrings.Add(linestring);
            }

            return new Polygon(linestrings);
        }

        /// <summary>
        /// LineString from WKT.
        /// </summary>
        /// <returns>The LineString</returns>
        /// <param name="wkt">WKT.</param>
        public static LineString LineStringFromWKT(string wkt)
        {
            var terms = wkt.TrimStart('(').TrimEnd(')').Split(',');
            string[] values;
            var positions = new List<IPosition>(terms.Length);
            Position prevgeopos = null;
            for (var i = 0; i < terms.Length; i++)
            {
                values = terms[i].Trim(' ').Split(' ');
                var z = (values.Length > 2 ? values[2] : null);
                var geopos = new Position(values[1], values[0], z);
                try
                {
                    if (prevgeopos != null && (geopos.Latitude == prevgeopos.Latitude) && (geopos.Longitude == prevgeopos.Longitude) && (geopos.Altitude == prevgeopos.Altitude))
                        continue;
                }
                catch
                {
                }
                positions.Add(geopos);
                prevgeopos = geopos;
            }

            var test = new LineString(positions);
            return test;
        }

        /// <summary>
        /// MultiLineString from WK.
        /// </summary>
        /// <returns>The MultiLineString</returns>
        /// <param name="wkt">WKT.</param>
        public static MultiLineString MultiLineStringFromWKT(string wkt)
        {
            var matches = Regex.Matches(wkt, @"(\ *[(]\ *(?:\ *(?:[0-9-.Ee]+[ ]+[0-9-.Ee]+)[, ]*\ *)*\ *[)])[, ]*");
            var linestrings = new List<LineString>(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var linestring = LineStringFromWKT(matches[i].Groups[1].Value);
                linestrings.Add(linestring);
            }

            return new MultiLineString(linestrings);
        }

        /// <summary>
        /// MultiPoint from WK.
        /// </summary>
        /// <returns>The MultiPoint</returns>
        /// <param name="wkt">WKT.</param>
        public static MultiPoint MultiPointFromWKT(string wkt)
        {
            var terms = wkt.TrimStart('(').TrimEnd(')').Trim(' ').Split(',');
            string[] values;
            var points = new List<Point>(terms.Length);
            for (var i = 0; i < terms.Length; i++)
            {
                values = terms[i].TrimStart('(').TrimEnd(')').Split(' ');
                var z = (values.Length > 2 ? values[2] : null);
                var geopos = new Point(new Position(values[1], values[0], z));
                points.Add(geopos);
            }
            return new MultiPoint(points);
        }

        /// <summary>
        /// Point from WK.
        /// </summary>
        /// <returns>The Point</returns>
        /// <param name="wkt">WKT.</param>
        public static Point PointFromWKT(string wkt)
        {
            string[] values;
            values = wkt.Trim(' ').Split(' ');
            var z = (values.Length > 2 ? values[2] : null);
            var geopos = new Position(values[1], values[0], z);
            return new Point(geopos);
        }
    }


}

