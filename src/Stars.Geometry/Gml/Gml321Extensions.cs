// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Gml321Extensions.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GeoJSON.Net.Geometry;
using Terradue.ServiceModel.Ogc.Gml321;

namespace Terradue.Stars.Geometry.Gml321
{
    public static class Gml321Extensions
    {

        private static readonly string conversionSpecifier = "G";
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");


        public static MultiSurfaceType ToGmlMultiSurface(this IGeometryObject geometry)
        {

            if (geometry is Polygon)
            {
                List<Polygon> polygons = new List<Polygon>();
                polygons.Add((Polygon)geometry);
                MultiPolygon multiPolygon = new MultiPolygon(polygons);

                return ToGmlMultiSurface((MultiPolygon)multiPolygon);
            }
            else if (geometry is MultiPolygon)
            {
                return ToGmlMultiSurface((MultiPolygon)geometry);
            }
            else
                return null;

        }

        public static MultiCurveType ToGmlMultiCurve(this IGeometryObject geometry)
        {

            if (geometry is LineString)
            {
                List<LineString> lineStrings = new List<LineString>();
                lineStrings.Add((LineString)geometry);
                MultiLineString multiLineString = new MultiLineString(lineStrings);

                return ToGmlMultiCurve((MultiLineString)multiLineString);
            }
            else if (geometry is MultiLineString)
            {
                return ToGmlMultiCurve((MultiLineString)geometry);
            }
            else
                return null;
        }

        public static AbstractGeometryType ToGml(this IGeometryObject geometry)
        {

            if (geometry is Point)
            {
                return ToGmlPoint((Point)geometry);
            }

            if (geometry is MultiPoint)
            {
                return ToGmlMultiPoint((MultiPoint)geometry);
            }

            if (geometry is LineString)
            {
                return ToGmlLineString((LineString)geometry);
            }

            if (geometry is MultiLineString)
            {
                return ToGmlMultiCurve((MultiLineString)geometry);
            }

            if (geometry is Polygon)
            {
                return ToGmlPolygon((Polygon)geometry);
            }

            if (geometry is MultiPolygon)
            {
                return ToGmlMultiSurface((MultiPolygon)geometry);
            }

            return null;
        }

        public static PointType ToGmlPoint(this Point point)
        {
            PointType gmlPoint = new PointType();
            var gmlPos = ToGmlPos(point.Coordinates);
            gmlPoint.srsDimension = gmlPos.srsDimension == "2" ? null : gmlPos.srsDimension;
            gmlPoint.Item = gmlPos;
            return gmlPoint;
        }

        public static MultiPointType ToGmlMultiPoint(this MultiPoint multiPoint)
        {

            MultiPointType gmlMultiPoint = new MultiPointType();
            List<PointPropertyType> gmlPointMembers = new List<PointPropertyType>();
            foreach (var point in multiPoint.Coordinates)
            {
                PointPropertyType gmlPointMember = new PointPropertyType();
                gmlPointMember.Point = ToGmlPoint(point);
                gmlPointMembers.Add(gmlPointMember);
            }
            gmlMultiPoint.pointMember = gmlPointMembers.ToArray();
            return gmlMultiPoint;
        }

        public static DirectPositionType ToGmlPos(this IPosition position)
        {

            if (position is Position)
            {
                DirectPositionType gmlPos = new DirectPositionType();
                gmlPos.srsDimension = ((Position)position).Altitude == null ? null : "3";
                Position p = (Position)position;
                if (p.Altitude != null)
                    gmlPos.Text = string.Format("{0} {1} {2}", p.Latitude, p.Longitude, p.Altitude);
                else
                    gmlPos.Text = string.Format("{0} {1}", p.Latitude, p.Longitude);
                return gmlPos;
            }
            return null;
        }

        public static DirectPositionListType ToGmlPosList(this IPosition[] positions)
        {

            if (positions.Length > 0 && positions[0] is Position)
            {
                DirectPositionListType gmlPosList = new DirectPositionListType();
                gmlPosList.count = positions.Length.ToString();
                gmlPosList.Text = string.Join(" ", positions.Cast<Position>()
                                              .SelectMany(p => p.Altitude == null ? new string[2] {
                    p.Latitude.ToString(),
                    p.Longitude.ToString()
                } : new string[3] {
                    p.Latitude.ToString(),
                    p.Longitude.ToString(),
                    p.Altitude.ToString()
                }).ToArray());
                gmlPosList.srsDimension = ((Position)positions[0]).Altitude == null ? null : "3";
                return gmlPosList;
            }
            return null;
        }

