// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: EoProfileExtensions.cs

using System;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Geometry.Gml321;

namespace Terradue.Stars.Services.Model.EOP
{
    public static class EoProfileExtensions
    {

        /// <summary>
        /// Finds the geometry.
        /// </summary>
        /// <returns>The geometry.</returns>
        /// <param name="earthObservation">Earth observation.</param>
        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Om20.OM_ObservationType earthObservation)
        {


            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Alt21.AltEarthObservationType type)
            {
                return type.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Ssp21.SspEarthObservationType type2)
            {

                return type2.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Lmb21.LmbEarthObservationType type3)
            {

                return type3.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Eop21.EarthObservationType type4)
            {

                return type4.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Alt20.AltEarthObservationType type5)
            {

                return type5.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Eop20.EarthObservationType type6)
            {

                return type6.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Ssp20.SspEarthObservationType type1)
            {

                return type1.FindGeometry();
            }

            if (earthObservation != null && earthObservation is ServiceModel.Ogc.Lmb20.LmbEarthObservationType type7)
            {

                return type7.FindGeometry();
            }

            return null;
        }

        /// <summary>
        /// Finds the geometry.
        /// </summary>
        /// <returns>The geometry.</returns>
        /// <param name="eo">Eo.</param>
        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Eop21.EarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Eop21Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Eop21Footprint.multiExtentOf.MultiSurface.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Alt21.AltEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Alt21Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Alt21Footprint.nominalTrack.MultiCurve.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Ssp21.SspEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Ssp21Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Ssp21Footprint.nominalTrack.MultiCurve.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Lmb21.LmbEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Lmb21Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Lmb21Footprint.occultationPoints.MultiPoint.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Eop20.EarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Eop20Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Eop20Footprint.multiExtentOf.MultiSurface.ToGeometry();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Alt20.AltEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Alt20Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Alt20Footprint.nominalTrack.MultiCurve.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Ssp20.SspEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Ssp20Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Ssp20Footprint.multiExtentOf.MultiSurface.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

        public static IGeometryObject FindGeometry(this ServiceModel.Ogc.Lmb20.LmbEarthObservationType eo)
        {

            if (eo.featureOfInterest != null)
            {

                if (eo.featureOfInterest.Lmb20Footprint != null)
                {
                    try
                    {
                        return eo.featureOfInterest.Lmb20Footprint.occultationPoints.MultiPoint.ToGeometry();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            return null;
        }

    }
}

