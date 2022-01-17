# SAOCOM-1 Level-1D TOPSAR WIDE
Ref: Saocom-1 Level 1 Products Format, CONAE, Jan 2020
Notes: Xml given for each single pol product under the "CONAE/SAOCOM-1/data/FILENAME/Data" folder 

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /SAOCOM_XMLProduct/Channel/RasterInfo/FileName | S1A_OPER_SAR_EOSSP__CORE_L1D_OLVF_20200819T231514 | take L1D product filename from /CONAE/SAOCOM-1/L1D/data/item_id/Config/parameterFile2.xml' as 'S1<X>_OPER_SAR_EOSSP__CORE_<LLL>_<Orbit>_<DDDDDDDD>T<TTTTTT>' |
| mission | | saocom-1 | HARDCODED mission name |
| platform | /SAOCOM_XMLProduct/Channel/RasterInfo/FileName | saocom-1A | from filename under /CONAE/SAOCOM-1/L1D/data/item_id/Data as '<level>-acqId<cccccccccc>-<x>-<mmm>-<hhhhhhhhhh>-<pp>-<r>' extract satellite A/B from -<x>- |
| instruments | /SAOCOM_XMLProduct/Channel/DataSetInfo/SensorName | SAO1A | |
| sat:orbit_state | /SAOCOM_XMLProduct/Channel/StateVectorData/OrbitDirection | ascending | |
| sat:absolute_orbit | /SAOCOM_XMLProduct/Channel/StateVectorData/OrbitNumber | | NOT AVAILABLE the .xemt metadata file is required |
| view:incidence_angle | | [24.9,48.7] | The incidence angle is the angle between the incident SAR beam and the axis perpendicular to the local geodetic ground surface (from target point of view). It ranges from Near Incidence Angle to Far Incidence Angle. Min incidence angle for TOPSAR Wide Single Pol and Dual Pol: near range 24.9, far range 48.7. Ref: CONAE, Saocom-1 Level 1 Products Format, Jan 2020, Tab.3 Level 1 Products main characteristics|
| view:off_nadir |  | [15,50] | The off-nadir angle (or look angle) is the angle between the satellite's nadir position and the SAR beam (from instrument point of view). Ref. https://eo.belspo.be/nl/satellites-and-sensors/saocom-1a |
| sar:observation_direction | /SAOCOM_XMLProduct/Channel/DataSetInfo/SideLooking | right | Antenna pointing direction, string: as "right" or "left". For KP5 given in capital letters -> e.g. from "RIGHT" to "right" |
| sar:instrument_mode | /SAOCOM_XMLProduct/Channel/RasterInfo/FileName | TW | REQUIRED, take filename '<level>-acqId<cccccccccc>-<x>-<mmm>-<hhhhhhhhhh>-<pp>-<r>' and extract <mmm> or <mmmm> "smx" for stripmap x, with x the beam number 1 to 10; "tna" for topsar narrow A; "tnb" for topsar narrow B; "tw" for topsar wide. In general TW, TN, SM (for TOPSAR Wide, TOPSAR Narrow  and Stripmap). |
| sar:frequency_band | | L | HARDCODED it is a SAR-L instrument |
| sar:center_frequency | /SAOCOM_XMLProduct/Channel/DataSetInfo/fc_hz | 12.75 | In gigahertz (GHz), multiple the 1275000000 value by 1e-9 and insert 12.75|
| sar:polarizations | /SAOCOM_XMLProduct/Channel/SwathInfo/Polarization | [VV,VH] | REQUIRED for both single pol images convert V/V and V/H to VV and VH. Same also from filename <pp> field |
| sar:product_type | /SAOCOM_XMLProduct/Channel/RasterInfo/FileName | GTC | REQUIRED: take filename '<level>-acqId<cccccccccc>-<x>-<mmm>-<hhhhhhhhhh>-<pp>-<r>' and extract <level> from the following options <level>={slc (L1A), di (L1B), gec (L1C), gtc (L1D)} and choose among P=(SSC,MGD,GRD,GEC,GTC,RTC), i.e. P=GTC |
| processing:level | /SAOCOM_XMLProduct/Channel/RasterInfo/FileName | L1D | from sar:product_type <level>={slc (L1A), di (L1B), gec (L1C), gtc (L1D)} |
| created | /SAOCOM_XMLProduct/Channel/DataSetInfo/ProcessingDate | 20-AUG-2020T01:25:21Z | format to ISO 20-AUG-2020 01:25:21.947890000000 -> 20-AUG-2020T01:25:21.94Z |
| start_datetime  | /SAOCOM_XMLProduct/Channel/SwathInfo/AcquisitionStartTime | 19-AUG-2020T21:17:26Z  | format to ISO 19-AUG-2020 21:17:26.613000000000 -> 19-AUG-2020T21:17:26.61Z |
| end_datetime | | | NOT AVAILABLE the .xemt metadata file is required |
| proj:epsg | /SAOCOM_XMLProduct/Channel/DataSetInfo/ProjectionParameters | 32652 | Use this field to derive EPSG. E.g. (+proj=utm +zone=52 +ellps=WGS84 +datum=WGS84 +units=m +no_defs +x_0=372414 +y_0=5.41344e+06)->EPSG=32652 |
| proj:wkt2 | | | find WKT2 from BBOX |
| sar:resolution_range | /Auxiliary/Root/GroundRangeGeometricResolution | 50 | Range resolution in meters (m). Ref: CONAE, Saocom-1 Level 1 Products Format, Jan 2020, Tab.3 Level 1 Products main characteristics. |
| sar:resolution_azimuth | /Auxiliary/Root/AzimuthGeometricResolution | 50 | Azimuth resolution in meters (m). Ref: CONAE, Saocom-1 Level 1 Products Format, Jan 2020, Tab.3 Level 1 Products main characteristics. |
| sar:pixel_spacing_range | /SAOCOM_XMLProduct/Channel/RasterInfo/LinesStep | 45.9972638425657 | Strongly RECOMMENDED for GRD |
| sar:pixel_spacing_azimuth | /SAOCOM_XMLProduct/Channel/RasterInfo/SamplesStep | 47.3467709207983 | Strongly RECOMMENDED for GRD |
| sar:looks_range |  |  |  |
| sar:looks_azimuth |  |  |  |
| sar:looks_equivalent_number |  | 3 | The equivalent number of looks (ENL). Ref: CONAE, Saocom-1 Level 1 Products Format, Jan 2020, Tab.3 Level 1 Products main characteristics.|
| geometry TLGC | /SAOCOM_XMLProduct/Channel/GroundCornerPoints/NorthWest | [51.3561111111111,123.819444444444] | Top Left Geodetic Coordinates. Extract Lon (first point val) Lat (second point val) in deg |
| geometry TRGC | /SAOCOM_XMLProduct/Channel/GroundCornerPoints/NorthEast | [51.3561111111111,129.868055555556] | Top Right Geodetic Coordinates. Extract Lon (first point val) Lat (second point val) in deg |
| geometry BRGC | /SAOCOM_XMLProduct/Channel/GroundCornerPoints/SouthEast | [46.2488888888889,129.868055555556] | Bottom Right Geodetic Coordinates. Extract Lon (first point val) Lat (second point val) in deg |
| geometry BLGC | /SAOCOM_XMLProduct/Channel/GroundCornerPoints/SouthWest | [46.2488888888889,123.819444444444] | Bottom Left Geodetic Coordinates. Extract Lon (first point val) Lat (second point val) in deg |