// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GeoRssExtensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using Terradue.ServiceModel.Ogc.GeoRss.GeoRss;
using Terradue.ServiceModel.Ogc.Gml311;
using Terradue.Stars.Geometry.Gml311;

namespace Terradue.Stars.Geometry.GeoRss
{

    public static class GeoRssExtensions
    {



        public static IGeometryObject ToGeometry(this IGeoRSS georss)
        {

            if (georss is GeoRssPoint geoRssPoint)
            {
                return geoRssPoint.ToGeometry();
            }

            if (georss is GeoRssLine geoRssLine)
            {
                return geoRssLine.ToGeometry();
            }

            if (georss is GeoRssPolygon geoRssPolygon)
            {
                return geoRssPolygon.ToGeometry();
            }

            if (georss is GeoRssBox geoRssBox)
            {
                return geoRssBox.ToGeometry();
            }

            if (georss is GeoRssWhere geoRssWhere)
            {
                return geoRssWhere.ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRssWhere where)
        {

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is EnvelopeType envelopeType)
            {
                return envelopeType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is CircleByCenterPointType)
            {
                throw new NotImplementedException();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is LineStringType lineStringType)
            {
                return lineStringType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is PointType pointType)
            {
                return pointType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is PolygonType polygonType)
            {
                return polygonType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPolygonType multiPolygonType)
            {
                return multiPolygonType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiCurveType multiCurveType)
            {
                return multiCurveType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPolygonType multiPolygonType1)
            {
                return multiPolygonType1.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiSurfaceType multiSurfaceType)
            {
                return multiSurfaceType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPointType multiPointType)
            {
                return multiPointType.ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiLineStringType multiLineStringType)
            {
                return multiLineStringType.ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRssPoint georssPoint)
        {

            if (georssPoint.Item == null)
                return null;

            return new Point(new DirectPositionType() { Text = georssPoint.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRssLine georssLine)
        {

            if (georssLine.Item == null)
                return null;

            return new LineString(new DirectPositionListType() { Text = georssLine.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRssPolygon georssPolygon)
        {

            if (georssPolygon.Item == null)
                return null;

            List<LineString> lineStrings = new List<LineString>();

            LineString ls = new LineString(new DirectPositionListType() { Text = georssPolygon.Item }.ToGeometry());

            if (ls.Coordinates.Count < 4 || !ls.IsClosed())
                throw new FormatException("invalid GML representation: linearring is not a closed ring of minimum 4 positions");

            lineStrings.Add(ls);

            return new Polygon(lineStrings);

        }

        public static IGeometryObject ToGeometry(this GeoRssBox georssBox)
        {

            if (georssBox.Item == null)
                return null;

            List<IPosition> position = new List<IPosition>();
            List<LineString> polygon = new List<LineString>();
            string georssbox;

            georssbox = georssBox.Item.Trim();

            string[] pos = georssbox.Split(' ');

            if (pos.Length != 4)
                throw new FormatException("invalid GeoRSS representation: georss:box members are not 4 :" + georssbox);

            position.Add(new Position(pos[0], pos[1]));
            position.Add(new Position(pos[0], pos[3]));
            position.Add(new Position(pos[2], pos[3]));
            position.Add(new Position(pos[2], pos[1]));
            position.Add(new Position(pos[0], pos[1]));

            polygon.Add(new LineString(position));
            return new Polygon(polygon);

        }

        public static IGeoRSS ToGeoRss(this IGeometryObject geom)
        {

            if (geom is Point point)
                return point.ToGeoRssPoint();

            if (geom is LineString lineString)
                return lineString.ToGeoRssLine();

            if (geom is Polygon polygon && polygon.Coordinates.Count == 1)
                return polygon.ToGeoRssPolygon();

            if (geom is Polygon polygon1 && polygon1.Coordinates.Count > 1)
                return polygon1.ToGeoRssWhere();

            if (geom is MultiPolygon multiPolygon && multiPolygon.Coordinates.Count == 1)
                return multiPolygon.Coordinates.First().ToGeoRss();

            if (geom is MultiPolygon multiPolygon1 && multiPolygon1.Coordinates.Count > 1)
                return multiPolygon1.ToGeoRssWhere();

            if (geom is MultiPoint multiPoint && multiPoint.Coordinates.Count == 1)
                return multiPoint.Coordinates.First().ToGeoRss();

            if (geom is MultiPoint multiPoint1)
                return multiPoint1.ToGeoRssWhere();

            if (geom is MultiLineString multiLineString && multiLineString.Coordinates.Count == 1)
                return multiLineString.Coordinates.First().ToGeoRss();

            if (geom is MultiLineString multiLineString1)
                return multiLineString1.ToGeoRssWhere();

            throw new NotImplementedException();
        }

        public static GeoRssWhere ToGeoRssWhere(this IGeometryObject geom)
        {

            if (geom is Point point)
                return point.ToGeoRssWhere();

            if (geom is LineString lineString)
                return lineString.ToGeoRssWhere();

            if (geom is Polygon polygon)
                return polygon.ToGeoRssWhere();

            if (geom is MultiPolygon multiPolygon)
                return multiPolygon.ToGeoRssWhere();

            if (geom is MultiPoint multiPoint)
                return multiPoint.ToGeoRssWhere();

            if (geom is MultiLineString multiLineString)
                return multiLineString.ToGeoRssWhere();


            throw new NotImplementedException();
        }

        public static GeoRssPoint ToGeoRssPoint(this Point point)
        {

            return new GeoRssPoint() { Item = point.Coordinates.ToGmlPos().Text };
        }

        public static GeoRssLine ToGeoRssLine(this LineString lineString)
        {
            return new GeoRssLine() { Item = lineString.Coordinates.ToArray().ToGmlPosList(2).Text };
        }

        public static GeoRssPolygon ToGeoRssPolygon(this Polygon polygon)
        {

            return new GeoRssPolygon() { Item = polygon.Coordinates[0].Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRssWhere ToGeoRssWhere(this Polygon polygon)
        {

            return new GeoRssWhere() { Item = new PolygonType[] { polygon.ToGmlPolygon() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this LineString line)
        {

            return new GeoRssWhere() { Item = new LineStringType[] { line.ToGmlLineString() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this Point point)
        {

            return new GeoRssWhere() { Item = new PointType[] { point.ToGmlPoint() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this MultiPolygon mpolygon)
        {

            if (mpolygon.Coordinates.Count() > 1)
            {
                return new GeoRssWhere() { Item = new MultiPolygonType[] { mpolygon.ToGmlMultiPolygon() }, Type = "multipolygon" };
            }
            else
            {
                return new GeoRssWhere() { Item = new PolygonType[] { mpolygon.Coordinates.First().ToGmlPolygon() } };
            }
        }

        public static GeoRssWhere ToGeoRssWhere(this MultiPoint mpoint)
        {

            if (mpoint.Coordinates.Count() > 1)
            {

                return new GeoRssWhere() { Item = new MultiPointType[] { mpoint.ToGmlMultiPoint() }, Type = "multipoint" };
            }
            else
            {
                return new GeoRssWhere() { Item = new PointType[] { mpoint.Coordinates.First().ToGmlPoint() } };
            }

        }

        public static GeoRssWhere ToGeoRssWhere(this MultiLineString mlinestring)
        {

            if (mlinestring.Coordinates.Count() > 1)
            {

                return new GeoRssWhere() { Item = new MultiLineStringType[] { mlinestring.ToGmlMultiLineString() }, Type = "multilinestring" };
            }
            else
            {
                return new GeoRssWhere() { Item = new LineStringType[] { mlinestring.Coordinates.First().ToGmlLineString() } };
            }

        }
    }
}
