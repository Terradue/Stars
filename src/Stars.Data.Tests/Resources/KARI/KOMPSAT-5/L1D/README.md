# KOMPSAT-5 Level-1D ULTRA HIGH RESOLUTION (UH)

Ref1. KOMPSAT-5 PRODUCT SPECIFICATIONS, Standard Product Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref2. KOMPSAT-5 PRODUCT ATTRIBUTES, Product Attributes Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref3. KOMPSAT-5 Image Data Manual, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.0 April,2014.

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | Auxiliary/Root/ProductFilename  | KMPS5_GTC_B_ES_10_HH_RD_P_20190916093251_20190916093256_20190918030009 | remove the extension |
| mission | | kompsat-5 | HARDCODED mission name |
| platform | | kompsat-5 | HARDCODED mission name |
| instruments | | cosi | HARDCODED sensor name |
| sat:orbit_state | /Auxiliary/Root/OrbitDirection | ascending | |
| sat:absolute_orbit | /Auxiliary/Root/OrbitNumber | 40077 | |
| view:incidence_angle | /Auxiliary/Root/MBI/NearIncidenceAngle, /Auxiliary/Root/MBI/FarIncidenceAngle | [38.640129244624696,45.706093759532102] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from NearIncidenceAngle to FarIncidenceAngle. MBI for Multi Beam Image or SBI for Single Beam Image. The reference incidence angle of 45 degrees under /Auxiliary/Root/ReferenceIncidenceAngle is used in calibration for the normalization of incidence angle correction. |
| view:off_nadir | /Auxiliary/Root/MBI/NearLookAngle, /Auxiliary/Root/MBI/FarLookAngle | [35.057220527416476,41.179045020001261] | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). MBI for Multi Beam Image or SBI for Single Beam Image. Also under /Auxiliary/Root/SubSwaths/SubSwath id="XX"/BeamOffNadirAngle -> [36.148362874984741,37.750097274780273,39.207453727722168,40.628804683685303] |
| sar:observation_direction | /Auxiliary/Root/LookSide | right | Antenna pointing direction, string: as "right" or "left". For KP5 given in capital letters -> e.g. from "RIGHT" to "right" |
| sar:instrument_mode | /Auxiliary/Root/AcquisitionMode | WS | REQUIRED. Choose acronym from: HIGH RESOLUTION (HR), ENHANCED HIGH RESOLUTION (EH), ULTRA HIGH RESOLUTION (UH), STANDARD (ST), ENHANCED STANDARD (ES), WIDE SWATH (WS), ENHANCED WIDE SWATH (EW) modes |
| sar:frequency_band | /Auxiliary/Root/RadarWavelength | X | REQUIRED: multiply by 100 to have wavelength in cm and choose among B={L(15-30),S(7.5-15),C(3.8-7.5),X(2.4-3.8)}, i.e. 0.031034415942028985 -> B=X |
| sar:center_frequency | /Auxiliary/Root/RadarFrequency | 9.66 | In gigahertz (GHz), multiple the value 9660000000 by 1e-9.  |
| sar:polarizations | /Auxiliary/Root/SubSwaths/SubSwath id="XX"/Polarisation | HH | REQUIRED Any combination of polarizations. Check each subswath XX=('01','02','03','04') |
| sar:product_type | /Auxiliary/Root/ProductType | GTC | REQUIRED: from string choose among P=(SSC,MGD,GRD,GEC,GTC,RTC), i.e. GTC_B -> P=GTC |
| processing:level | /Auxiliary/Root/RadarFrequency | L1D | Linked to sar:product_type -> (GTC PRODUCT Synonymous with Level 1D Product, GEC Synonymous with Level 1C Product), Ref. K5 SAR Products Attributes doc, V1.0 / 2014.04.01, page : 4/71) |
| created | /Auxiliary/Root/ProductGenerationUTC | 2020-12-10T00:25:58Z | format to ISO |
| start_datetime  | /Auxiliary/Root/SceneSensingStartUTC | 2020-12-09T00:42:07Z | format to ISO |
| end_datetime | /Auxiliary/Root/SceneSensingStopUTC | 2020-12-09T00:42:40Z | format to ISO |
| proj:epsg | /Auxiliary/Root/EllipsoidDesignator, /Auxiliary/Root/ProjectionID, /Auxiliary/Root/MapProjectionZone | 32644 | Use these 3 fields to derive EPSG. E.g. (WGS84,UNIVERSAL TRANSVERSE MERCATOR,44)->EPSG=32644, (WGS84,UNIVERSAL TRANSVERSE MERCATOR,-50)->EPSG=32750 |
| proj:wkt2 | | | find WKT2 from geometry. |
| sar:resolution_range | /Auxiliary/Root/GroundRangeGeometricResolution | 16.43536290851652 | Ground Range resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:resolution_azimuth | /Auxiliary/Root/AzimuthGeometricResolution | 19.730255997459341 | Azimuth resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <20m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:pixel_spacing_range | /Auxiliary/Root/SubSwaths/SubSwath/MBI/LineSpacing | 6.25 | Line spacing (azimuth) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:pixel_spacing_azimuth | /Auxiliary/Root/SubSwaths/SubSwath/MBI/ColumnSpacing  | 6.25 | Column spacing (range) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:looks_range | /Auxiliary/Root/Root/RangeProcessingNumberofLooks | 7.0 | number of looks perpendicular to the flight path |
| sar:looks_azimuth | /Auxiliary/Root/Root/AzimuthProcessingNumberofLooks | 1.0 | number of looks parallel to the flight path |
| sar:looks_equivalent_number | /Auxiliary/Root/MBI/EquivalentNumberofLooks | 4.0476193428039551 | The equivalent number of looks (ENL). SBI for STANDARD mode. |
| geometry TLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopLeftGeodeticCoordinates | [79.502419753930653,8.5997569381504899] | Extract Lon/Lat from <TopLeftGeodeticCoordinates>8.5997569381504899, 79.502419753930653, -96.708727340610722</TopLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry TRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopRightGeodeticCoordinates | [80.621019754110122,8.60248389641262] | Extract Lon/Lat from <TopRightGeodeticCoordinates>8.60248389641262, 80.621019754110122, 56.472185919132002</TopRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomRightGeodeticCoordinates | [80.62206588155189,7.4736259647067236] | Extract Lon/Lat from <BottomRightGeodeticCoordinates>7.4736259647067236, 80.62206588155189, 275.36740178243844</BottomRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomLeftGeodeticCoordinates | [79.506551881741757,7.4712610779009792] | Extract Lon/Lat from <BottomLeftGeodeticCoordinates>7.4712610779009792, 79.506551881741757, -98.916134332857993</BottomLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |

# KOMPSAT-5 Level-1D ENHANCED STANDARD (ES)

Ref1. KOMPSAT-5 PRODUCT SPECIFICATIONS, Standard Product Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref2. KOMPSAT-5 PRODUCT ATTRIBUTES, Product Attributes Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref3. KOMPSAT-5 Image Data Manual, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.0 April,2014.

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | Auxiliary/Root/ProductFilename  | KMPS5_GTC_B_ES_10_HH_RD_P_20190916093251_20190916093256_20190918030009 | remove the extension |
| mission | | kompsat-5 | HARDCODED mission name |
| platform | | kompsat-5 | HARDCODED mission name |
| instruments | | cosi | HARDCODED sensor name |
| sat:orbit_state | /Auxiliary/Root/OrbitDirection | ascending | |
| sat:absolute_orbit | /Auxiliary/Root/OrbitNumber | 40077 | |
| view:incidence_angle | /Auxiliary/Root/MBI/NearIncidenceAngle, /Auxiliary/Root/MBI/FarIncidenceAngle | [38.640129244624696,45.706093759532102] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from NearIncidenceAngle to FarIncidenceAngle. MBI for Multi Beam Image or SBI for Single Beam Image. The reference incidence angle of 45 degrees under /Auxiliary/Root/ReferenceIncidenceAngle is used in calibration for the normalization of incidence angle correction. |
| view:off_nadir | /Auxiliary/Root/MBI/NearLookAngle, /Auxiliary/Root/MBI/FarLookAngle | [35.057220527416476,41.179045020001261] | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). MBI for Multi Beam Image or SBI for Single Beam Image. Also under /Auxiliary/Root/SubSwaths/SubSwath id="XX"/BeamOffNadirAngle -> [36.148362874984741,37.750097274780273,39.207453727722168,40.628804683685303] |
| sar:observation_direction | /Auxiliary/Root/LookSide | right | Antenna pointing direction, string: as "right" or "left". For KP5 given in capital letters -> e.g. from "RIGHT" to "right" |
| sar:instrument_mode | /Auxiliary/Root/AcquisitionMode | WS | REQUIRED. Choose acronym from: HIGH RESOLUTION (HR), ENHANCED HIGH RESOLUTION (EH), ULTRA HIGH RESOLUTION (UH), STANDARD (ST), ENHANCED STANDARD (ES), WIDE SWATH (WS), ENHANCED WIDE SWATH (EW) modes |
| sar:frequency_band | /Auxiliary/Root/RadarWavelength | X | REQUIRED: multiply by 100 to have wavelength in cm and choose among B={L(15-30),S(7.5-15),C(3.8-7.5),X(2.4-3.8)}, i.e. 0.031034415942028985 -> B=X |
| sar:center_frequency | /Auxiliary/Root/RadarFrequency | 9.66 | In gigahertz (GHz), multiple the value 9660000000 by 1e-9.  |
| sar:polarizations | /Auxiliary/Root/SubSwaths/SubSwath id="XX"/Polarisation | HH | REQUIRED Any combination of polarizations. Check each subswath XX=('01','02','03','04') |
| sar:product_type | /Auxiliary/Root/ProductType | GTC | REQUIRED: from string choose among P=(SSC,MGD,GRD,GEC,GTC,RTC), i.e. GTC_B -> P=GTC |
| processing:level | /Auxiliary/Root/RadarFrequency | L1D | Linked to sar:product_type -> (GTC PRODUCT Synonymous with Level 1D Product, GEC Synonymous with Level 1C Product), Ref. K5 SAR Products Attributes doc, V1.0 / 2014.04.01, page : 4/71) |
| created | /Auxiliary/Root/ProductGenerationUTC | 2020-12-10T00:25:58Z | format to ISO |
| start_datetime  | /Auxiliary/Root/SceneSensingStartUTC | 2020-12-09T00:42:07Z | format to ISO |
| end_datetime | /Auxiliary/Root/SceneSensingStopUTC | 2020-12-09T00:42:40Z | format to ISO |
| proj:epsg | /Auxiliary/Root/EllipsoidDesignator, /Auxiliary/Root/ProjectionID, /Auxiliary/Root/MapProjectionZone | 32644 | Use these 3 fields to derive EPSG. E.g. (WGS84,UNIVERSAL TRANSVERSE MERCATOR,44)->EPSG=32644, (WGS84,UNIVERSAL TRANSVERSE MERCATOR,-50)->EPSG=32750 |
| proj:wkt2 | | | find WKT2 from geometry. |
| sar:resolution_range | /Auxiliary/Root/GroundRangeGeometricResolution | 16.43536290851652 | Ground Range resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:resolution_azimuth | /Auxiliary/Root/AzimuthGeometricResolution | 19.730255997459341 | Azimuth resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <20m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:pixel_spacing_range | /Auxiliary/Root/SubSwaths/SubSwath/MBI/LineSpacing | 6.25 | Line spacing (azimuth) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:pixel_spacing_azimuth | /Auxiliary/Root/SubSwaths/SubSwath/MBI/ColumnSpacing  | 6.25 | Column spacing (range) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:looks_range | /Auxiliary/Root/Root/RangeProcessingNumberofLooks | 7.0 | number of looks perpendicular to the flight path |
| sar:looks_azimuth | /Auxiliary/Root/Root/AzimuthProcessingNumberofLooks | 1.0 | number of looks parallel to the flight path |
| sar:looks_equivalent_number | /Auxiliary/Root/MBI/EquivalentNumberofLooks | 4.0476193428039551 | The equivalent number of looks (ENL). SBI for STANDARD mode. |
| geometry TLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopLeftGeodeticCoordinates | [79.502419753930653,8.5997569381504899] | Extract Lon/Lat from <TopLeftGeodeticCoordinates>8.5997569381504899, 79.502419753930653, -96.708727340610722</TopLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry TRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopRightGeodeticCoordinates | [80.621019754110122,8.60248389641262] | Extract Lon/Lat from <TopRightGeodeticCoordinates>8.60248389641262, 80.621019754110122, 56.472185919132002</TopRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomRightGeodeticCoordinates | [80.62206588155189,7.4736259647067236] | Extract Lon/Lat from <BottomRightGeodeticCoordinates>7.4736259647067236, 80.62206588155189, 275.36740178243844</BottomRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomLeftGeodeticCoordinates | [79.506551881741757,7.4712610779009792] | Extract Lon/Lat from <BottomLeftGeodeticCoordinates>7.4712610779009792, 79.506551881741757, -98.916134332857993</BottomLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |

