# PLANET PlanetScope - L3B PlanetScope Ortho Scene Product - PSScene4Bands

Note: L3B: Orthorectified, scaled Top of Atmosphere radiance (at sensor) image product suitable for analytic and visual applications. This product has scene based framing and projected to a cartographic projection.

Ref: Planet, Planet Imagery Product Specification: PLANETSCOPE & RAPIDEYE, October 2016.

Resources:

https://developers.planet.com/docs/data/psscene4band/

https://developers.planet.com/docs/orders/tools-reference/


|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /ps:EarthObservation/gml:metaDataProperty/ps:EarthObservationMetaData/eop:identifier | 20181001_015434_1027_3B_AnalyticMS | |
| mission | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:Platform/eop:shortName | "planetscope" | |
| platform | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:Platform/eop:shortName | "planetscope" | |
| instruments | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:instrument | "ps2" | PS2, "item_type": "PSScene4Band",|
| sat:orbit_state | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:orbitDirection | "descending" | DESCENDING |
| sat:absolute_orbit | | | "strip_id": "1736074", |
| view:sun_elevation | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:illuminationElevationAngle | 6.099140e+01 | in degrees |
| view:sun_azimuth | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:illuminationAzimuthAngle | 9.470213e+01 | in degrees |
| view:incidence_angle | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:incidenceAngle | 9.263208e-02 | in degrees |
| view:off_nadir | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/ps:spaceCraftViewAngle | 5.094407e-02 | in degrees |
| processing:level | /ps:EarthObservation/gml:metaDataProperty/ps:EarthObservationMetaData/eop:productType | L3B | |
| gsd | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:sensor/ps:Sensor/eop:resolution | 3 | in meters |
| B1:name | /ps:bandSpecificMetadata/ps:bandNumber | "band-1" | |
| B1:common_name | | "red" | hardcoded |
| B1:center_wavelength | | 0.630 | hardcoded, 590 - 670 |
| B1:radiometric_scale_factor | /ps:bandSpecificMetadata/ps:radiometricScaleFactor | 0.01 | Multiply by radiometricScaleFactor to convert DNs to TOA Radiance (watts per steradian per square metre |
| B1:reflectance_coeffcient | /ps:bandSpecificMetadata/ps:reflectanceCoefficient | 1.85080054268e-05 | Multiply by reflectanceCoefficient to convert DNs to TOA Reflectance |
| B2:name | /ps:bandSpecificMetadata/ps:bandNumber | "band-2" | |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.545 | hardcoded, 500 - 590 |
| B2:radiometric_scale_factor | /ps:bandSpecificMetadata/ps:radiometricScaleFactor | 0.01 | Multiply by radiometricScaleFactor to convert DNs to TOA Radiance (watts per steradian per square metre |
| B2:reflectance_coeffcient | /ps:bandSpecificMetadata/ps:reflectanceCoefficient | 1.96242607392e-05 | Multiply by reflectanceCoefficient to convert DNs to TOA Reflectance |
| B3:name | /ps:bandSpecificMetadata/ps:bandNumber | "band-3" | |
| B3:common_name | | "blue" | hardcoded |
| B3:center_wavelength | | 0.485 | hardcoded, 455 - 515 |
| B3:radiometric_scale_factor | /ps:bandSpecificMetadata/ps:radiometricScaleFactor | 0.01 | Multiply by radiometricScaleFactor to convert DNs to TOA Radiance (watts per steradian per square metre |
| B3:reflectance_coeffcient | /ps:bandSpecificMetadata/ps:reflectanceCoefficient | 2.1850578954e-05 | Multiply by reflectanceCoefficient to convert DNs to TOA Reflectance |
| B4:name | /ps:bandSpecificMetadata/ps:bandNumber | "band-4" | |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.82 | hardcoded, 780 - 860 |
| B4:radiometric_scale_factor | /ps:bandSpecificMetadata/ps:radiometricScaleFactor | 0.01 | Multiply by radiometricScaleFactor to convert DNs to TOA Radiance (watts per steradian per square metre |
| B4:reflectance_coeffcient | /ps:bandSpecificMetadata/ps:reflectanceCoefficient | 3.30930296327e-05 | Multiply by reflectanceCoefficient to convert DNs to TOA Reflectance |
| created | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:acquisitionDateTime | 2018-OCT-01T01:54:34Z | Format to ISO: 2018-10-01T01:54:34+00:00 to 2018-OCT-01T01:54:34Z. "updated": "2018-10-01T08:35:26Z", "published": "2018-10-01T08:23:37Z", |
| start_datetime  | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:acquisitionDateTime | 2018-OCT-01T01:54:34Z | Format to ISO: 2018-10-01T01:54:34+00:00 to 2018-OCT-01T01:54:34Z |
| end_datetime | /ps:EarthObservation/gml:using/eop:EarthObservationEquipment/eop:platform/eop:acquisitionParameters/ps:Acquisition/eop:acquisitionDateTime | 2018-OCT-01T01:54:34Z | Format to ISO: 2018-10-01T01:54:34+00:00 to 2018-OCT-01T01:54:34Z |
| proj:epsg | /prdf/image/projectionName & originLongitude & originLatitude | 32750 | UTM zone is (50S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topLeft/ps:longitude & ps:longitude  | [119.734919409, -0.85070793723]| Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:topRight/ps:longitude & ps:longitude | [119.977694309, -0.85070793723] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomRight/ps:longitude & ps:longitude | [119.977694309, -0.970300911917] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /ps:EarthObservation/gml:target/ps:Footprint/ps:geographicLocation/ps:bottomLeft/ps:longitude & ps:longitude | [119.734919409, -0.970300911917] | Coordinates Type="Geographic" Units="Degrees |