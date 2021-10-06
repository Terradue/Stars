# INPE CBERS-4 - L2 - PAN5M Camera

Ref1: http://www.cbers.inpe.br/sobre/cameras/cbers3-4.php

Product in View mode, PAN at 5m GSD

|  Field  | xpath or method  |  Example | Notes |
|---|---|---|---|
| item_id | /prdf/image/bandX | CBERS_4_PAN5M_20190510_027_076_L2_BAND4 | Remove path and extension from basename |
| mission | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| platform | /prdf/satellite/name & /prdf/satellite/number | "cbers-4" | |
| instruments | /prdf/satellite/instrument | "pan5m" | PAN5M |
| B1:name | | "band-1" | hardcoded, from /prdf/availableBands |
| B1:common_name | | "pan" | hardcoded |
| B1:center_wavelength | | 0.680 | hardcoded, 510 - 850 |