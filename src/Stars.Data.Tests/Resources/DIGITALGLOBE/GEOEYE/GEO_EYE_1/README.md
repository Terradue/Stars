# DigitalGlobe GeoEye-1 - L1B Corrected Product Multispectral
 
The XML metadata file is under the vendor_meatadata folder.
 
GeoEye-1 Calibration Adjustment Factors updated on 6/6/2017 (Source: DigitalGlobe)

Source:

https://dg-cms-uploads-production.s3.amazonaws.com/uploads/document/file/209/ABSRADCAL_FLEET_2016v0_Rel20170606.pdf

ESUN from Thuillier (Source: DigitalGlobe)
 
Note: this product is multispectral, however Pan should be also provided as separated product. Metadata shall be similar except for: instruments="pan", PAN:common_name=”pan”, PAN:center_wavelength=0.625 (450-800nm), PAN:full_width_half_max=0.35, PAN:abs_scaling_factor=??? (to be extracted from XML metadata from the ABSCALFACTOR), PAN:gain=0.97, PAN:offset=-1.926, PAN:ESUN=1610.73.


Radiances are calculated using the following formula:

**_𝐿 = 𝐺𝐴𝐼𝑁 * 𝐷𝑁 * (𝑎𝑏𝑠𝑐𝑎𝑙𝑓𝑎𝑐𝑡𝑜𝑟 / 𝑒𝑓𝑓𝑒𝑐𝑡𝑖𝑣𝑒𝑏𝑎𝑛𝑑𝑤𝑖𝑡ℎ) + 𝑂𝐹𝐹𝑆𝐸𝑇_**


|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /isd/TIL/TILE/FILENAME | 21MAR18021224-M1BS-505570424020_01_P001 | Remove extension. |
| agency | | "DigitalGlobe" | |
| mission | /isd/IMD/IMAGE/SATID | "geoeye-1” | “GEO1” means GeoEye-1 |
| platform | /isd/IMD/IMAGE/SATID | "geoeye-1" | “GEO1” means GeoEye-1 |
| instruments | | "msi" | |
| sat:orbit_state | | | |
| sat:absolute_orbit | /isd/IMD/IMAGE/REVNUMBER | 66958 | |
| view:sun_elevation | /isd/IMD/IMAGE/MEANSUNEL | 39.1 | in degrees |
| view:sun_azimuth | /isd/IMD/IMAGE/MEANSUNAZ | 156.6 | in degrees |
| view:incidence_angle | /isd/IMD/IMAGE/MEANOFFNADIRVIEWANGLE | 23.7 | in degrees |
| view:off_nadir | | | |
| processing:level | /isd/IMD/PRODUCTLEVEL | L1B | |
| gsd | /isd/IMD/IMAGE/MEANPRODUCTGSD  | 1.934 | in meters |
| bit_depth | /isd/IMD/BITSPERPIXEL | 16 | |
| B1:name | /isd/IMD/BAND_B | "BAND_B" | |
| B1:common_name | | "blue" | hardcoded |
| B1:center_wavelength | | 0.480 | hardcoded, 450 - 510 nm |
| B1:full_width_half_max | /isd/IMD/BAND_B/EFFECTIVEBANDWIDTH | 0.06 | Bandwidth in μm |
| B1:abs_scaling_factor | /isd/IMD/BAND_B/ABSCALFACTOR | 8.919000000000000e-03 | |
| B1:gain | | 1.053 | The gain to be passed to OTB calibration tool is 1/(B1:gain*B1:abs_scaling_factor/B1:full_width_half_max) |
| B1:offset | | -4.537 | |
| B1:EAI | | 1993.18 | Esun(λ) in mW/cm2/μm |
| B2:name | /isd/IMD/BAND_G | "BAND_G" | |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.545 | hardcoded, 510 - 580 nm |
| B2:full_width_half_max | /isd/IMD/BAND_G/EFFECTIVEBANDWIDTH | 0.07 | Bandwidth in μm |
| B2:abs_scaling_factor | /isd/IMD/BAND_G/ABSCALFACTOR | 7.094500000000000e-03 | |
| B2:gain | | 0.994 | The gain to be passed to OTB calibration tool is 1/(B2:gain*B2:abs_scaling_factor/B2:full_width_half_max) |
| B2:offset | | -4.175 | |
| B2:EAI | | 1828.83 | Esun(λ) in mW/cm2/μm |
| B3:name | /isd/IMD/BAND_R | "BAND_R" | |
| B3:common_name | | "red" | hardcoded |
| B3:center_wavelength | | 0.673 | hardcoded, 655 - 690 nm |
| B3:full_width_half_max | /isd/IMD/BAND_R/EFFECTIVEBANDWIDTH | 0.035 | Bandwidth in μm |
| B3:abs_scaling_factor | /isd/IMD/BAND_R/ABSCALFACTOR | 5.667901000000000e-03 | |
| B3:gain | | 0.998 | The gain to be passed to OTB calibration tool is 1/(B3:gain*B3:abs_scaling_factor/B3:full_width_half_max) |
| B3:offset | | -3.754 | |
| B3:EAI | | 1491.49 | Esun(λ) in mW/cm2/μm |
| B4:name | /isd/IMD/BAND_N | "BAND_N" | |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.85 | hardcoded, 780 - 920 nm |
| B4:full_width_half_max | /isd/IMD/BAND_N/EFFECTIVEBANDWIDTH | 0.14 | Bandwidth in μm |
| B4:abs_scaling_factor | /isd/IMD/BAND_N/ABSCALFACTOR | 7.986999999999999e-03 | |
| B4:gain | | 0.994 | The gain to be passed to OTB calibration tool is 1/(B3:gain*B4:abs_scaling_factor/B4:full_width_half_max)
| B4:offset | | -3.870 | |
| B4:EAI | | 1022.58 | Esun(λ) in mW/cm2/μm |
| created | /isd/IMD/GENERATIONTIME | 2021-AUG-06T17:01:55Z | format to ISO |
| start_datetime  | /isd/IMD/IMAGE/TLCTIME | 2021-MAR-18T02:12:24Z | format to ISO |
| end_datetime | | 2021-MAR-18T02:12:24Z | Same as start_datetime |
| proj:epsg | | | To be derived from gdal |
| proj:wkt2 | | | To be derived from epsg |
| geometry UL | /isd/TIL/TILE/ULLON , ULLAT  | [130.841111111111, 47.8238888888889]| Coordinates Type="Geographic" Units="Degrees |
| geometry UR | /isd/TIL/TILE/URLON , URLAT | [131.085277777778, 47.8238888888889] | Coordinates Type="Geographic" Units="Degrees |
| geometry LR | /isd/TIL/TILE/LRLON , LRLAT | [131.085277777778, 47.6491666666667] | Coordinates Type="Geographic" Units="Degrees |
| geometry LL | /isd/TIL/TILE/LLLON , LLLAT | [130.841111111111, 47.6491666666667] | Coordinates Type="Geographic" Units="Degrees |
