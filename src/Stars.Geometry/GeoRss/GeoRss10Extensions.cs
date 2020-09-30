//
//  GeoRss1010Extensions.cs
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
using GeoJSON.Net.Geometry;
using Terradue.ServiceModel.Ogc.GeoRss.GeoRss10;
using Terradue.Stars.Geometry.Gml311;
using System.Linq;

namespace Terradue.Stars.Geometry.GeoRss {

    public static class GeoRss10Extensions {

     

        public static IGeometryObject ToGeometry(this IGeoRSS georss) {

            if (georss is GeoRss10Point) {
                return ((GeoRss10Point)georss).ToGeometry();
            }

            if (georss is GeoRss10Line) {
                return ((GeoRss10Line)georss).ToGeometry();
            }

            if (georss is GeoRss10Polygon) {
                return ((GeoRss10Polygon)georss).ToGeometry();
            }

            if (georss is GeoRss10Box) {
                return ((GeoRss10Box)georss).ToGeometry();
            }

            if (georss is GeoRss10Where) {
                return ((GeoRss10Where)georss).ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRss10Where where) {

            if (where.Item is EnvelopeType) {
                throw new NotImplementedException();
            }

            if (where.Item is CircleByCenterPointType) {
                throw new NotImplementedException();
            }

            if (where.Item is LineStringType) {
                return ((LineStringType)where.Item).ToGeometry();
            }

            if (where.Item is PointType) {
                return ((PointType)where.Item).ToGeometry();
            }

            if (where.Item is PolygonType) {
                return ((PolygonType)where.Item).ToGeometry();
            }

            return null;

        }

        public static IGeometryObject ToGeometry(this GeoRss10Point georssPoint) {

            if (georssPoint.Item == null)
                return null;

            return new Point(new DirectPositionType(){ Text = georssPoint.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRss10Line georssLine) {

            if (georssLine.Item == null)
                return null;

            return new LineString(new DirectPositionListType(){ Text = georssLine.Item }.ToGeometry());

        }

        public static IGeometryObject ToGeometry(this GeoRss10Polygon georssPolygon) {

            if (georssPolygon.Item == null)
                return null;

            List<LineString> lineStrings = new List<LineString>();

            LineString ls = new LineString(new DirectPositionListType(){ Text = georssPolygon.Item }.ToGeometry());

            if (ls.Coordinates.Count < 4 || !ls.IsClosed())
                throw new FormatException("invalid GML representation: linearring is not a closed ring of minimum 4 positions");

            lineStrings.Add(ls);

            return new Polygon(lineStrings);

        }

        public static IGeometryObject ToGeometry(this GeoRss10Box georssBox) {

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

        public static IGeoRSS ToGeoRss10(this IGeometryObject geom) {

            if (geom is Point)
                return ((Point)geom).ToGeoRss10Point();

            if (geom is LineString)
                return ((LineString)geom).ToGeoRss10Line();

            if (geom is Polygon && ((Polygon)geom).Coordinates.Count == 1)
                return ((Polygon)geom).ToGeoRss10Polygon();

            if (geom is Polygon && ((Polygon)geom).Coordinates.Count > 1)
                return ((Polygon)geom).ToGeoRss10Where();

            if (geom is MultiPolygon)
                return ((MultiPolygon)geom).ToGeoRss10Where();

            if (geom is MultiPoint)
                return ((MultiPoint)geom).ToGeoRss10Where();


            throw new NotImplementedException();
        }

        public static GeoRss10Where ToGeoRss10Where(this IGeometryObject geom) {

            if (geom is Point)
                return ((Point)geom).ToGeoRss10Where();

            if (geom is LineString)
                return ((LineString)geom).ToGeoRss10Where();

            if (geom is Polygon)
                return ((Polygon)geom).ToGeoRss10Where();

            if (geom is MultiPolygon)
                return ((MultiPolygon)geom).ToGeoRss10Where();

            if (geom is MultiPoint)
                return ((MultiPoint)geom).ToGeoRss10Where();


            throw new NotImplementedException();
        }

        public static GeoRss10Point ToGeoRss10Point(this Point point) {

            return new GeoRss10Point(){ Item = point.Coordinates.ToGmlPos().Text };
        }

        public static GeoRss10Line ToGeoRss10Line(this LineString lineString) {

            return new GeoRss10Line(){ Item = lineString.Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRss10Polygon ToGeoRss10Polygon(this Polygon polygon) {

            return new GeoRss10Polygon(){ Item = polygon.Coordinates[0].Coordinates.ToArray().ToGmlPosList().Text };
        }

        public static GeoRss10Where ToGeoRss10Where(this Polygon polygon) {

            return new GeoRss10Where(){ Item = polygon.ToGmlPolygon() };
        }

        public static GeoRss10Where ToGeoRss10Where(this LineString line) {

            return new GeoRss10Where(){ Item = line.ToGmlLineString() };
        }

        public static GeoRss10Where ToGeoRss10Where(this Point point) {

            return new GeoRss10Where(){ Item = point.ToGmlPoint() };
        }

        public static GeoRss10Where ToGeoRss10Where(this MultiPolygon mpolygon) {

            return new GeoRss10Where(){ Item = mpolygon.ToGmlMultiSurface() };
        }

        public static GeoRss10Where ToGeoRss10Where(this MultiPoint mpoint) {

            return new GeoRss10Where(){ Item = mpoint.ToGmlMultiPoint() };
        }
    }
}

