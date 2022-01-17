# CSA RCM-2 - Quad-Polarization (QP) - Ground Range Detected (GRD) data product
Ref: RADARSAT Constellation Mission (RCM) technical characteristics, CSA, Accessed on Feb 2021 at www.asc-csa.gc.ca

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | | RCM2_OK1486596_PK1486712_1_QP26_20210209_124433_HH_VV_HV_VH_GRD | take product filename |
| mission | | rcm | HARDCODED mission name |
| platform | /product/sourceAttributes/sensor/satellite | rcm-2 | |
| instruments | /product/sourceAttributes/sensor | SAR | |
| sat:orbit_state | /product/sourceAttributes/orbitAndAttitude/orbitInformation/passDirection | ascending | |
| sat:absolute_orbit | | | NOT AVAILABLE |
| view:incidence_angle | /product/sceneAttributes/imageAttributes/incAngNearRng & /product/sceneAttributes/imageAttributes/incAngFarRng | [42.38,43.82] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from Near Incidence Angle to Far Incidence Angle. Reference angles are available at https://www.asc-csa.gc.ca/eng/satellites/radarsat/technical-features/characteristics.asp. Values in spec tables can be identified using the code extracted from /product/sourceAttributes/beamModeMnemonic. E.g. '3M24' 41,04 (near) 42,36 (far) |
| view:off_nadir |  | | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). |
| sar:observation_direction | /product/sourceAttributes/radarParameters/antennaPointing | right | Antenna pointing direction, string: as "right" or "left". |
| sar:instrument_mode | /product/sourceAttributes/beamMode | QP | REQUIRED, 'SC100' for Low resolution 100m (ScanSAR); 'SC50','SC30', and '16M' for Medium Resolution at 50m (ScanSAR), 30m (ScanSAR), and 16m (StripMap); '5M' for High Resolution 5m (StripMap); '3M' for Very High Resolution 3m (StripMap); 'LN' for Low Noise; 'SD' for Ship Detection; 'QP' for quad-polarization; and 'FSL' for Spotlight. |
| sar:frequency_band | | C | HARDCODED it is a SAR-C instrument |
| sar:center_frequency | /product/sourceAttributes/radarParameters/radarCenterFrequency | 5.405 | From Hertz to GigaHertz |
| sar:polarizations | /product/sourceAttributes/radarParameters/polarizations | [HH, HV, VH, VV] | REQUIRED 'CH and CV' stands for compact (RH and RV). RCM in compact polarization transmits in circular polarization and receives in dual linear polarization. RCM 5M data products are given in the following polarizations: Single (HH or HV or VH or VV), Dual-cross (HH or VV and HV), Dual-copol (HH and VV), or Compact (RH and RV). |
| sar:product_type | /product/imageGenerationParameters/generalProcessingInformation/productType | GRD | REQUIRED: typical RCM data products are: Single-look complex (SLC), Multilook complex (GRC), Ground-range detected (GRD), Geocoded complex (GCC), Geocoded detected data (GCD) |
| processing:level | | | NONE |
| created | /product/imageGenerationParameters/generalProcessingInformation/processingTime | 2021-FEB-09T22:14:18Z | format to ISO 2021-02-09T22:14:18.000000Z -> 2021-FEB-09T22:14:18Z |
| start_datetime | /product/sourceAttributes/rawDataStartTime | 2021-FEB-09T12:44:33Z | format to ISO 2021-02-09T12:44:33.186000Z -> 2021-FEB-09T12:44:33Z |
| end_datetime | | | N.A. |
| proj:epsg | /product/geographicInformation/ellipsoidParameters/ellipsoidName | 4326 | GCS: WGS84 |
| proj:wkt2 | | | find WKT2 from BBOX |
| sar:resolution_range | | 9 | Range resolution in meters (m). Not available. Use nominal values from https://www.asc-csa.gc.ca/eng/satellites/radarsat/technical-features/radarsat-comparison.asp. for QP -> 9 × 9 |
| sar:resolution_azimuth | | 9 | Azimuth resolution in meters (m). Not available. Same as 'sar:resolution_range' |
| sar:pixel_spacing_range | /product/imageReferenceAttributes/rasterAttributes/sampledPixelSpacing | 2.5 | Strongly RECOMMENDED for GRD |
| sar:pixel_spacing_azimuth | /product/imageReferenceAttributes/rasterAttributes/sampledPixelSpacing | 2.5 | Strongly RECOMMENDED for GRD |
| sar:looks_range | /product/imageGenerationParameters/sarProcessingInformation/numberOfRangeLooks | 1 |  |
| sar:looks_azimuth | /product/imageGenerationParameters/sarProcessingInformation/numberOfAzimuthLooks | 1 |  |
| sar:looks_equivalent_number | | | The equivalent number of looks (ENL). N.A. |
| geometry TLGC | | | |
| geometry TRGC | | | |
| geometry BRGC | | | |
| geometry BLGC | | | |