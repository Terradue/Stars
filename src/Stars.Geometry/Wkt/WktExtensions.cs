// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: WktExtensions.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Stac.Collection;

namespace Terradue.Stars.Geometry.Wkt
{
    public static class WktExtensions
    {
        private static readonly IFormatProvider ci = CultureInfo.InvariantCulture;

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
            if (geometry is Point point1)
            {
                var point = GeometryToWktString(point1);

                return string.Format("POINT({0})", point);
            }

            if (geometry is MultiPoint multiPoint)
            {
                var point = GeometryToWktString(multiPoint);

                return string.Format("MULTIPOINT{0}", point);
            }

            if (geometry is LineString lineString1)
            {
                var linestring = GeometryToWktString(lineString1);

                return string.Format("LINESTRING{0}", linestring);
            }

            if (geometry is Polygon polygon1)
            {
                var polygon = GeometryToWktString(polygon1);

                return string.Format("POLYGON{0}", polygon);
            }

            if (geometry is MultiPolygon multiPolygon1)
            {
                var multiPolygon = GeometryToWktString(multiPolygon1);

                return string.Format("MULTIPOLYGON{0}", multiPolygon);
            }

            if (geometry is MultiLineString multiLineString1)
            {
                var multiLineString = GeometryToWktString(multiLineString1);

                return string.Format("MULTILINESTRING{0}", multiLineString);
            }

            return null;
        }

        private static string GeometryToWktString(Point point)
        {
            return GeometryToWktString(point.Coordinates);
        }

        private static string GeometryToWktString(MultiPoint multiPoint)
        {
            return string.Format("({0})", string.Join(",", multiPoint.Coordinates.Select(GeometryToWktString)));
        }

        private static string GeometryToWktString(IPosition position)
        {

            if (position is Position position1)
                return string.Format(ci, "{0} {1}", position1.Longitude, position1.Latitude);

            return "";
        }

        private static string GeometryToWktString(LineString lineString)
        {
            return string.Format("({0})", string.Join(",", lineString.Coordinates.Select(GeometryToWktString)));
        }

        private static string GeometryToWktString(Polygon polygon)
        {
            return string.Format("({0})", string.Join(",", polygon.Coordinates.Select(GeometryToWktString)));
        }

        private static string GeometryToWktString(MultiPolygon multiPolygon)
        {
            return string.Format("({0})", string.Join(",", multiPolygon.Coordinates.Select(GeometryToWktString)));
        }

        private static string GeometryToWktString(MultiLineString multiLineString)
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