        public static LineStringType ToGmlLineString(this LineString lineString)
        {

            LineStringType gmlLineString = new LineStringType();
            gmlLineString.ItemsElementName = new ItemsChoiceType[1];
            gmlLineString.ItemsElementName[0] = ItemsChoiceType.posList;
            gmlLineString.Items = new object[1];
            gmlLineString.Items[0] = ToGmlPosList(lineString.Coordinates.ToArray());
            return gmlLineString;
        }

        public static LinearRingType ToGmlLinearRing(this LineString lineString)
        {

            if (!lineString.IsClosed())
                throw new FormatException("LineString geometry is not closed and cannot be transformed to GML linearRing");

            LinearRingType gmlLineString = new LinearRingType();
            gmlLineString.ItemsElementName = new ItemsChoiceType6[1];
            gmlLineString.ItemsElementName[0] = ItemsChoiceType6.posList;
            gmlLineString.Items = new object[1];
            gmlLineString.Items[0] = ToGmlPosList(lineString.Coordinates.ToArray());
            return gmlLineString;
        }

        public static MultiCurveType ToGmlMultiCurve(this MultiLineString multiLineString)
        {

            MultiCurveType gmlMultiLineString = new MultiCurveType();
            gmlMultiLineString.curveMembers = new CurveArrayPropertyType();
            List<LineStringType> gmlLineStrings = new List<LineStringType>();
            foreach (var lineString in multiLineString.Coordinates)
            {
                gmlLineStrings.Add(ToGmlLineString(lineString));
            }
            gmlMultiLineString.curveMembers.Items = gmlLineStrings.ToArray();
            return gmlMultiLineString;
        }

        public static PolygonType ToGmlPolygon(this Polygon polygon)
        {
            PolygonType gmlPolygon = new PolygonType();
            if (polygon.Coordinates.Count > 0)
            {
                gmlPolygon.exterior = new AbstractRingPropertyType();
                if (!polygon.Coordinates.Any(ls => !ls.IsClosed()))
                    polygon = ClosePolygon(polygon);
                gmlPolygon.exterior.Item = ToGmlLinearRing(polygon.Coordinates[0]);
                if (polygon.Coordinates.Count > 1)
                {
                    var interiors = new List<AbstractRingPropertyType>();
                    foreach (var lineString in polygon.Coordinates.Take(1))
                    {
                        var interior = new AbstractRingPropertyType();
                        interior.Item = ToGmlLinearRing(lineString);
                        interiors.Add(interior);
                    }
                    gmlPolygon.interior = interiors.ToArray();
                }
            }

            return gmlPolygon;
        }

        private static Polygon ClosePolygon(Polygon polygon)
        {
            return new Polygon(polygon.Coordinates.Select(ls => CloseLineString(ls)));
        }

        private static LineString CloseLineString(LineString lineString)
        {
            List<IPosition> positions = new List<IPosition>(lineString.Coordinates);
            positions.Add(positions[0]);
            return new LineString(positions);
        }

        public static MultiSurfaceType ToGmlMultiSurface(this MultiPolygon multiPolygon)
        {
            MultiSurfaceType gmlMultiSurface = new MultiSurfaceType();
            gmlMultiSurface.surfaceMembers = new SurfaceArrayPropertyType();
            gmlMultiSurface.surfaceMembers.Items = multiPolygon.Coordinates.Select(p => p.ToGmlPolygon()).ToArray();

            return gmlMultiSurface;
        }

        public static IGeometryObject ToGeometry(this AbstractGeometryType gmlObject)
        {

            if (gmlObject == null)
            {
                throw new ArgumentNullException("gmlObject");
            }

            if (gmlObject is MultiCurveType)
            {
                return ((MultiCurveType)gmlObject).ToGeometry();
            }

            if (gmlObject is MultiSurfaceType)
            {
                return ((MultiSurfaceType)gmlObject).ToGeometry();
            }

            if (gmlObject is MultiPointType)
            {
                return ((MultiPointType)gmlObject).ToGeometry();
            }

            throw new NotImplementedException(gmlObject.GetType().ToString());

        }

        public static MultiPolygon ToGeometry(this MultiSurfaceType gmlMultiSurface)
        {
            List<Polygon> polygons = new List<Polygon>();

            if (gmlMultiSurface.surfaceMember != null)
            {

                foreach (var member in gmlMultiSurface.surfaceMember)
                {

                    if (member.Item is PolygonType)
                    {
                        polygons.Add(((PolygonType)member.Item).ToGeometry());
                        continue;
                    }

                    throw new NotImplementedException();
                }
            }

            if (gmlMultiSurface.surfaceMembers != null)
            {

                foreach (var member in gmlMultiSurface.surfaceMembers.Items)
                {

                    if (member is PolygonType)
                    {
                        polygons.Add(((PolygonType)member).ToGeometry());
                        continue;
                    }

                    throw new NotImplementedException();
                }
            }

            return new MultiPolygon(polygons);
        }

        public static MultiLineString ToGeometry(this MultiCurveType gmlMultiCurve)
        {
            List<LineString> linestrings = new List<LineString>();

            if (gmlMultiCurve.curveMember != null)
            {

                foreach (var member in gmlMultiCurve.curveMember)
                {
                    if (member.Item is LineStringType)
                    {
                        linestrings.Add(((LineStringType)member.Item).ToGeometry());
                        continue;
                    }

                    throw new NotImplementedException();
                }
            }

            if (gmlMultiCurve.curveMembers != null)
            {

                foreach (var member in gmlMultiCurve.curveMembers.Items)
                {
                    if (member is LineStringType)
                    {
                        linestrings.Add(((LineStringType)member).ToGeometry());
                        continue;
                    }

                    throw new NotImplementedException();
                }
            }

            return new MultiLineString(linestrings);
        }

        public static MultiPoint ToGeometry(this MultiPointType gmlMultipoint)
        {
            List<Point> points = new List<Point>();

            if (gmlMultipoint.pointMember != null)
            {
                foreach (var member in gmlMultipoint.pointMember)
                {
                    points.Add(new Point(member.Point.ToGeometry().Coordinates));
                }
            }

            if (gmlMultipoint.pointMembers != null)
            {
                foreach (var member in gmlMultipoint.pointMembers.Point)
                {
                    points.Add(new Point(member.ToGeometry().Coordinates));
                }
            }

            return new MultiPoint(points);
        }

        public static Polygon ToGeometry(this PolygonType gmlPolygon)
        {
            List<LineString> polygon = new List<LineString>();
            LineString ls = null;

            if (gmlPolygon.exterior != null)
            {

                AbstractRingPropertyType arpt = (AbstractRingPropertyType)gmlPolygon.exterior;


                if (arpt.Item is LinearRingType)
                {
                    ls = ((LinearRingType)arpt.Item).ToGeometry();

                    if (ls.Coordinates.Count < 4 || !ls.IsClosed())
                        throw new FormatException("invalid GML representation: polygon outer is not a closed ring of minimum 4 positions");
                }
            }

            if (ls == null)
                throw new FormatException("invalid GML representation: polygon outer is empty");

            polygon.Add(ls);


            if (gmlPolygon.interior != null)
            {
                foreach (AbstractRingPropertyType arpt in gmlPolygon.interior)
                {

                    if (arpt.Item is LinearRingType)
                    {
                        ls = ((LinearRingType)arpt.Item).ToGeometry();

                        if (ls.Coordinates.Count < 4 || !ls.IsClosed())
                            throw new FormatException("invalid GML representation: polygon inner is not a closed ring of minimum 4 positions");
                    }

                    polygon.Add(ls);
                }
            }

            return new Polygon(polygon);
        }

        public static LineString ToGeometry(this LinearRingType linearRing)
        {
            List<IPosition> positions;

            Type posType = linearRing.ItemsElementName.First().GetType();

            positions = FromGMLData(linearRing.Items, Array.ConvertAll(linearRing.ItemsElementName, i => i.ToString()));

            LineString linestring = new LineString(positions);

            if (linestring.Coordinates.Count < 4 || !linestring.IsClosed())
                throw new FormatException("invalid GML representation: linearring is not a closed ring of minimum 4 positions");

            return linestring;
        }

        public static LineString ToGeometry(this LineStringType lineString)
        {
            if (lineString.Items == null)
                return null;

            List<IPosition> points = FromGMLData(lineString.Items, Array.ConvertAll(lineString.ItemsElementName, i => i.ToString()));

            if (points.Count < 2)
                throw new FormatException("invalid GML representation: LineString type must have at least 2 positions");

            return new LineString(points);
        }

        public static IPosition ToGeometry(this DirectPositionType pos)
        {
            IPosition position;

            int dim;

            var whitespaceSplitter = new Regex(@"\s+");
            string[] coord = whitespaceSplitter.Split(pos.Text.Trim());

            if (string.IsNullOrEmpty(pos.srsDimension))
                dim = 2; /* We assume that we are in 2D */
            else
            {
                dim = int.Parse(pos.srsDimension);
                if (dim < 2 || dim > 3)
                    throw new FormatException("invalid GML representation: gml:pos dimension equals " + dim);
            }

            if (dim == 2)
                position = new Position(coord[0], coord[1]);
            else
                position = new Position(coord[0], coord[1], coord[2]);
            return position;
        }

        public static List<IPosition> ToGeometry(this DirectPositionListType pos)
        {
            List<IPosition> positions = new List<IPosition>();
            int dim;

            var whitespaceSplitter = new Regex(@"\s+");
            string[] coord = whitespaceSplitter.Split(pos.Text.Trim());

            if (string.IsNullOrEmpty(pos.srsDimension))
                dim = 2; /* We assume that we are in 2D */
            else
            {
                dim = int.Parse(pos.srsDimension);
                if (dim < 2 || dim > 3)
                    throw new FormatException("invalid GML representation: gml:pos dimension equals " + dim);
            }

            for (int i = 0; i < coord.Count(); i += dim)
            {
                if (dim == 2)
                    positions.Add(new Position(coord[i + 0].ToString(), coord[i + 1]));
                if (dim == 3)
                    positions.Add(new Position(coord[i + 0], coord[i + 1], coord[i + 2]));
            }
            return positions;
        }

        public static List<IPosition> ToGeometry(this CoordinatesType coordinates)
        {

            List<IPosition> positions = new List<IPosition>();
            string gmlcoord, gmlts, gmlcs, gmldec;

            /* Retrieve separator between coordinates tuples */
            gmlts = coordinates.ts;
            if (char.TryParse(gmlts, out char ts) != true)
                ts = ' ';

            /* Retrieve separator between each coordinate */
            gmlcs = coordinates.cs;
            if (char.TryParse(gmlcs, out char cs) != true)
            {
                cs = ',';
            }

            /* Retrieve decimal separator */
            gmldec = coordinates.@decimal;
            if (char.TryParse(gmldec, out char dec) != true)
                dec = '.';

            if (cs == ts || cs == dec || ts == dec)
                throw new FormatException("invalid GML representation: gml:coordinates ambiguity in separators");

            /* We retrieve gml:coord string */
            gmlcoord = coordinates.Value.Trim().Replace("  ", " ").Replace(" ,", ",").Replace(", ", ",");
            if (string.IsNullOrEmpty(gmlcoord))
                throw new FormatException("invalid GML representation: gml:coordinates is empty");

            string[] coordinates1 = gmlcoord.Split(ts);

            foreach (string coord in coordinates1)
            {
                string[] pos = coord.Split(cs);
                try
                {

                    double x = double.Parse(pos[1]);
                    double y = double.Parse(pos[0]);
                    double? z = null;
                    if (pos.Length > 2)
                        z = double.Parse(pos[2]);

                    positions.Add(new Position(y, x, z));
                }
                catch (FormatException)
                {
                    throw new FormatException(string.Format("invalid GML coordinate representation: \"{0}\" X={1}, Y={2}", coord, pos[1], pos[0]));
                }

            }
            return positions;

        }


        public static Point ToGeometry(this PointType point)
        {

            if (point.Item is DirectPositionType)
                return new Point(((DirectPositionType)point.Item).ToGeometry());
            if (point.Item is CoordinatesType)
                return new Point(((CoordinatesType)point.Item).ToGeometry().First());

            throw new FormatException("invalid GML representation: gml:point is empty");
        }

        private static List<IPosition> FromGMLData(object[] items, string[] itemsType)
        {

            List<IPosition> positions = new List<IPosition>();

            for (int i = 0; i < items.Count(); i++)
            {

                switch (itemsType[i])
                {

                    case "pos":
                        positions.Add(((DirectPositionType)items[i]).ToGeometry());
                        break;
                    case "posList":
                        positions.AddRange(((DirectPositionListType)items[i]).ToGeometry());
                        break;
                    case "coordinates":
                        positions.AddRange(((CoordinatesType)items[i]).ToGeometry());
                        break;
                    case "pointRep":
                    case "pointProperty":
                        positions.Add(((PointPropertyType)items[i]).Point.ToGeometry().Coordinates);
                        break;
                }

            }

            return positions;

        }
    }
}
