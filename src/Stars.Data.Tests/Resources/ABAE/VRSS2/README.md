# ABAE VRSS-2 - L2B - Panchromatic (PAN)

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /productMeta/imageName | VRSS-2_PAN_0996_0379_20210127_L2B_128183941985 | Remove extension from basename |
| mission | /productMeta/satelliteId | vrss-2 | |
| platform | /productMeta/satelliteId | vrss-2 | |
| instruments | /productMeta/sensorId | pan | |
| sat:orbit_state | |  | N.A. |
| sat:absolute_orbit | /productMeta/orbitId | 17788 | |
| view:incidence_angle | | | N.A. |
| view:off_nadir | /productMeta/satOffNadir | -27.046949 | |
| processing:level | /productMeta/productLevel | L2B | |
| gsd | /productMeta/sensorGSD | 1 | Ground Spacing Resolution |
| PAN:name | | "pan" | hardcoded, or from /productMeta/sensorId |
| PAN:common_name | | "pan" | hardcoded, or from /productMeta/sensorId |
| PAN:center_wavelength | | 0.650 | hardcoded, 500-800 |
| PAN:EAI | /productMeta/SolarIrradiance | 1507.420044 | ESUN in watt/m2/micron |
| PAN:Gain | /productMeta/ab_calibra_param/K | 3.271800 | Calibration parameter |
| PAN:Offset | /productMeta/ab_calibra_param/B | 40.626000 | Calibration parameter |
| created | /productMeta/productDate | 2021-JAN-28T18:47:54Z | Format to ISO: 2021 01 28 18:47:54.161185 to 2021-JAN-28T18:47:54Z |
| start_datetime  | /productMeta/Scene_imagingStartTime | 2021-JAN-27T02:20:09Z | Format to ISO: 2021 01 27 02:20:09.786955 to 2021-JAN-27T02:20:09Z |
| end_datetime | /productMeta/Scene_imagingStopTime | 2021-JAN-27T02:20:15Z | Format to ISO: 2021 01 27 02:20:15.181830 to 2021-JAN-27T02:20:15Z |
| proj:epsg | /productMeta/mapProjection & /productMeta/Zone_Number | 32750 | from UTM zone to (50S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /productMeta/dataUpperLeftLong & dataUpperLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /productMeta/dataUpperRightLong & dataUpperRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /productMeta/dataLowerRightLong & dataLowerRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /productMeta/dataLowerLeftLong & dataLowerLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |

# ABAE VRSS-2 - L2B - Multispectral (MSS)

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /productMeta/browseName | VRSS-2_MSS_0996_0379_20210127_L2B_12818393908 | Remove extension from basename |
| mission | /productMeta/satelliteId | vrss-2 | |
| platform | /productMeta/satelliteId | vrss-2 | |
| instruments | /productMeta/sensorId | mss | |
| sat:orbit_state | |  | N.A. |
| sat:absolute_orbit | /productMeta/orbitId | 17788 | |
| view:incidence_angle | | | N.A. |
| view:off_nadir | /productMeta/satOffNadir | -27.046949 | |
| processing:level | /productMeta/productLevel | L2B | |
| gsd | /productMeta/sensorGSD | 3 | Ground Spacing Resolution |
| B1:name | /productMeta/bands | "band-1" | |
| B1:common_name | | "blue" | hardcoded |
| B1:center_wavelength | | 0.485 | hardcoded, 450–520 nm |
| B1:EAI | /productMeta/SolarIrradiance for Band="1" | 1933.640015 | ESUN in watt/m2/micron |
| B1:Gain | /productMeta/ab_calibra_param/K for Band="1" | 2.290750 | Calibration parameter |
| B1:Offset | /productMeta/ab_calibra_param/B for Band="1" | 49.852500 | Calibration parameter |
| B2:name | /productMeta/bands | "band-2" | |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.555 | hardcoded, 520–590 nm |
| B2:EAI | /productMeta/SolarIrradiance for Band="2" | 1847.510010 | ESUN in watt/m2/micron |
| B2:Gain | /productMeta/ab_calibra_param/K for Band="2" | 3.300200 | Calibration parameter |
| B2:Offset | /productMeta/ab_calibra_param/B for Band="2" | 66.413500 | Calibration parameter |
| B3:name | /productMeta/bands | "band-3" | |
| B3:common_name | | "red" | hardcoded |
| B3:center_wavelength | | 0.66 | hardcoded, 630–690 nm |
| B3:EAI | /productMeta/SolarIrradiance for Band="3" | 1536.420044 | ESUN in watt/m2/micron |
| B3:Gain | /productMeta/ab_calibra_param/K for Band="3" | 3.001100 | Calibration parameter |
| B3:Offset | /productMeta/ab_calibra_param/B for Band="3" | 58.845000 | Calibration parameter |
| B4:name | /productMeta/bands | "band-4" | |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.83 | hardcoded, 770–890 nm |
| B4:EAI | /productMeta/SolarIrradiance for Band="4" | 1064.979980 | ESUN in watt/m2/micron |
| B4:Gain | /productMeta/ab_calibra_param/K for Band="4" | 3.871100 | Calibration parameter |
| B4:Offset | /productMeta/ab_calibra_param/B for Band="4" | 46.252500 | Calibration parameter |
| created | /productMeta/productDate | 2020-AUG-17T14:07:46Z | Format to ISO: 2021 01 28 18:50:50.161185 to 2021-JAN-28T18:50:50Z |
| start_datetime  | /productMeta/Scene_imagingStartTime | 2020-AUG-14T05:16:55Z | Format to ISO: 2021 01 27 02:20:09.142848 to 2021-JAN-27T02:20:09Z |
| end_datetime | /productMeta/Scene_imagingStopTime | 2020-AUG-14T05:17:00Z | Format to ISO: 2021 01 27 02:20:14.754697 to 2021-JAN-27T02:20:14Z |
| proj:epsg | /productMeta/mapProjection & /productMeta/Zone_Number | 32750 | from UTM zone to (50S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /productMeta/dataUpperLeftLong & dataUpperLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /productMeta/dataUpperRightLong & dataUpperRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /productMeta/dataLowerRightLong & dataLowerRightLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /productMeta/dataLowerLeftLong & dataLowerLeftLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |