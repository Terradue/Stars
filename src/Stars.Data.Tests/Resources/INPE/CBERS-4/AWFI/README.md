# INPE CBERS-4 - L2 - AWFI Camera

Ref1: http://www.cbers.inpe.br/sobre/cameras/cbers3-4.php

Ref2: http://marte.sid.inpe.br/col/dpi.inpe.br/sbsr@80/2008/11.18.12.46/doc/2001-2008.pdf

Product in View mode given as 4-band 8-bit GeoTIFF with the following band order: 16 15 14 13 to make automatic false NIR RGB 1(nir)-2(red)-3(green). Use 2(red)-3(green)-4(blue) for natural color RGB.  

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /prdf/image/band5 | CBERS_4_AWFI_20201031_003_075_L2_BAND16151413 | Remove path and extension from basename |
| mission | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| platform | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| instruments | /prdf/satellite/instrument | "awfi" | AWFI |
| B13:name | | "band-13" | hardcoded, from /prdf/availableBands |
| B13:common_name | | "nir" | hardcoded |
| B13:center_wavelength | | 0.830 | hardcoded, 770 - 890 |
| B14:name | | "band-14" | hardcoded, from /prdf/availableBands |
| B14:common_name | | "red" | hardcoded |
| B14:center_wavelength | | 0.660 | hardcoded, 630 - 690 |
| B15:name | | "band-15" | hardcoded, from /prdf/availableBands |
| B15:common_name | | "green" | hardcoded |
| B15:center_wavelength | | 0.555 | hardcoded, 520 - 590 |
| B16:name | | "band-16" | derive from /prdf/availableBands |
| B16:common_name | | "blue" | hardcoded |
| B16:center_wavelength | | 0.485 | hardcoded, 450 - 520 |