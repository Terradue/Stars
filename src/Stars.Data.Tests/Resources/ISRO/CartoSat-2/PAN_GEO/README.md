# ISRO CartoSat-2 - PAN GEO sensor

Product given as single-band GeoTIFF with panchromatic  

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | ProductID | 203660031 | |
| mission | SatID | "cartosat-2" | |
| platform | SatID | "cartosat-2" | |
| instruments | Sensor | "pan" | PAN |
| sat:orbit_state | | | N.A. |
| sat:absolute_orbit | OrbitNo | 64579 | |
| view:sun_elevation | SunElevation | 47.15876842 | |
| view:sun_azimuth | SunAzimuth| 119.79799774 | |
| view:incidence_angle | AngleIncidence | 15.74005824 | |
| view:off_nadir | | | |
| processing:level | ProcessingLevel | "GEO" | |
| gsd | ObservedGSDPixel | 0.83327899 | |
| B1:name | | "pan" | hardcoded |
| B1:common_name | | "pan" | hardcoded |
| B1:center_wavelength | | 0.665 | hardcoded, 0.5 to 0.85 |
| B1:eai | | | |
| created | GenerationDateTime | 2020-JUN-26T14:43:37Z | Format to ISO: 26JUN20 14:43:37 |
| start_datetime | GenerationDateTime and SceneStartTime | 2020-JUN-26T02:15:51Z | 2020-JUN-26T02:15:51Z | Format to ISO |
| end_datetime | GenerationDateTime and SceneStartTime | 2020-JUN-26T02:15:51Z | Format to ISO join date and time |
| proj:epsg | MapProjection=UTM | | |
| proj:wkt2 | | | To be derived from epsg |
| geometry TLGC | ImageULLon & ImageULLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry TRGC | ImageURLon & ImageURLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BRGC | ImageLRLon & ImageLRLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |
| geometry BLGC | ImageLLLon & ImageLLLat | [lon,lat] | Coordinates Type="Geographic" Units="Degrees |