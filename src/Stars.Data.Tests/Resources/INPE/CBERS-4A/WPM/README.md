# INPE CBERS-4A - L4 - WPM Camera

Ref1: http://www.cbers.inpe.br/sobre/cameras/cbers04a.php

Ref2: http://marte.sid.inpe.br/col/dpi.inpe.br/sbsr@80/2008/11.18.12.46/doc/2001-2008.pdf

Product in View mode given with PAN and 4 Multispectral bands.  

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /prdf/image/band5 | CBERS_4_WFI_20201031_003_075_L2_BAND16151413 | Remove path and extension from basename |
| mission | /prdf/satellite/name & /prdf/satellite/number | "cbers-4a" | |
| platform | /prdf/satellite/name & /prdf/satellite/number | "cbers-4a" | |
| instruments | /prdf/satellite/instrument | "wpm" | WPM |
| processing:level | /prdf/image/level | L4 | |

| B0:name | | "band-0" | hardcoded, from /prdf/availableBands |
| B0:common_name | | "pan" | hardcoded |
| B0:center_wavelength | | 0.830 | hardcoded, 770 - 890 |
| B0:gsd | | 2 | |

| B1:name | | "band-1" | derive from /prdf/availableBands |
| B1:common_name | | "blue" | hardcoded |
| B1:center_wavelength | | 0.485 | hardcoded, 450 - 520 |
| B2:gsd | | 8 | |

| B2:name | | "band-2" | hardcoded, from /prdf/availableBands |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.555 | hardcoded, 520 - 590 |
| B2:gsd | | 8 | |

| B3:name | | "band-3" | hardcoded, from /prdf/availableBands |
| B3:common_name | | "red" | hardcoded |
| B3:center_wavelength | | 0.660 | hardcoded, 630 - 690 |
| B3:gsd | | 8 | |

| B4:name | | "band-4" | hardcoded, from /prdf/availableBands |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.830 | hardcoded, 770 - 890 |
| B4:gsd | | 8 | |