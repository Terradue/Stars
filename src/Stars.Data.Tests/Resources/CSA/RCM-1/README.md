# CSA RCM-1 - StripMap Very High Resolution 3m (3M) in Compact Polarimetry - Geocoded Detected (GCD) data product
Ref: RADARSAT Constellation Mission (RCM) technical characteristics, CSA, Accessed on Feb 2021 at www.asc-csa.gc.ca

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | | RCM1_OK1052791_PK1052796_1_3MCP24_20200219_123901_CH_CV_GCD | take product filename |
| mission | | rcm | HARDCODED mission name |
| platform | /product/sourceAttributes/sensor/satellite | rcm-1 | |
| instruments | /product/sourceAttributes/sensor | SAR | |
| sat:orbit_state | /product/sourceAttributes/orbitAndAttitude/orbitInformation/passDirection | descending | |
| sat:absolute_orbit | | | NOT AVAILABLE |
| view:incidence_angle | /product/sceneAttributes/imageAttributes/incAngNearRng & /product/sceneAttributes/imageAttributes/incAngFarRng | [40.94,42.26] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from Near Incidence Angle to Far Incidence Angle. Reference angles are available at https://www.asc-csa.gc.ca/eng/satellites/radarsat/technical-features/characteristics.asp. Values in spec tables can be identified using the code extracted from /product/sourceAttributes/beamModeMnemonic. E.g. '3M24' 41,04 (near) 42,36 (far) |
| view:off_nadir |  | | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). |
| sar:observation_direction | /product/sourceAttributes/radarParameters/antennaPointing | right | Antenna pointing direction, string: as "right" or "left". |
| sar:instrument_mode | /product/sourceAttributes/beamMode | 3M | REQUIRED, 'SC100' for Low resolution 100m (ScanSAR); 'SC50','SC30', and '16M' for Medium Resolution at 50m (ScanSAR), 30m (ScanSAR), and 16m (StripMap); '5M' for High Resolution 5m (StripMap); '3M' for Very High Resolution 3m (StripMap); 'LN' for Low Noise; 'SD' for Ship Detection; 'QP' for quad-polarization; and 'FSL' for Spotlight. |
| sar:frequency_band | | C | HARDCODED it is a SAR-C instrument |
| sar:center_frequency | /product/sourceAttributes/radarParameters/radarCenterFrequency | 5.405 | From Hertz to GigaHertz |
| sar:polarizations | /product/sourceAttributes/radarParameters/polarizations | [CH,CV] | REQUIRED 'CH and CV' stands for compact (RH and RV). RCM in compact polarization transmits in circular polarization and receives in dual linear polarization. RCM 5M data products are given in the following polarizations: Single (HH or HV or VH or VV), Dual-cross (HH or VV and HV), Dual-copol (HH and VV), or Compact (RH and RV). |
| sar:product_type | /product/imageGenerationParameters/generalProcessingInformation/productType | GCD | REQUIRED: typical RCM data products are: Single-look complex (SLC), Multilook complex (GRC), Ground-range detected (GRD), Geocoded complex (GCC), Geocoded detected data (GCD) |
| processing:level | | | NONE |
| created | /product/imageGenerationParameters/generalProcessingInformation/processingTime | 2020-FEB-21T15:08:16Z | format to ISO 2020-02-21T15:08:16.000000Z -> 2020-FEB-21T15:08:16Z |
| start_datetime | /product/sourceAttributes/rawDataStartTime | 2020-FEB-19T12:39:01Z | format to ISO 2020-02-19T12:39:01.820000Z -> 2020-FEB-19T12:39:01Z |
| end_datetime | | | N.A. |
| proj:epsg | /product/geographicInformation/mapProjection/utmProjectionParameters/utmZone & hemisphere | 32614 | UTM 14N |
| proj:wkt2 | | | find WKT2 from BBOX |
| sar:resolution_range | | 3 | Range resolution in meters (m). Not available. Use nominal values from https://www.asc-csa.gc.ca/eng/satellites/radarsat/technical-features/radarsat-comparison.asp. for 3M -> 3 × 3 @ 35° |
| sar:resolution_azimuth | | 3 | Azimuth resolution in meters (m). Not available. Same as 'sar:resolution_range' |
| sar:pixel_spacing_range | /product/imageReferenceAttributes/rasterAttributes/sampledPixelSpacing | 0.8 | Strongly RECOMMENDED for GRD |
| sar:pixel_spacing_azimuth | /product/imageReferenceAttributes/rasterAttributes/sampledPixelSpacing | 0.8 | Strongly RECOMMENDED for GRD |
| sar:looks_range | /product/imageGenerationParameters/sarProcessingInformation/numberOfRangeLooks | 1 |  |
| sar:looks_azimuth | /product/imageGenerationParameters/sarProcessingInformation/numberOfAzimuthLooks | 1 |  |
| sar:looks_equivalent_number | | | The equivalent number of looks (ENL). N.A. |
| geometry TLGC | /product/geographicInformation/mapProjection/positioningInformation/upperLeftCorner/geodeticCoordinate/longitude & latitude | [longitude,latitude] | |
| geometry TRGC | /product/geographicInformation/mapProjection/positioningInformation/upperRightCorner/geodeticCoordinate/longitude & latitude | [longitude,latitude] | |
| geometry BRGC | /product/geographicInformation/mapProjection/positioningInformation/lowerRightCorner/geodeticCoordinate/longitude & latitude | [longitude,latitude] | |
| geometry BLGC | /product/geographicInformation/mapProjection/positioningInformation/lowerLeftCorner/geodeticCoordinate/longitude & latitude | [longitude,latitude] | |