# INPE CBERS-4 - L2 - PAN10 Camera

Ref1: http://www.cbers.inpe.br/sobre/cameras/cbers3-4.php

Product in View mode, RED, Green and NIR bands at 10m GSD

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /prdf/image/bandX | CBERS_4_PAN10M_20190510_027_076_L2_BAND4 | Remove path and extension from basename |
| mission | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| platform | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| instruments | /prdf/satellite/instrument | "pan10m" | PAN10M |
| B2:name | | "band-2" | hardcoded, from /prdf/availableBands |
| B2:common_name | | "green" | hardcoded |
| B2:center_wavelength | | 0.555 | hardcoded, 520 - 590 |
| B3:name | | "band-3" | hardcoded, from /prdf/availableBands |
| B3:common_name | | "red" | hardcoded |
| B3:center_wavelength | | 0.660 | hardcoded, 630 - 690 |
| B4:name | | "band-4" | hardcoded, from /prdf/availableBands |
| B4:common_name | | "nir" | hardcoded |
| B4:center_wavelength | | 0.830 | hardcoded, 770 - 890