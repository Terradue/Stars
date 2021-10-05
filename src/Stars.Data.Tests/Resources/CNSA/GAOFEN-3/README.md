# CNSA GAOFEN-3 - Fine StripMap (FSI) - Dual-cross (HH,HV) - L2 SGC data product
Metadata given as XML
L2 SGC as Deteceted amplitude in map projection.

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | | GF3_SAY_FSI_023228_W65.3_S18.7_20210107_L2_HHHV_L20005379609 | take product filename and remove extension |
| mission | /product/satellite | gf-3 | |
| platform | /product/satellite | gf-3 | |
| instruments | /product/sensor/sensorID | SAR | |
| sat:orbit_state | /product/Direction | descending | Assuming "DEC" for descending|
| sat:absolute_orbit | /product/orbitID | 5379609 | |
| view:incidence_angle | /product/processinfo/incidenceAngleNearRange & /product/processinfo/incidenceAngleFarRange | [35.280714,38.456128] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from Near Incidence Angle to Far Incidence Angle. If one value is needed, take the average. |
| view:off_nadir | /product/sensor/waveParams/wave/centerLookAngle | 32.2 | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). If one value is needed, take the average. |
| sar:observation_direction | /product/sensor/lookDirection | right | Antenna pointing direction, string: as "right" or "left". |
| sar:instrument_mode | /product/sensor/imagingMode & agcMode | FSI | REQUIRED. GF3 has the following acquisition modes: Spotlight (SL), Stripmap (SuperFine (UFS), Fine (FSI), Wide Fine (FSII), Standard (SS), Quad. Pol 1 (QPSI), and Quad Pol.2 (QPSII) ), ScanSAR (Narrow (NSC), Wide (WSC), and GLobal (GLO)), and Wave (WAV). |
| sar:frequency_band | | C | HARDCODED it is a SAR-C instrument |
| sar:center_frequency | /product/sensor/RadarCenterFrequency | 5.40 | From Hertz to GigaHertz |
| sar:polarizations | /product/productinfo/productPolar or /product/polarParams/polar/polarMode | [HH, HV] | REQUIRED GF-3 is given as: Single (HH or VV), Dual (HH or VV), Dual-cross (HH and HV or VV and VH), Quad-pol (HH, VV, VH and HV). |
| sar:product_type | /product/productinfo/productType | SGC | REQUIRED: typical GF3 data products are: Single-look complex (SLC), Single-look image products (SLP), Multi-look image products (MLP), or Geocoded products (SGC). Is given as Detected Amplitude in Map projection -> SGC and not SLP. |
| processing:level | /product/productinfo/productType | L2 | L1A for SLC, L1B for SLP, L1B for MLP, L2 for SGC |
| created | /product/productinfo/productGentime | 2021-JAN-08T13:49:16Z | format to ISO 2021-01-08 13:49:16 -> 2021-JAN-08T13:49:16Z |
| start_datetime | /product/imageinfo/imagingTime/start | 2021-JAN-07T09:54:49Z | format to ISO 2021-01-07 09:54:49.286464 -> 2021-JAN-07T09:54:49Z |
| start_datetime | /product/imageinfo/imagingTime/end | 2021-JAN-07T09:54:58Z | format to ISO 2021-01-07 09:54:58.238015 -> 2021-JAN-07T09:54:58Z |
| HH:qualify | /product/imageinfo/QualifyValue/HH | 997.102600 | |
| HV:qualify | /product/imageinfo/QualifyValue/HV | 466.574951 | |
| VH:qualify | /product/imageinfo/QualifyValue/VH | | N.A. |
| VV:qualify | /product/imageinfo/QualifyValue/VV | | N.A. |
| HH:calibration_constant | /product/processinfo/CalibrationConst/HH | 24.460000 | |
| HV:calibration_constant | /product/processinfo/CalibrationConst/HV | 24.530000 | |
| VH:calibration_constant | /product/processinfo/CalibrationConst/VH | | N.A. |
| VV:calibration_constant | /product/processinfo/CalibrationConst/VV | | N.A. |
| proj:epsg | /product/processinfo/ProjectModel | | UTM, from gdal PROJCS["unnamed",GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]],PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",-18.74938865686857],PARAMETER["central_meridian",-65.33728102074238],PARAMETER["scale_factor",0.9996],PARAMETER["false_easting",400000],PARAMETER["false_northing",100000],UNIT["Meter",1]] |
| proj:wkt2 | | | find WKT2 from BBOX |
| sar:resolution_range | | | Range resolution in meters (m). Not available. |
| sar:resolution_azimuth | | | Azimuth resolution in meters (m). Not available. |
| sar:pixel_spacing_range | /product/imageinfo/rasterAttributes/widthspace | 2.5 | Strongly RECOMMENDED for GRD |
| sar:pixel_spacing_azimuth | /product/imageinfo/rasterAttributes/heightspace | 2.5 | Strongly RECOMMENDED for GRD |
| sar:looks_range | /product/processinfo/MultilookRange | 1 |  |
| sar:looks_azimuth | /product/processinfo/MultilookAzimuth | 1 |  |
| sar:looks_equivalent_number | | | The equivalent number of looks (ENL). N.A. |
| geometry TLGC | /product/imageinfo/corner/topLeft/longitude & latitude | | |
| geometry TRGC | /product/imageinfo/corner/topRight/longitude & latitude | | |
| geometry BRGC | /product/imageinfo/corner/bottomRight/longitude & latitude | | |
| geometry BLGC | /product/imageinfo/corner/bottomLeft/longitude & latitude | | |