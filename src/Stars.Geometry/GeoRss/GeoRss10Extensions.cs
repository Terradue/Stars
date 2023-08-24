// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GeoRss10Extensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using Terradue.ServiceModel.Ogc.GeoRss.GeoRss10;
using Terradue.ServiceModel.Ogc.Gml311;
using Terradue.Stars.Geometry.Gml311;

namespace Terradue.Stars.Geometry.GeoRss
{

    public static class GeoRss10Extensions
    {



        public static IGeometryObject ToGeometry(this IGeoRSS georss)
        {

            if (georss is GeoRss10Point geoRss10Point)
            {
                return geoRss10Point.ToGeometry();
            }

            if (georss is GeoRss10Line geoRss10Line)
            {
                return geoRss10Line.ToGeometry();
            }

            if (georss is GeoRss10Polygon geoRss10Polygon)
            {
                return geoRss10Polygon.ToGeometry();
            }

            if (georss is GeoRss10Box geoRss10Box)
            {
                return geoRss10Box.ToGeometry();
            }

            if (georss is GeoRss10Where geoRss10Where)
            {
                return geoRss10Where.ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRss10Where where)
        {

            if (where.Item is EnvelopeType)
            {
                throw new NotImplementedException();
            }

            if (where.Item is CircleByCenterPointType)
            {
                throw new NotImplementedException();
            }

            if (where.Item is LineStringType lineStringType)
            {
                return lineStringType.ToGeometry();
            }

            if (where.Item is PointType pointType)
            {
                return pointType.ToGeometry();
            }

            if (where.Item is PolygonType polygonType)
            {
                return polygonType.ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRss10Point georssPoint)
        {

            if (georssPoint.Item == null)
                return null;

            return new Point(new DirectPositionType() { Text = georssPoint.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRss10Line georssLine)
        {

            if (georssLine.Item == null)
                return null;

            return new LineString(new DirectPositionListType() { Text = georssLine.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRss10Polygon georssPolygon)
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

        public static IGeometryObject ToGeometry(this GeoRss10Box georssBox)
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

        public static IGeoRSS ToGeoRss10(this IGeometryObject geom)
        {

            if (geom is Point point)
                return point.ToGeoRss10Point();

            if (geom is LineString lineString)
                return lineString.ToGeoRss10Line();

            if (geom is Polygon polygon1 && (polygon1).Coordinates.Count == 1)
                return polygon1.ToGeoRss10Polygon();

            if (geom is Polygon polygon && (polygon).Coordinates.Count > 1)
                return polygon.ToGeoRss10Where();

            if (geom is MultiPolygon multiPolygon)
                return multiPolygon.ToGeoRss10Where();

            if (geom is MultiPoint multiPoint)
                return multiPoint.ToGeoRss10Where();


            throw new NotImplementedException();
        }

        public static GeoRss10Where ToGeoRss10Where(this IGeometryObject geom)
        {

            if (geom is Point point)
                return point.ToGeoRss10Where();

            if (geom is LineString lineString)
                return lineString.ToGeoRss10Where();

            if (geom is Polygon polygon)
                return polygon.ToGeoRss10Where();

            if (geom is MultiPolygon multiPolygon)
                return multiPolygon.ToGeoRss10Where();

            if (geom is MultiPoint multiPoint)
                return multiPoint.ToGeoRss10Where();


            throw new NotImplementedException();
        }

        public static GeoRss10Point ToGeoRss10Point(this Point point)
        {

            return new GeoRss10Point() { Item = point.Coordinates.ToGmlPos().Text };
        }

        public static GeoRss10Line ToGeoRss10Line(this LineString lineString)
        {

            return new GeoRss10Line() { Item = lineString.Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRss10Polygon ToGeoRss10Polygon(this Polygon polygon)
        {

            return new GeoRss10Polygon() { Item = polygon.Coordinates[0].Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRss10Where ToGeoRss10Where(this Polygon polygon)
        {

            return new GeoRss10Where() { Item = polygon.ToGmlPolygon() };
        }

        public static GeoRss10Where ToGeoRss10Where(this LineString line)
        {

            return new GeoRss10Where() { Item = line.ToGmlLineString() };
        }

        public static GeoRss10Where ToGeoRss10Where(this Point point)
        {

            return new GeoRss10Where() { Item = point.ToGmlPoint() };
        }

        public static GeoRss10Where ToGeoRss10Where(this MultiPolygon mpolygon)
        {

            return new GeoRss10Where() { Item = mpolygon.ToGmlMultiSurface() };
        }

        public static GeoRss10Where ToGeoRss10Where(this MultiPoint mpoint)
        {

            return new GeoRss10Where() { Item = mpoint.ToGmlMultiPoint() };
        }
    }
}
