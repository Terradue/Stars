TMSP V4.11 Level 1b Product
---------------------------

The Linux variant of this processor is able to generate overlength ( > 4 GB ) MGD products.
If the MGD product data amount exceeds 4 GB the GeoTiff image file format automatically switches
from Tiff - 32bit internal pointers - to BigTiff - 64bit internal pointers -.

TMSP V4.10  Level 1b Product
---------------------------

The accuracy of the azimuth time annotation and hence the pixel localization accuracy is improved: 
A new azimuth time estimation algorithm overcomes the onboard time quantization of approx. 1/54000 s,
which corresponds to an azimuth location quantization of +/- 6.5 cm (max), 3.8 cm (1-sigma).
Pixel localizations accuracies of approx. +/- 1.5 cm have been demonstrated based on the algorithm.

For SSC type Sliding Spotlight (HS, SL) and Staring Spotlight (ST) level 1b products the
annotation of the Doppler centroid of the focused product, provided as 9 point grids, has been corrected. 

Now both variants, SE (spatially enhanced) and RE (radiometric enhanced), of detected 
Staring Spotlight level 1b products (MGD, GEC and EEC) are corrected for noise.

   Level 1b product noise correction application scheme:

   -----------------------------------------------------------------
   | Product     | Variant | Imaging Mode       | Noise Correction |
   -----------------------------------------------------------------
   | SSC         | --      | SC, SM, SL, HS, ST |       no         |
   | MGD/GEC/EEC | RE      | SC, SM, SL, HS, ST |       yes        |
   | MGD/GEC/EEC | SE      |     SM, SL, HS     |       no         |
   | MGD/GEC/EEC | SE      |                 ST |       yes        |
   -----------------------------------------------------------------

New maximum thresholds for amplitude scaling of 8-bit quicklooks improve quicklook interpretability.
8-bit quicklooks of low backscatter scenes now appear darked and avoid saturation of natural targets,
such as small islands surrounded by low backscatter ocean. 
This especially improves the appearance of 8-bit quicklooks of the individual channel of cross-pol (HV)
6 beam ScanSAR products.     


TMSP V4.8  Level 1b Product
---------------------------

For SSC type level 1b products the subdirectory ANNOTATION contains new additional files
which describe the Doppler centroid after focusing for each image file provided in subdirectory IMAGEDATA.
In case of ScanSAR mode within these files a description for each burst is provided in a vectorized form.

The noise correction applied to detected (MGD/GEC/EEC) ScanSAR products has been refined by adapting
the measured noise level to the ratio of noise measurement bandwidth to the beam specific processed range bandwidth.  

TMSP V4.7  Level 1b Product
---------------------------

For TerraSAR-X product specifications refer to the relevant documentation distributed 
by the commercial and science service segments since the start of the TerraSAR-X operational 
phase. 


TMSP V4.7 Release Remarks
-------------------------

The azimuth focussing now takes into account the instruments motion during the time needed to transmit the chirp
for different lengths of the chirp signals. Hence azimuth time tagging does not refer anymore to an average 
chirp length. This approach reduces the instruments constant "external azimuth time shift" in the GEOREF.xml file
to about -6.5cm (-4.8 TDX-1, -8.0 TSX-1) and results in a more accurate azimuth pixel location of 
better than 6cm (1 sigma) if all external and geophysical sources for shifts are corrected for.

Improvements have also been added for focussing of SAR data with extreme squints. In Spotlight modes the 
annotated azimuth FM-rate and velocity parameters are now given for each commanded azimuth beam.


TMSP V4.5 Release Remarks
-------------------------

1.: TSX-1 and TDX-1 products

Note that the product name (e.g. "TDX1_SAR_...") and the "mission" identifier in the XML
file (e.g. "TDX-1") indicates which instrument - TSX-1 or TDX-1 - acquired the data take. 
Nevertheless, the nominally generated level 1b products are all "TerraSAR-X mission" 
products.

2.: Phase pattern

An antenna phase pattern correction is applied to all complex SSC products which compensates an
instrument dependent antenna phase screen for different beams and polarization channels projected 
onto the DEM. This ensures the suitability for polarimetric InSAR and the interchangability of 
products generated from TSX-1 or TDX-1 data.

Value adding & processing of interferometric stacks of such corrected products and previously 
generated ones has to take this compensation into account. The (unprojected) phase patterns that were 
projected and subtracted are annotated in the products as new additional XML files e.g. as 
ANNOTATION/RFANTPAT_PHASE_VV_SRA_strip_005.xml which are referenced in the main annotation XML file
as "<annotation>" entry of type "OTHER".

The correction can thus be removed for each image layer by the experienced user with the help of the 
elevation angles annotated in the Geo-Grid XML file. See the antennaPhasePattern.xsd file in the 
SUPPORT directory for formatting details.

