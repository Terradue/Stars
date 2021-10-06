# ISRO ResourceSat-2 - LISS-IV sensor

Product in View mode given as 4-band 8-bit GeoTIFF with the following band order: 8 7 6 5 to make automatic false NIR RGB 1(nir)-2(red)-3(green). Use 2(red)-3(green)-4(blue) for natural color RGB.  

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /prdf/image/band5 | CBERS_4_MUX_20201031_003_073_L2_BAND5 | Remove path and extension from basename |
| mission | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| platform | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| instruments | /prdf/satellite/instrument | "mux" | MUX |
| sat:orbit_state | /prdf/image/orbitDirection | "descending" | Descending |
| sat:absolute_orbit | /prdf/image/path & /prdf/image/row | "3_073" | CBERS Path and Row are also included in the product filename given as: "CBERS_{N}_{CAM}_{YYYYMMDD}_{PPP}_{RRR}_L{L}_BAND{B}.{tif|xml}" where: N mission, CAM camera identification (MUX, AWFI, PAN10M or PAN5M), YYYYMMDD acquisition date, PPP path, and RRR row. |
| view:sun_elevation | /prdf/sunPosition/elevation | 48.4855 | |
| view:sun_azimuth | /prdf/sunPosition/sunAzimuth | 156.103 | |
| view:incidence_angle | /prdf/sunIncidenceAngle | 41.9625 | It is given as degree, minute, seconds, milliseconds. Convert 30 degrees 15 minutes and 50 seconds angle to decimal degrees: 30° 15' 50" The decimal degrees dd is equal to: dd = d + m/60 + s/3600 = 30° + 15'/60 + 50"/3600 = 30.263888889° (https://www.rapidtables.com/convert/number/degrees-minutes-seconds-to-degrees.html) |
| view:off_nadir | /prdf/orientationAngle | 0.000389889 | |
| processing:level | /prdf/image/level | L2 | |
| gsd | /prdf/image/productMeta/verticalPixelSize and horizontalPixelSize | 20 | |
| B1:name | | "band-8" | hardcoded, from /prdf/availableBands |
| B1:common_name | | "nir" | hardcoded |
| B1:center_wavelength | | 0.830 | hardcoded, 770 - 890 |
| B2:name | | "band-7" | hardcoded, from /prdf/availableBands |
| B2:common_name | | "red" | hardcoded |
| B2:center_wavelength | | 0.660 | hardcoded, 630 - 690 |
| B3:name | | "band-6" | hardcoded, from /prdf/availableBands |
| B3:common_name | | "green" | hardcoded |
| B3:center_wavelength | | 0.555 | hardcoded, 520 - 590 |
| B4:name | | "band-5" | derive from /prdf/availableBands |
| B4:common_name | | "blue" | hardcoded |
| B4:center_wavelength | | 0.485 | hardcoded, 450 - 520 |
| created | /prdf/image/processingTime | 2020-NOV-01T03:27:00Z | Format to ISO: 2020-11-01T03:27:00.279915 to 2020-NOV-01T03:27:00Z |
| start_datetime  | /prdf/image/timeStamp/begin | 2021-JAN-27T02:20:09Z | Format to ISO: 2020-10-31T03:16:01.968242 to 2020-OCT-31T03:16:01Z |
| end_datetime | /prdf/image/timeStamp/end | 2021-JAN-27T02:20:15Z | Format to ISO: 2020-10-31T03:16:19.763179 to 2020-OCT-31T03:16:19Z |
| proj:epsg | /prdf/image/projectionName & originLongitude & originLatitude | 32750 | UTM zone is (50S) to EPSG code |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | /prdf/image/boundingBox/UL/longitude & latitude | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | /prdf/image/boundingBox/UR/longitude & latitude | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | /prdf/image/boundingBox/LR/longitude & latitude | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | /prdf/image/boundingBox/LL/longitude & latitude | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |