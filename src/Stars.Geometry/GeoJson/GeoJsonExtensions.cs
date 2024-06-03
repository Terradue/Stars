using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;


namespace Terradue.Stars.Geometry.GeoJson
{
    public static class GeoJsonExtensions
    {

        /// <summary>Checks and corrects the coordinates of a geometry and returns an equivalent valid geometry.</summary>
        /// <param name="geometry">The input geometry.</param>
        /// <returns>The same geometry if no change was needed, otherwise an adjusted polygon or multipolygon.</returns>
        public static IGeometryObject NormalizeGeometry(this IGeometryObject geometry)
        {
            if (geometry is Polygon)
            {
                Polygon polygon = geometry as Polygon;
                return NormalizePolygon(polygon);
            }
            else if (geometry is MultiPolygon)
            {
                MultiPolygon multiPolygon = geometry as MultiPolygon;
                List<Polygon> polygons = new List<Polygon>();
                foreach (Polygon polygon in multiPolygon.Coordinates)
                {
                    IGeometryObject newGeometry = polygon.NormalizePolygon();
                    if (newGeometry is Polygon)
                    {
                        polygons.Add(newGeometry as Polygon);
                    }
                    else if (newGeometry is MultiPolygon)
                    {
                        foreach (Polygon p in (newGeometry as MultiPolygon).Coordinates) polygons.Add(p);
                    }
                }
                return new MultiPolygon(polygons);
            }
            else if (geometry is LineString)
            {
                LineString lineString = geometry as LineString;
                return NormalizeLineString(lineString);
            }
            else if (geometry is MultiLineString)
            {
                MultiLineString multiLineString = geometry as MultiLineString;
                List<LineString> lineStrings = new List<LineString>();
                foreach (LineString lineString in multiLineString.Coordinates)
                {
                    IGeometryObject newGeometry = lineString.NormalizeLineString();
                    if (newGeometry is LineString)
                    {
                        lineStrings.Add(newGeometry as LineString);
                    }
                    else if (newGeometry is MultiLineString)
                    {
                        foreach (LineString l in (newGeometry as MultiLineString).Coordinates) lineStrings.Add(l);
                    }
                }
                return new MultiLineString(lineStrings);
            }
            return geometry;
        }


        /// <summary>Checks and corrects the coordinates of a polygon and returns a valid geometry (split, if necessary, into more than one if segments of it crosses the 180-degree meridian).</summary>
        /// <param name="polygon">The input polygon.</param>
        /// <returns>The same polygon if no change was needed, otherwise an adjusted polygon or multipolygon.</returns>
        public static IGeometryObject NormalizePolygon(this Polygon polygon)
        {
            List<RingSegment> segments = new List<RingSegment>();
            List<PositionInfo> positions = new List<PositionInfo>();

            // Find segments that cross the 180-degree meridian.
            // As a convention, this is considered to be the case if the difference
            // between the longitude valus of two adjecent points is greater than 180,
            // i.e. crossing the 180-degree meridian would be shorter than
            // crossing the prime (Greenwich) meridian

            bool split = false;
            for (int ringIndex = 0; ringIndex < polygon.Coordinates.Count; ringIndex++)
            {
                List<IPosition> ring = new List<IPosition>(polygon.Coordinates[ringIndex].Coordinates);
                if (!ring[0].Equals(ring[ring.Count - 1])) ring.Add(ring[0]);
                if (ring.Count < 4) throw new InvalidOperationException("The linear ring has not enough coordinates");

                RingSegment segment = new RingSegment(segments.Count, 0, ringIndex);
                segments.Add(segment);
                for (int curr = 0; curr < ring.Count; curr++)
                {
                    int prev = (curr + ring.Count - 1) % ring.Count;
                    if (Math.Abs(ring[curr].Longitude - ring[prev].Longitude) > 180 && Math.Abs(ring[curr].Longitude) != 180 && Math.Abs(ring[curr].Longitude) != 180)
                    {
                        if (ringIndex == 0) split = true;

                        // If the outer ring doesn't cross the 180-degree meridian,
                        // inner rings cannot cross it either
                        if (ringIndex != 0 && !split) throw new Exception("An inner ring crosses the 180-degree meridian, but its outer ring does not");

                        int dir = ring[curr].Longitude < 0 ? 1 : -1;
                        // Meaning:
                        //           ... 180|-180 ...
                        // dir = 1:  east ->|-> west (prev -> 180 -> -180 -> curr), crossing from eastern to western hemisphere (going eastward)
                        // dir = -1: east <-|<- west (curr <- 180 <- -180 <- prev), crossing from western to eastern hemisphere (going westward)

                        double crossingLatitude = Math.Round(GetCrossingLatitude(ring[curr], ring[prev]), 6);
                        positions.Add(new PositionInfo(new Position(crossingLatitude, 180 * dir), segment, true));
                        segment.Split = true;

                        // Start new segment
                        segment = new RingSegment(segments.Count, segment.Offset + 360 * dir, ringIndex);
                        segments.Add(segment);
                        segment.Split = true;
                        positions.Add(new PositionInfo(new Position(crossingLatitude, -180 * dir), segment, true));
                    }

                    positions.Add(new PositionInfo(ring[curr], segment));
                }
            }

            // Check whether ring is clockwise (sum > 0) or anti-clockwise (sum < 0)
            // The outer ring of a polygon must be anti-clockwise,
            // inner rings ("holes") clockwise.
            bool reverse = false;
            for (int ringIndex = 0; ringIndex < polygon.Coordinates.Count; ringIndex++)
            {
                int start = -1;
                int end = -1;
                double sum = 0;
                for (int curr = 0; curr < positions.Count; curr++)
                {
                    int prev = (curr + positions.Count - 1) % positions.Count;
                    while (positions[prev].Segment.RingIndex != ringIndex) prev = (prev + positions.Count - 1) % positions.Count;
                    if (positions[curr].Segment.RingIndex != ringIndex) continue;

                    if (start == -1) start = curr;
                    end = curr;

                    double x1 = positions[curr].Longitude + positions[curr].Segment.Offset;
                    double x0 = positions[prev].Longitude + positions[prev].Segment.Offset;
                    double y1 = positions[curr].Latitude;
                    double y0 = positions[prev].Latitude;

                    sum += (x1 - x0) * (y1 + y0);
                }

                int expectedSense = (ringIndex == 0 ? -1 : 1);
                if (sum * expectedSense < 0)
                {
                    reverse = true;
                    for (int i = 0; i < (end - start + 1) / 2; i++)
                    {
                        PositionInfo swap = positions[start + i];
                        positions[start + i] = positions[end - i];
                        positions[end - i] = swap;
                    }
                }

            }

            // Build the new geometry if necessary (multipolygon or reversed polygon)
            if (split)
            {
                List<List<LineString>> rawPolygons = new List<List<LineString>>();
                List<LineString> innerRings = new List<LineString>();

                // Find all unsplit inner rings (holes); note that inner rings that are split become part of an outer ring
                foreach (RingSegment s in segments)
                {
                    if (s.RingIndex != 0 && !s.Split)
                    {
                        List<Position> coords = new List<Position>();
                        foreach (PositionInfo pos in positions)
                        {
                            if (pos.Segment.RingIndex == s.RingIndex)
                            {
                                coords.Add(new Position(pos.Latitude, pos.Longitude, pos.Altitude));
                            }
                        }
                        innerRings.Add(new LineString(coords));
                    }
                }


                // Find all segments of original outer ring (each becomes a new polygon)
                RingSegment segment = null;
                while (true)
                {
                    PositionInfo startPos = null;
                    List<Position> coords = new List<Position>();
                    foreach (RingSegment s in segments)
                    {
                        if (s.IsUsed || s.RingIndex != 0) continue;
                        segment = s;
                        break;
                    }
                    if (segment == null) break;

                    int startIndex = 0;
                    while (segment != null)
                    {
                        segment.IsUsed = true;
                        PositionInfo lastPos = null;

                        for (int curr = startIndex; curr < startIndex + positions.Count; curr++)
                        {
                            PositionInfo pos = positions[curr % positions.Count];
                            if (pos.Segment != segment) continue;

                            if (startPos == null) startPos = pos;
                            Position prevPos = coords.Count == 0 ? null : coords[coords.Count - 1];
                            if (prevPos == null || pos.Longitude != prevPos.Longitude || pos.Latitude != prevPos.Latitude)
                            {
                                coords.Add(new Position(pos.Latitude, pos.Longitude, pos.Altitude));
                            }
                            lastPos = pos;
                        }
                        if (lastPos.Split)
                        {
                            // Look for the next split point on 180-degree meridian, either north or south depending on ring (inner/outer) and hemisphere
                            // dir = 1: look north (at 180)
                            // dir = -1: look south (at -180)
                            int dir = (lastPos.Longitude > 0 ? 1 : -1);
                            RingSegment nextSegment = null;
                            double minDist = 180;
                            for (int curr = 0; curr < positions.Count; curr++)
                            {
                                PositionInfo p = positions[curr % positions.Count];
                                if (p.Split && p.Longitude * dir > 0 && p.Segment != segment && !p.Segment.IsUsed)
                                {
                                    double dist = (p.Latitude - lastPos.Latitude) * dir;
                                    if (dist > 0 && dist < minDist)
                                    {
                                        minDist = dist;
                                        nextSegment = p.Segment;
                                        startIndex = curr;
                                    }
                                }
                            }
                            segment = nextSegment;
                        }
                        else
                        {
                            RingSegment nextSegment = null;
                            foreach (PositionInfo p in positions)
                            {
                                if (p.Longitude == lastPos.Longitude && p.Latitude == lastPos.Latitude && !p.Segment.IsUsed)
                                {
                                    nextSegment = p.Segment;
                                }
                            }
                            segment = nextSegment;
                        }
                    }

                    // Close polygon
                    if (segment == null && startPos.Split) coords.Add(coords[0]);
                    rawPolygons.Add(new List<LineString>() { new LineString(coords) });
                }

                // Add inner rings to appropriate outer ring
                foreach (LineString innerRing in innerRings)
                {
                    foreach (List<LineString> rawPolygon in rawPolygons)
                    {
                        if (IsWithin(innerRing, rawPolygon[0]))
                        {
                            rawPolygon.Add(innerRing);
                        }
                    }
                }


                // Build multipolygon
                List<Polygon> polygons = new List<Polygon>();
                foreach (List<LineString> p in rawPolygons)
                {
                    polygons.Add(new Polygon(p));
                }

                MultiPolygon multiPolygon = new MultiPolygon(polygons);
                return multiPolygon;

            }
            else if (reverse)
            {
                List<LineString> lineStrings = new List<LineString>();

                for (int ringIndex = 0; ringIndex < polygon.Coordinates.Count; ringIndex++)
                {
                    List<Position> coords = new List<Position>();
                    foreach (PositionInfo pos in positions)
                    {
                        if (pos.Segment.RingIndex == ringIndex)
                        {
                            coords.Add(new Position(pos.Latitude, pos.Longitude, pos.Altitude));
                        }
                    }
                    lineStrings.Add(new LineString(coords));

                }
                return new Polygon(lineStrings);
            }

            return polygon;
        }


        /// <summary>Checks and corrects the coordinates of a linestring and returns a valid geometry (split, if necessary, into more than one if segments of it crosses the 180-degree meridian).</summary>
        /// <param name="lineString">The input linestring.</param>
        /// <returns>The same linestring if no change was needed, otherwise an adjusted linestring or multilinestring.</returns>
        public static IGeometryObject NormalizeLineString(this LineString lineString)
        {
            List<PositionInfo> positions = new List<PositionInfo>();

            // Find segments that cross the 180-degree meridian.
            // As a convention, this is considered to be the case if the difference
            // between the two longitudes is greater than 180,
            // i.e. crossing the 180-degree meridian would be shorter than
            // crossing the prime (Greenwich) meridian

            bool split = false;
            int segmentIndex = 0;
            List<IPosition> coordinates = new List<IPosition>(lineString.Coordinates);
            for (int curr = 1; curr < coordinates.Count; curr++)
            {
                int prev = curr - 1;
                if (Math.Abs(coordinates[curr].Longitude - coordinates[prev].Longitude) > 180 && Math.Abs(coordinates[curr].Longitude) != 180 && Math.Abs(coordinates[curr].Longitude) != 180)
                {
                    split = true;

                    int dir = coordinates[curr].Longitude < 0 ? 1 : -1;
                    // Meaning:
                    //           ... 180|-180 ...
                    // dir = 1:  east ->|-> west (prev -> 180 -> -180 -> curr), crossing from eastern to western hemisphere (going eastward)
                    // dir = -1: east <-|<- west (curr <- 180 <- -180 <- prev), crossing from western to eastern hemisphere (going westward)

                    double crossingLatitude = Math.Round(GetCrossingLatitude(coordinates[curr], coordinates[prev]), 6);
                    positions.Add(new PositionInfo(new Position(crossingLatitude, 180 * dir), segmentIndex));

                    // Start new segment
                    segmentIndex++;
                    positions.Add(new PositionInfo(new Position(crossingLatitude, -180 * dir), segmentIndex));
                }

                positions.Add(new PositionInfo(coordinates[curr], segmentIndex));
            }

            if (split)
            {
                List<LineString> lineStrings = new List<LineString>();

                int segmentCount = segmentIndex + 1;

                for (segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
                {
                    List<Position> coords = new List<Position>();
                    foreach (PositionInfo pos in positions)
                    {
                        if (pos.SegmentIndex == segmentIndex)
                        {
                            coords.Add(new Position(pos.Latitude, pos.Longitude, pos.Altitude));
                        }
                    }
                    lineStrings.Add(new LineString(coords));
                }
                return new MultiLineString(lineStrings);
            }

            return lineString;
        }



        public static bool IsWithin(LineString inner, LineString outer, bool fullCheck = false)
        {
            IPosition point = inner.Coordinates[0];
            List<IPosition> outerCoordinates = new List<IPosition>(outer.Coordinates);
            int north = 0;
            int south = 0;
            IPosition start = outerCoordinates[outerCoordinates.Count - 1];
            IPosition prev = start;
            foreach (IPosition pos in outerCoordinates)
            {
                if (pos.Longitude != point.Longitude)
                {
                    if ((pos.Longitude - point.Longitude) * (start.Longitude - point.Longitude) < 0)
                    {
                        double crossingLatitude = GetCrossingLatitude(prev, pos, point.Longitude);
                        if (crossingLatitude > point.Latitude) north++;
                        else if (crossingLatitude < point.Latitude) south++;
                    }
                    start = pos;
                }
                prev = pos;
            }

            return (north % 2 == 1 && south % 2 == 1);

        }



        /// <summary>Gets the latitude at which the geometry crosses the 180-degree meridian.</summary>
        /// <param name="pos1">Point on one side of the 180-degree meridian.</param>
        /// <param name="pos2">Point on the other side of the 180-degree meridian.</param>
        /// <returns>The latitude where the geometry crosses.</returns>
        public static double GetCrossingLatitude(IPosition pos1, IPosition pos2)
        {
            double lon1 = pos1.Longitude < 0 ? pos1.Longitude + 360 : pos1.Longitude;
            double lat1 = pos1.Latitude;
            double lon2 = pos2.Longitude < 0 ? pos2.Longitude + 360 : pos2.Longitude;
            double lat2 = pos2.Latitude;

            return (lat2 - lat1) * (180 - lon1) / (lon2 - lon1) + lat1;
        }



        /// <summary>Gets the longitude at which the geometry crosses the given parallel.</summary>
        /// <param name="pos1">Point on one side of the parallel.</param>
        /// <param name="pos2">Point on the other side of the parallel.</param>
        /// <param name="latitude">The latitude of the parallel.</param>
        /// <returns>The latitude where the geometry crosses.</returns>
        public static double GetCrossingLatitude(IPosition pos1, IPosition pos2, double longitude)
        {
            double lon1 = pos1.Longitude - longitude;
            double lat1 = pos1.Latitude;
            double lon2 = pos2.Longitude - longitude;
            double lat2 = pos2.Latitude;

            return (lat1 - lat2) * lon1 / (lon2 - lon1) + lat1;
        }



    }



    /// <summary>A segment of a linear ring of that does not cross the 180-degree meridian.</summary>
    public class RingSegment
    {

        /// <summary>Gets or sets the index of this segment in a list.</summary>
        public int Index { get; set; }

        /// <summary>Gets or sets the theoretical offset of this segment (multiples of 360).</summary>
        public int Offset { get; set; }

        /// <summary>Gets or sets the index of this segment within the original polygon.</summary>
        public int RingIndex { get; set; }

        /// <summary>Gets or sets whether this segment is part of a split ring.</summary>
        public bool Split { get; set; }

        /// <summary>Gets or sets whether the segment has been reused for the normalised geometry.</summary>
        public bool IsUsed { get; set; }

        public RingSegment(int index, int offset, int ringIndex)
        {
            Index = index;
            Offset = offset;
            RingIndex = ringIndex;
        }
    }


    public class PositionInfo : IPosition
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double? Altitude { get; set; }
        public RingSegment Segment { get; set; }
        public int SegmentIndex { get; set; }
        public bool Split { get; set; }

        public PositionInfo(IPosition source, RingSegment segment, bool split = false)
        {
            Longitude = source.Longitude;
            Latitude = source.Latitude;
            Altitude = source.Altitude;
            Segment = segment;
            Split = split;
        }

        public PositionInfo(IPosition source, int segmentIndex, bool split = false)
        {
            Longitude = source.Longitude;
            Latitude = source.Latitude;
            Altitude = source.Altitude;
            SegmentIndex = segmentIndex;
            Split = split;
        }
    }

}