3.: Additional image files

All TMSP V4.5 level 1b products are supplemented by new auxiliary raster files including colour 
coded RGB quicklooks which represent the image statistics. The files are provided in the AUXRASTER
subdirectory and comprise

- the standard deviation (in 16 bit) of the full resolution image in quicklook sampling (STD_MRES_*.tif)
  for each beam and polarization. 
- an absolutely scaled "calibration preview" quicklook image corresponding to a very coarse sigma0 estimate in 
  only 8bit ("CAL_QL_MRES_*.tif) for each beam and polarization. Unlike the nominal "composite" quicklooks,
  this new image is not scaling with the image mean value and may hence be subject to saturation or fading. 
  The CAL_QL_MRES* pixel digital number (DN) values are thus to be converted to sigma0 with a fixed 
  "calibration" factor
  sigma0 = -46.7 dB + 10 log (DN*DN )
- the 24bit RGB image composed of scaled, weighted and calibrated statistic layers of all polarizations
  (and geometric DRA layers) combined, which visualizes the full resolution image speckle variations on 
  quicklook scales to enhance texture variations ("stdcalcomposite_MRES_*.tif"). It is provided for each
  ScanSAR beam separately.  

The coding and scaling of the latter is scene adaptive and NOT meant as a classification but intended to 
support visual interpretation of complex scenes. All these new medium resolution images are in GeoTIFF 
format (hence coarsely(!) georeferenced) and accompanied by KML files. They are referenced in the main 
XML files as "auxRasterFiles". The files are provided in slant-range ("SSC") geometry for geocoded / detected 
ScanSAR products and may also be used to identify beam borders and overlap regions in the combined swath.

4.: HS / SL noise profiles

Spotlight mode noise annotation derived from radiometric calibration and normalization information is now 
more precisely annotated. Note however the remarks on the radiometric calibration of spotlight products 
below.

5.: Azimuth timing accuracy improvement

The TMSP 4.5 has new reference points for instrument azimuth timing corrections. Some of the azimuth timing
shifts are now directly associated with timing delays and directly applied to the annotated instrument time. 
Hence the (external) azimuth timing offset parameter annotated in the geo grid file is significantly different
from previous ones. This does NOT correspond to a location or geometric shift by that amount but primarily to a 
different share of internal and external timing corrections. The absolute azimuth pixel location accuracy is 
thereby further improved by some centimetres. 

6.: DRA mode products

The operationally generated products in DRA Quad polarization and DRA ATI mode are still considered as experimental
products - hence not covered by the product specification. The DRA characterization campaigns yielded however
an excellent performance of the DRA mode data with geometric, radiometric and polarimetric characteristics very 
close to the ones specified for nominal products. Nevertheless, the processing of DRA data and the product
annotation content and structure - specifically the DRA parameter structure given in the calibration section - 
may still be subject to change. Please note the remarks on calibration below.

7.: Average Doppler centroid information

For spotlight modes, the averageDopplerCentroidInfo now contains the Doppler centroid of the focussed data at the
zero Doppler time of the indicated scene positions and not anymore the raw data Doppler centroid at these azimuth 
times and ranges. Note however that this parameter is still provided for informative purposes only and precise 
spectral processing of spotlight SSCs (e.g. for interferometric purposes) requires the accurate use of all the
annotated Doppler values at corresponding beam centre offset times considering the annotated bandwidths. 


----------
TMSP V4.3:

Noise correction is performed by the processor for a selected subset of product variants:
Currently all detected radiometrically enhanced products (MGD-RE, GEC-RE and EEC-RE) are
corrected for noise by default. This is always indicated by the flag 

   <processing>
      <processingFlags>
         <noiseCorrectedFlag>

within the L1b XML product annotation file.

If this flag is set to "true" no subtraction of annotated noise power (thus NEBN)
has to be performed by the user for radiometric calibration.

------------------------------------------------------------

General Limitations:
--------------------

- Product calibration is not applicable if the products are not
  explicitly flagged as "CALIBRATED". However, the Spotlight product calibration is only 
  derived from the Stripmap mode. Experimental products are not calibrated even if flagged as 
  "CALIBRATED". Left looking data is flagged as calibrated since all processing corrections
  are performed and a calibration constant is provided - however, performance degradations
  can not be ruled out and left looking products were not subject to the SAR product
  verification activities.

Pre-Operational Products:
-------------------------

Pre-operational products are distributed for test purposes only. Those products are 
provided "as is". No warranty of any kind given. Performance of pre-operational products 
may be different from operational TerraSAR-X products. Content and format have been 
subject to change prior to the ground segments "Operational Readiness Review". 

