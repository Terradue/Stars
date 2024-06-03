// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: NTSExtensions.cs

using System;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Stars.Geometry.NTS
{
    public static class NTSExtensions
    {
        public static NetTopologySuite.Geometries.Geometry ToNTSGeometry(this GeoJSON.Net.Geometry.IGeometryObject geometryObject)
        {
            switch (geometryObject.Type)
            {
                case GeoJSON.Net.GeoJSONObjectType.Point:
                    return ((GeoJSON.Net.Geometry.Point)geometryObject).ToNTSPoint();
                case GeoJSON.Net.GeoJSONObjectType.MultiPoint:
                    return ((GeoJSON.Net.Geometry.MultiPoint)geometryObject).ToNTSMultiPoint();
                case GeoJSON.Net.GeoJSONObjectType.LineString:
                    return ((GeoJSON.Net.Geometry.LineString)geometryObject).ToNTSLineString();
                case GeoJSON.Net.GeoJSONObjectType.MultiLineString:
                    return ((GeoJSON.Net.Geometry.MultiLineString)geometryObject).ToNTSMultiLineString();
                case GeoJSON.Net.GeoJSONObjectType.Polygon:
                    return ((GeoJSON.Net.Geometry.Polygon)geometryObject).ToNTSPolygon();
                case GeoJSON.Net.GeoJSONObjectType.MultiPolygon:
                    return ((GeoJSON.Net.Geometry.MultiPolygon)geometryObject).ToNTSMultiPolygon();
                case GeoJSON.Net.GeoJSONObjectType.GeometryCollection:
                    return ((GeoJSON.Net.Geometry.GeometryCollection)geometryObject).ToNTSGeometryCollection();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Point ToNTSPoint(this GeoJSON.Net.Geometry.Point geometryPoint)
        {
            return new Point(geometryPoint.Coordinates.Longitude, geometryPoint.Coordinates.Latitude, geometryPoint.Coordinates.Altitude ?? 0);
        }

        public static Coordinate ToNTSCoordinate(this GeoJSON.Net.Geometry.IPosition geometryPosition)
        {
            var coordinate = new Coordinate(geometryPosition.Longitude, geometryPosition.Latitude);
            if (geometryPosition.Altitude.HasValue)
                coordinate.Z = geometryPosition.Altitude.Value;
            return coordinate;
        }

        public static MultiPoint ToNTSMultiPoint(this GeoJSON.Net.Geometry.MultiPoint geometryMultiPoint)
        {
            return new MultiPoint(geometryMultiPoint.Coordinates.Select(c => c.ToNTSPoint()).ToArray());
        }

        public static LineString ToNTSLineString(this GeoJSON.Net.Geometry.LineString geometryLineString)
        {
            return new LineString(geometryLineString.Coordinates.Select(c => c.ToNTSCoordinate()).ToArray());
        }

        public static MultiLineString ToNTSMultiLineString(this GeoJSON.Net.Geometry.MultiLineString geometryMultiLineString)
        {
            return new MultiLineString(geometryMultiLineString.Coordinates.Select(c => c.ToNTSLineString()).ToArray());
        }

        public static LinearRing ToNTSLinearRing(this GeoJSON.Net.Geometry.LineString geometryLineString)
        {
            return new LinearRing(geometryLineString.Coordinates.Select(c => c.ToNTSCoordinate()).ToArray());
        }

        public static Polygon ToNTSPolygon(this GeoJSON.Net.Geometry.Polygon geometryPolygon)
        {
            return new Polygon(geometryPolygon.Coordinates.First().ToNTSLinearRing(), geometryPolygon.Coordinates.Skip(1).Select(c => c.ToNTSLinearRing()).ToArray());
        }

        public static MultiPolygon ToNTSMultiPolygon(this GeoJSON.Net.Geometry.MultiPolygon geometryMultiPolygon)
        {
            return new MultiPolygon(geometryMultiPolygon.Coordinates.Select(c => c.ToNTSPolygon()).ToArray());
        }

        public static GeometryCollection ToNTSGeometryCollection(this GeoJSON.Net.Geometry.GeometryCollection geometryCollection)
        {
            return new GeometryCollection(geometryCollection.Geometries.Select(g => g.ToNTSGeometry()).ToArray());
        }
    }
}
