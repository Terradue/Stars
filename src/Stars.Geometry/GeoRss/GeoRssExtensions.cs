//
//  GeoRss10Extensions.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2015 Terradue
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
using Terradue.ServiceModel.Ogc.Gml311;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using GeoJSON.Net.Geometry;
using Terradue.ServiceModel.Ogc.GeoRss.GeoRss;
using Terradue.Stars.Geometry.Gml311;

namespace Terradue.Stars.Geometry.GeoRss {
    
    public static class GeoRssExtensions {

        

        public static IGeometryObject ToGeometry(this IGeoRSS georss) {

            if (georss is GeoRssPoint) {
                return ((GeoRssPoint)georss).ToGeometry();
            }

            if (georss is GeoRssLine) {
                return ((GeoRssLine)georss).ToGeometry();
            }

            if (georss is GeoRssPolygon) {
                return ((GeoRssPolygon)georss).ToGeometry();
            }

            if (georss is GeoRssBox) {
                return ((GeoRssBox)georss).ToGeometry();
            }

            if (georss is GeoRssWhere) {
                return ((GeoRssWhere)georss).ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRssWhere where) {
           
			if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is EnvelopeType) {
                return ((EnvelopeType)where.Item[0]).ToGeometry();
            }

			if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is CircleByCenterPointType) {
                throw new NotImplementedException();
            }

			if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is LineStringType) {
                return ((LineStringType)where.Item[0]).ToGeometry();
            }

			if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is PointType) {
                return ((PointType)where.Item[0]).ToGeometry();
            }

			if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is PolygonType) {
                return ((PolygonType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPolygonType)
            {
                return ((MultiPolygonType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiCurveType) {
                return ((MultiCurveType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPolygonType)
            {
                return ((MultiCurveType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiSurfaceType)
            {
                return ((MultiSurfaceType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiPointType)
            {
                return ((MultiPointType)where.Item[0]).ToGeometry();
            }

            if (where.Item != null && where.Item.Count() > 0 && where.Item[0] is MultiLineStringType)
            {
                return ((MultiLineStringType)where.Item[0]).ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRssPoint georssPoint) {

            if (georssPoint.Item == null)
                return null;

            return new Point(new DirectPositionType(){ Text = georssPoint.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRssLine georssLine) {

            if (georssLine.Item == null)
                return null;

            return new LineString(new DirectPositionListType(){ Text = georssLine.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRssPolygon georssPolygon) {

            if (georssPolygon.Item == null)
                return null;

            List<LineString> lineStrings = new List<LineString>();

            LineString ls = new LineString(new DirectPositionListType(){ Text = georssPolygon.Item }.ToGeometry());

            if (ls.Coordinates.Count < 4 || !ls.IsClosed())
                throw new FormatException("invalid GML representation: linearring is not a closed ring of minimum 4 positions");

            lineStrings.Add(ls);

            return new Polygon(lineStrings);

        }

        public static IGeometryObject ToGeometry(this GeoRssBox georssBox) {

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

        public static IGeoRSS ToGeoRss(this IGeometryObject geom) {

            if (geom is Point)
                return ((Point)geom).ToGeoRssPoint();

            if (geom is LineString)
                return ((LineString)geom).ToGeoRssLine();

            if (geom is Polygon && ((Polygon)geom).Coordinates.Count == 1)
                return ((Polygon)geom).ToGeoRssPolygon();

            if (geom is Polygon && ((Polygon)geom).Coordinates.Count > 1)
                return ((Polygon)geom).ToGeoRssWhere();

            if (geom is MultiPolygon && ((MultiPolygon)geom).Coordinates.Count == 1)
                return ((MultiPolygon)geom).Coordinates.First().ToGeoRss();

            if (geom is MultiPolygon && ((MultiPolygon)geom).Coordinates.Count > 1)
                return ((MultiPolygon)geom).ToGeoRssWhere();

            if (geom is MultiPoint && ((MultiPoint)geom).Coordinates.Count == 1)
                return ((MultiPoint)geom).Coordinates.First().ToGeoRss();

            if (geom is MultiPoint)
                return ((MultiPoint)geom).ToGeoRssWhere();

            if (geom is MultiLineString && ((MultiLineString)geom).Coordinates.Count == 1)
                return ((MultiLineString)geom).Coordinates.First().ToGeoRss();
            
            if (geom is MultiLineString)
                return ((MultiLineString)geom).ToGeoRssWhere();

            throw new NotImplementedException();
        }

        public static GeoRssWhere ToGeoRssWhere(this IGeometryObject geom) {

            if (geom is Point)
                return ((Point)geom).ToGeoRssWhere();

            if (geom is LineString)
                return ((LineString)geom).ToGeoRssWhere();

            if (geom is Polygon)
                return ((Polygon)geom).ToGeoRssWhere();

            if (geom is MultiPolygon)
                return ((MultiPolygon)geom).ToGeoRssWhere();

            if (geom is MultiPoint)
                return ((MultiPoint)geom).ToGeoRssWhere();

            if (geom is MultiLineString)
                return ((MultiLineString)geom).ToGeoRssWhere();


            throw new NotImplementedException();
        }

        public static GeoRssPoint ToGeoRssPoint(this Point point) {

            return new GeoRssPoint(){ Item = point.Coordinates.ToGmlPos().Text };
        }

        public static Terradue.ServiceModel.Ogc.GeoRss.GeoRss.GeoRssLine ToGeoRssLine(this LineString lineString) {
            return new Terradue.ServiceModel.Ogc.GeoRss.GeoRss.GeoRssLine(){ Item = lineString.Coordinates.ToArray().ToGmlPosList(2).Text };
        }

        public static GeoRssPolygon ToGeoRssPolygon(this Polygon polygon) {

            return new GeoRssPolygon(){ Item = polygon.Coordinates[0].Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRssWhere ToGeoRssWhere(this Polygon polygon) {

			return new GeoRssWhere(){ Item = new PolygonType[] { polygon.ToGmlPolygon() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this LineString line) {

			return new GeoRssWhere() { Item = new LineStringType[] { line.ToGmlLineString() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this Point point) {

			return new GeoRssWhere(){ Item = new PointType[] { point.ToGmlPoint() } };
        }

        public static GeoRssWhere ToGeoRssWhere(this MultiPolygon mpolygon) {

            if (mpolygon.Coordinates.Count() > 1)
            {
                return new GeoRssWhere() { Item = new MultiPolygonType[] { mpolygon.ToGmlMultiPolygon() }, Type = "multipolygon" };
            }
            else {
                return new GeoRssWhere() { Item = new PolygonType[] { mpolygon.Coordinates.First().ToGmlPolygon() } };
            }
        }

        public static GeoRssWhere ToGeoRssWhere(this MultiPoint mpoint) {

            if (mpoint.Coordinates.Count() > 1)
            {

                return new GeoRssWhere() { Item = new MultiPointType[] { mpoint.ToGmlMultiPoint() }, Type = "multipoint" };
            }
            else {
                return new GeoRssWhere() { Item = new PointType[] { mpoint.Coordinates.First().ToGmlPoint() } };
            }

        }

        public static GeoRssWhere ToGeoRssWhere(this MultiLineString mlinestring)
        {

            if (mlinestring.Coordinates.Count() > 1)
            {

                return new GeoRssWhere() { Item = new MultiLineStringType[] { mlinestring.ToGmlMultiLineString() }, Type = "multilinestring" };
            }
            else {
                return new GeoRssWhere() { Item = new LineStringType[] { mlinestring.Coordinates.First().ToGmlLineString() } };
            }

        }
    }
}