# KOMPSAT-5 Level-1D ENHANCED WIDE SWATH (EW)

Ref1. KOMPSAT-5 PRODUCT SPECIFICATIONS, Standard Product Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref2. KOMPSAT-5 PRODUCT ATTRIBUTES, Product Attributes Specifications, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.2, July, 2015,
Ref3. KOMPSAT-5 Image Data Manual, KOREA AEROSPACE RESEARCH INSTITUTE, SI IMAGING SERVICES Co. LTD, Version 1.0 April,2014.

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | Auxiliary/Root/ProductFilename  | KMPS5_GTC_B_ES_10_HH_RD_P_20190916093251_20190916093256_20190918030009 | remove the extension |
| mission | | kompsat-5 | HARDCODED mission name |
| platform | | kompsat-5 | HARDCODED mission name |
| instruments | | cosi | HARDCODED sensor name |
| sat:orbit_state | /Auxiliary/Root/OrbitDirection | ascending | |
| sat:absolute_orbit | /Auxiliary/Root/OrbitNumber | 40077 | |
| view:incidence_angle | /Auxiliary/Root/MBI/NearIncidenceAngle, /Auxiliary/Root/MBI/FarIncidenceAngle | [38.640129244624696,45.706093759532102] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from NearIncidenceAngle to FarIncidenceAngle. MBI for Multi Beam Image or SBI for Single Beam Image. The reference incidence angle of 45 degrees under /Auxiliary/Root/ReferenceIncidenceAngle is used in calibration for the normalization of incidence angle correction. |
| view:off_nadir | /Auxiliary/Root/MBI/NearLookAngle, /Auxiliary/Root/MBI/FarLookAngle | [35.057220527416476,41.179045020001261] | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). MBI for Multi Beam Image or SBI for Single Beam Image. Also under /Auxiliary/Root/SubSwaths/SubSwath id="XX"/BeamOffNadirAngle -> [36.148362874984741,37.750097274780273,39.207453727722168,40.628804683685303] |
| sar:observation_direction | /Auxiliary/Root/LookSide | right | Antenna pointing direction, string: as "right" or "left". For KP5 given in capital letters -> e.g. from "RIGHT" to "right" |
| sar:instrument_mode | /Auxiliary/Root/AcquisitionMode | WS | REQUIRED. Choose acronym from: HIGH RESOLUTION (HR), ENHANCED HIGH RESOLUTION (EH), ULTRA HIGH RESOLUTION (UH), STANDARD (ST), ENHANCED STANDARD (ES), WIDE SWATH (WS), ENHANCED WIDE SWATH (EW) modes |
| sar:frequency_band | /Auxiliary/Root/RadarWavelength | X | REQUIRED: multiply by 100 to have wavelength in cm and choose among B={L(15-30),S(7.5-15),C(3.8-7.5),X(2.4-3.8)}, i.e. 0.031034415942028985 -> B=X |
| sar:center_frequency | /Auxiliary/Root/RadarFrequency | 9.66 | In gigahertz (GHz), multiple the value 9660000000 by 1e-9.  |
| sar:polarizations | /Auxiliary/Root/SubSwaths/SubSwath id="XX"/Polarisation | HH | REQUIRED Any combination of polarizations. Check each subswath XX=('01','02','03','04') |
| sar:product_type | /Auxiliary/Root/ProductType | GTC | REQUIRED: from string choose among P=(SSC,MGD,GRD,GEC,GTC,RTC), i.e. GTC_B -> P=GTC |
| processing:level | /Auxiliary/Root/RadarFrequency | L1D | Linked to sar:product_type -> (GTC PRODUCT Synonymous with Level 1D Product, GEC Synonymous with Level 1C Product), Ref. K5 SAR Products Attributes doc, V1.0 / 2014.04.01, page : 4/71) |
| created | /Auxiliary/Root/ProductGenerationUTC | 2020-12-10T00:25:58Z | format to ISO |
| start_datetime  | /Auxiliary/Root/SceneSensingStartUTC | 2020-12-09T00:42:07Z | format to ISO |
| end_datetime | /Auxiliary/Root/SceneSensingStopUTC | 2020-12-09T00:42:40Z | format to ISO |
| proj:epsg | /Auxiliary/Root/EllipsoidDesignator, /Auxiliary/Root/ProjectionID, /Auxiliary/Root/MapProjectionZone | 32644 | Use these 3 fields to derive EPSG. E.g. (WGS84,UNIVERSAL TRANSVERSE MERCATOR,44)->EPSG=32644, (WGS84,UNIVERSAL TRANSVERSE MERCATOR,-50)->EPSG=32750 |
| proj:wkt2 | | | find WKT2 from geometry. |
| sar:resolution_range | /Auxiliary/Root/GroundRangeGeometricResolution | 16.43536290851652 | Ground Range resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:resolution_azimuth | /Auxiliary/Root/AzimuthGeometricResolution | 19.730255997459341 | Azimuth resolution in meters (m). Values are: <3m for STANDARD MODE, <2.5m for ENHANCED STANDARD MODE, <20m for WIDE SWATH, <20m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2  |
| sar:pixel_spacing_range | /Auxiliary/Root/SubSwaths/SubSwath/MBI/LineSpacing | 6.25 | Line spacing (azimuth) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:pixel_spacing_azimuth | /Auxiliary/Root/SubSwaths/SubSwath/MBI/ColumnSpacing  | 6.25 | Column spacing (range) in meters. Strongly RECOMMENDED for GRD. MBI for Multi Beam Image or SBI for Single Beam Image. Values are: ~1.125m for STANDARD MODE and ENHANCED STANDARD MODE, ~12.5m for WIDE SWATH, ~7.5m for ENHANCED WIDE SWATH, Ref. K5 Standard Product Specifications Document v1.2 |
| sar:looks_range | /Auxiliary/Root/Root/RangeProcessingNumberofLooks | 7.0 | number of looks perpendicular to the flight path |
| sar:looks_azimuth | /Auxiliary/Root/Root/AzimuthProcessingNumberofLooks | 1.0 | number of looks parallel to the flight path |
| sar:looks_equivalent_number | /Auxiliary/Root/MBI/EquivalentNumberofLooks | 4.0476193428039551 | The equivalent number of looks (ENL). SBI for STANDARD mode. |
| geometry TLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopLeftGeodeticCoordinates | [79.502419753930653,8.5997569381504899] | Extract Lon/Lat from <TopLeftGeodeticCoordinates>8.5997569381504899, 79.502419753930653, -96.708727340610722</TopLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry TRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/TopRightGeodeticCoordinates | [80.621019754110122,8.60248389641262] | Extract Lon/Lat from <TopRightGeodeticCoordinates>8.60248389641262, 80.621019754110122, 56.472185919132002</TopRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BRGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomRightGeodeticCoordinates | [80.62206588155189,7.4736259647067236] | Extract Lon/Lat from <BottomRightGeodeticCoordinates>7.4736259647067236, 80.62206588155189, 275.36740178243844</BottomRightGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |
| geometry BLGC | /Auxiliary/Root/SubSwaths/SubSwath/MBI/BottomLeftGeodeticCoordinates | [79.506551881741757,7.4712610779009792] | Extract Lon/Lat from <BottomLeftGeodeticCoordinates>7.4712610779009792, 79.506551881741757, -98.916134332857993</BottomLeftGeodeticCoordinates> given as Geodetic Coordinates (Lat, Lon, Ellipsoidal Height). MBI for Multi Beam Image or SBI for Single Beam Image. |