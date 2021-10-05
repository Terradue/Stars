# ABAE VRSS-1 - L2B - Panchromatic (PAN)
Ref. https://directory.eoportal.org/web/eoportal/satellite-missions/v-w-x-y-z/vrss-1

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /productMeta/imageName | VRSS-1_PAN-2_0698_0239_20200814_L2B_817135102196 | Remove extension from basename |
| mission | /productMeta/satelliteId | vrss-1 | |
| platform | /productMeta/satelliteId | vrss-1 | |
| instruments | /productMeta/sensorId | pan-2 | |
| sat:orbit_state | |  | N.A. |
| sat:absolute_orbit | /productMeta/orbitId | 42475 | |
| view:incidence_angle | | | N.A. |
| view:off_nadir | /productMeta/satOffNadir | -21.270622 | |
| processing:level | /productMeta/productLevel | L2B | |
| gsd | /productMeta/sensorGSD | 2.5 | Ground Spacing Resolution |
| PAN:name | | "pan" | hardcoded, or from /productMeta/sensorId |
| PAN:common_name | | "pan" | hardcoded, or from /productMeta/sensorId |
| PAN:center_wavelength | | 0.675 | hardcoded, 450-900 | |
| PAN:EAI | /productMeta/SolarIrradiance | 1368.769287 | ESUN in watt/m2/micron |
| PAN:Gain | /productMeta/ab_calibra_param/K | 5.116200 | Calibration parameter |
| PAN:Offset | /productMeta/ab_calibra_param/B | 39.540001 | Calibration parameter |
| created | /productMeta/productDate | 2020-AUG-17T14:01:37Z | Format to ISO: 2020 08 17 14:01:37.159767 to 2020-AUG-17T14:01:37Z |
| start_datetime  | /productMeta/Scene_imagingStartTime | 2020-AUG-14T05:16:59Z | Format to ISO: 2020 08 14T05:16:59.381451 to 2020-AUG-14T05:16:59Z |
| end_datetime | /productMeta/Scene_imagingStopTime | 2020-AUG-14T05:17:04Z | Format to ISO: 2020 08 14T05:17:04.232018 to 2020-AUG-14T05:17:04Z |
| proj:epsg | /productMeta/mapProjection & /productMeta/Zone_Number | 32740 | from UTM zone to (40S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /productMeta/dataUpperLeftLong & dataUpperLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /productMeta/dataUpperRightLong & dataUpperRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /productMeta/dataLowerRightLong & dataLowerRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /productMeta/dataLowerLeftLong & dataLowerLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |

# ABAE VRSS-1 - L2B - Multispectral (MSS)

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /productMeta/browseName | VRSS-1_MSS-2_0698_0239_20200814_L2B_81713505511 | Remove extension from basename |
| mission | /productMeta/satelliteId | vrss-1 | |
| platform | /productMeta/satelliteId | vrss-1 | |
| instruments | /productMeta/sensorId | mss-2 | |
| sat:orbit_state | |  | N.A. |
| sat:absolute_orbit | /productMeta/orbitId | 42475 | |
| view:incidence_angle | | | N.A. |
| view:off_nadir | /productMeta/satOffNadir | -21.285824 | |
| processing:level | /productMeta/productLevel | L2B | |
| gsd | /productMeta/sensorGSD | 10.0 | Ground Spacing Resolution |
| B1:name | /productMeta/bands | "band-1" | |
| B1:common_name | | "blue" | hardcoded |
| B1:center_wavelength | | 0.485 | hardcoded, 450–520 nm |
| B1:EAI | /productMeta/SolarIrradiance for Band="1" | 1976.417847 | ESUN in watt/m2/micron |
| B1:Gain | /productMeta/ab_calibra_param/K for Band="1" | 4.416600 | Calibration parameter |
| B1:Offset | /productMeta/ab_calibra_param/B for Band="1" | 60.790001 | Calibration parameter |
| B2:name | /productMeta/bands | "band-2" | |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.555 | hardcoded, 520–590 nm |
| B2:EAI | /productMeta/SolarIrradiance for Band="2" | 1863.542725 | ESUN in watt/m2/micron |
| B2:Gain | /productMeta/ab_calibra_param/K for Band="2" | 4.897800 | Calibration parameter |
| B2:Offset | /productMeta/ab_calibra_param/B for Band="2" | 38.639999 | Calibration parameter |
| B3:name | /productMeta/bands | "band-3" | |
| B3:common_name | | "red" | hardcoded |
| B3:center_wavelength | | 0.66 | hardcoded, 630–690 nm |
| B3:EAI | /productMeta/SolarIrradiance for Band="3" | 1542.293457 | ESUN in watt/m2/micron |
| B3:Gain | /productMeta/ab_calibra_param/K for Band="3" | 5.477900 | Calibration parameter |
| B3:Offset | /productMeta/ab_calibra_param/B for Band="3" | 33.070000 | Calibration parameter |
| B4:name | /productMeta/bands | "band-4" | |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.83 | hardcoded, 770–890 nm |
| B4:EAI | /productMeta/SolarIrradiance for Band="4" | 1073.072144 | ESUN in watt/m2/micron |
| B4:Gain | /productMeta/ab_calibra_param/K for Band="4" | 3.977300 | Calibration parameter |
| B4:Offset | /productMeta/ab_calibra_param/B for Band="4" | 20.100000 | Calibration parameter |
| created | /productMeta/productDate | 2020-AUG-17T14:07:46Z | Format to ISO: 2020 08 17 14:01:37.159767 to 2020-AUG-17T14:01:37Z |
| start_datetime  | /productMeta/Scene_imagingStartTime | 2020-AUG-14T05:16:55Z | Format to ISO: 2020 08 14T05:16:59.381451 to 2020-AUG-14T05:16:59Z |
| end_datetime | /productMeta/Scene_imagingStopTime | 2020-AUG-14T05:17:00Z | Format to ISO: 2020 08 14T05:17:04.232018 to 2020-AUG-14T05:17:04Z |
| proj:epsg | /productMeta/mapProjection & /productMeta/Zone_Number | 32740 | from UTM zone to (40S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /productMeta/dataUpperLeftLong & dataUpperLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /productMeta/dataUpperRightLong & dataUpperRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /productMeta/dataLowerRightLong & dataLowerRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /productMeta/dataLowerLeftLong & dataLowerLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |