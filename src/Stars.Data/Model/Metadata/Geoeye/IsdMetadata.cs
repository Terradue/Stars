using System.Collections.Generic;
using System.Xml.Serialization;


namespace Terradue.Stars.Data.Model.Metadata.Geoeye
{
    [XmlRoot(ElementName = "BAND")]
    public class BAND
    {
        [XmlElement(ElementName = "ULLON")] public string ULLON { get; set; }
        [XmlElement(ElementName = "ULLAT")] public string ULLAT { get; set; }
        [XmlElement(ElementName = "ULHAE")] public string ULHAE { get; set; }
        [XmlElement(ElementName = "URLON")] public string URLON { get; set; }
        [XmlElement(ElementName = "URLAT")] public string URLAT { get; set; }
        [XmlElement(ElementName = "URHAE")] public string URHAE { get; set; }
        [XmlElement(ElementName = "LRLON")] public string LRLON { get; set; }
        [XmlElement(ElementName = "LRLAT")] public string LRLAT { get; set; }
        [XmlElement(ElementName = "LRHAE")] public string LRHAE { get; set; }
        [XmlElement(ElementName = "LLLON")] public string LLLON { get; set; }
        [XmlElement(ElementName = "LLLAT")] public string LLLAT { get; set; }
        [XmlElement(ElementName = "LLHAE")] public string LLHAE { get; set; }

        [XmlElement(ElementName = "ABSCALFACTOR")]
        public double ABSCALFACTOR { get; set; }

        [XmlElement(ElementName = "EFFECTIVEBANDWIDTH")]
        public double EFFECTIVEBANDWIDTH { get; set; }

        [XmlElement(ElementName = "TDILEVEL")] public string TDILEVEL { get; set; }
        [XmlElement(ElementName = "BANDID")] public string BANDID { get; set; }

        [XmlElement(ElementName = "DETECTOR_ARRAY")]
        public DETECTOR_ARRAY DETECTOR_ARRAY { get; set; }
    }

    [XmlRoot(ElementName = "TLCLISTList")]
    public class TLCLISTList
    {
        [XmlElement(ElementName = "TLCLIST")] public List<string> TLCLIST { get; set; }
    }

    [XmlRoot(ElementName = "IMAGE")]
    public class IMAGE
    {
        [XmlElement(ElementName = "SATID")] public string SATID { get; set; }
        [XmlElement(ElementName = "MODE")] public string MODE { get; set; }

        [XmlElement(ElementName = "SCANDIRECTION")]
        public string SCANDIRECTION { get; set; }

        [XmlElement(ElementName = "CATID")] public string CATID { get; set; }
        [XmlElement(ElementName = "TLCTIME")] public string TLCTIME { get; set; }
        [XmlElement(ElementName = "NUMTLC")] public string NUMTLC { get; set; }

        [XmlElement(ElementName = "TLCLISTList")]
        public TLCLISTList TLCLISTList { get; set; }

        [XmlElement(ElementName = "FIRSTLINETIME")]
        public string FIRSTLINETIME { get; set; }

        [XmlElement(ElementName = "AVGLINERATE")]
        public string AVGLINERATE { get; set; }

        [XmlElement(ElementName = "EXPOSUREDURATION")]
        public string EXPOSUREDURATION { get; set; }

        [XmlElement(ElementName = "MINCOLLECTEDROWGSD")]
        public string MINCOLLECTEDROWGSD { get; set; }

        [XmlElement(ElementName = "MAXCOLLECTEDROWGSD")]
        public string MAXCOLLECTEDROWGSD { get; set; }

        [XmlElement(ElementName = "MEANCOLLECTEDROWGSD")]
        public string MEANCOLLECTEDROWGSD { get; set; }

        [XmlElement(ElementName = "MINCOLLECTEDCOLGSD")]
        public string MINCOLLECTEDCOLGSD { get; set; }

        [XmlElement(ElementName = "MAXCOLLECTEDCOLGSD")]
        public string MAXCOLLECTEDCOLGSD { get; set; }

        [XmlElement(ElementName = "MEANCOLLECTEDCOLGSD")]
        public string MEANCOLLECTEDCOLGSD { get; set; }

        [XmlElement(ElementName = "MEANCOLLECTEDGSD")]
        public string MEANCOLLECTEDGSD { get; set; }

        [XmlElement(ElementName = "MEANPRODUCTROWGSD")]
        public string MEANPRODUCTROWGSD { get; set; }

        [XmlElement(ElementName = "MEANPRODUCTCOLGSD")]
        public string MEANPRODUCTCOLGSD { get; set; }

        [XmlElement(ElementName = "MEANPRODUCTGSD")]
        public string MEANPRODUCTGSD { get; set; }

        [XmlElement(ElementName = "ROWUNCERTAINTY")]
        public string ROWUNCERTAINTY { get; set; }

        [XmlElement(ElementName = "COLUNCERTAINTY")]
        public string COLUNCERTAINTY { get; set; }

        [XmlElement(ElementName = "MINSUNAZ")] public string MINSUNAZ { get; set; }
        [XmlElement(ElementName = "MAXSUNAZ")] public string MAXSUNAZ { get; set; }

        [XmlElement(ElementName = "MEANSUNAZ")]
        public string MEANSUNAZ { get; set; }

        [XmlElement(ElementName = "MINSUNEL")] public string MINSUNEL { get; set; }
        [XmlElement(ElementName = "MAXSUNEL")] public string MAXSUNEL { get; set; }

        [XmlElement(ElementName = "MEANSUNEL")]
        public string MEANSUNEL { get; set; }

        [XmlElement(ElementName = "MINSATAZ")] public string MINSATAZ { get; set; }
        [XmlElement(ElementName = "MAXSATAZ")] public string MAXSATAZ { get; set; }

        [XmlElement(ElementName = "MEANSATAZ")]
        public string MEANSATAZ { get; set; }

        [XmlElement(ElementName = "MINSATEL")] public string MINSATEL { get; set; }
        [XmlElement(ElementName = "MAXSATEL")] public string MAXSATEL { get; set; }

        [XmlElement(ElementName = "MEANSATEL")]
        public string MEANSATEL { get; set; }

        [XmlElement(ElementName = "MININTRACKVIEWANGLE")]
        public string MININTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MAXINTRACKVIEWANGLE")]
        public string MAXINTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MEANINTRACKVIEWANGLE")]
        public string MEANINTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MINCROSSTRACKVIEWANGLE")]
        public string MINCROSSTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MAXCROSSTRACKVIEWANGLE")]
        public string MAXCROSSTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MEANCROSSTRACKVIEWANGLE")]
        public string MEANCROSSTRACKVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MINOFFNADIRVIEWANGLE")]
        public string MINOFFNADIRVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MAXOFFNADIRVIEWANGLE")]
        public string MAXOFFNADIRVIEWANGLE { get; set; }

        [XmlElement(ElementName = "MEANOFFNADIRVIEWANGLE")]
        public string MEANOFFNADIRVIEWANGLE { get; set; }

        [XmlElement(ElementName = "PNIIRS")] public string PNIIRS { get; set; }

        [XmlElement(ElementName = "CLOUDCOVER")]
        public string CLOUDCOVER { get; set; }

        [XmlElement(ElementName = "RESAMPLINGKERNEL")]
        public string RESAMPLINGKERNEL { get; set; }

        [XmlElement(ElementName = "POSITIONKNOWLEDGESRC")]
        public string POSITIONKNOWLEDGESRC { get; set; }

        [XmlElement(ElementName = "ATTITUDEKNOWLEDGESRC")]
        public string ATTITUDEKNOWLEDGESRC { get; set; }

        [XmlElement(ElementName = "REVNUMBER")]
        public string REVNUMBER { get; set; }

        [XmlElement(ElementName = "ERRBIAS")] public string ERRBIAS { get; set; }
        [XmlElement(ElementName = "ERRRAND")] public string ERRRAND { get; set; }

        [XmlElement(ElementName = "LINEOFFSET")]
        public string LINEOFFSET { get; set; }

        [XmlElement(ElementName = "SAMPOFFSET")]
        public string SAMPOFFSET { get; set; }

        [XmlElement(ElementName = "LATOFFSET")]
        public string LATOFFSET { get; set; }

        [XmlElement(ElementName = "LONGOFFSET")]
        public string LONGOFFSET { get; set; }

        [XmlElement(ElementName = "HEIGHTOFFSET")]
        public string HEIGHTOFFSET { get; set; }

        [XmlElement(ElementName = "LINESCALE")]
        public string LINESCALE { get; set; }

        [XmlElement(ElementName = "SAMPSCALE")]
        public string SAMPSCALE { get; set; }

        [XmlElement(ElementName = "LATSCALE")] public string LATSCALE { get; set; }

        [XmlElement(ElementName = "LONGSCALE")]
        public string LONGSCALE { get; set; }

        [XmlElement(ElementName = "HEIGHTSCALE")]
        public string HEIGHTSCALE { get; set; }

        [XmlElement(ElementName = "LINENUMCOEFList")]
        public LINENUMCOEFList LINENUMCOEFList { get; set; }

        [XmlElement(ElementName = "LINEDENCOEFList")]
        public LINEDENCOEFList LINEDENCOEFList { get; set; }

        [XmlElement(ElementName = "SAMPNUMCOEFList")]
        public SAMPNUMCOEFList SAMPNUMCOEFList { get; set; }

        [XmlElement(ElementName = "SAMPDENCOEFList")]
        public SAMPDENCOEFList SAMPDENCOEFList { get; set; }
    }

    [XmlRoot(ElementName = "IMD")]
    public class IMD
    {
        [XmlElement(ElementName = "VERSION")] public string VERSION { get; set; }

        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "PRODUCTORDERID")]
        public string PRODUCTORDERID { get; set; }

        [XmlElement(ElementName = "PRODUCTCATALOGID")]
        public string PRODUCTCATALOGID { get; set; }

        [XmlElement(ElementName = "IMAGEDESCRIPTOR")]
        public string IMAGEDESCRIPTOR { get; set; }

        [XmlElement(ElementName = "BANDID")] public string BANDID { get; set; }

        [XmlElement(ElementName = "PANSHARPENALGORITHM")]
        public string PANSHARPENALGORITHM { get; set; }

        [XmlElement(ElementName = "NUMROWS")] public string NUMROWS { get; set; }

        [XmlElement(ElementName = "NUMCOLUMNS")]
        public string NUMCOLUMNS { get; set; }

        [XmlElement(ElementName = "PRODUCTLEVEL")]
        public string PRODUCTLEVEL { get; set; }

        [XmlElement(ElementName = "PRODUCTTYPE")]
        public string PRODUCTTYPE { get; set; }

        [XmlElement(ElementName = "NUMBEROFLOOKS")]
        public string NUMBEROFLOOKS { get; set; }

        [XmlElement(ElementName = "RADIOMETRICLEVEL")]
        public string RADIOMETRICLEVEL { get; set; }

        [XmlElement(ElementName = "BITSPERPIXEL")]
        public string BITSPERPIXEL { get; set; }

        [XmlElement(ElementName = "COMPRESSIONTYPE")]
        public string COMPRESSIONTYPE { get; set; }

        [XmlElement(ElementName = "JPEGPROFILENAME")]
        public string JPEGPROFILENAME { get; set; }

        [XmlElement(ElementName = "OUTPUTFORMAT")]
        public string OUTPUTFORMAT { get; set; }

        [XmlElement(ElementName = "BAND_P")] public BAND BAND_P { get; set; }

        [XmlElement(ElementName = "BAND_C")] public BAND BAND_C { get; set; }

        [XmlElement(ElementName = "BAND_B")] public BAND BAND_B { get; set; }

        [XmlElement(ElementName = "BAND_G")] public BAND BAND_G { get; set; }

        [XmlElement(ElementName = "BAND_Y")] public BAND BAND_Y { get; set; }

        [XmlElement(ElementName = "BAND_R")] public BAND BAND_R { get; set; }

        [XmlElement(ElementName = "BAND_RE")] public BAND BAND_RE { get; set; }

        [XmlElement(ElementName = "BAND_N")] public BAND BAND_N { get; set; }

        [XmlElement(ElementName = "BAND_N2")] public BAND BAND_N2 { get; set; }

        [XmlElement(ElementName = "IMAGE")] public IMAGE IMAGE { get; set; }
    }

    [XmlRoot(ElementName = "EPHEMLISTList")]
    public class EPHEMLISTList
    {
        [XmlElement(ElementName = "EPHEMLIST")]
        public List<string> EPHEMLIST { get; set; }
    }

    [XmlRoot(ElementName = "EPH")]
    public class EPH
    {
        [XmlElement(ElementName = "SATID")] public string SATID { get; set; }

        [XmlElement(ElementName = "REVNUMBER")]
        public string REVNUMBER { get; set; }

        [XmlElement(ElementName = "STRIPID")] public string STRIPID { get; set; }
        [XmlElement(ElementName = "TYPE")] public string TYPE { get; set; }
        [XmlElement(ElementName = "VERSION")] public string VERSION { get; set; }

        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "STARTTIME")]
        public string STARTTIME { get; set; }

        [XmlElement(ElementName = "NUMPOINTS")]
        public string NUMPOINTS { get; set; }

        [XmlElement(ElementName = "TIMEINTERVAL")]
        public string TIMEINTERVAL { get; set; }

        [XmlElement(ElementName = "EPHEMLISTList")]
        public EPHEMLISTList EPHEMLISTList { get; set; }
    }

    [XmlRoot(ElementName = "ATTLISTList")]
    public class ATTLISTList
    {
        [XmlElement(ElementName = "ATTLIST")] public List<string> ATTLIST { get; set; }
    }

    [XmlRoot(ElementName = "ATT")]
    public class ATT
    {
        [XmlElement(ElementName = "SATID")] public string SATID { get; set; }

        [XmlElement(ElementName = "REVNUMBER")]
        public string REVNUMBER { get; set; }

        [XmlElement(ElementName = "STRIPID")] public string STRIPID { get; set; }
        [XmlElement(ElementName = "TYPE")] public string TYPE { get; set; }
        [XmlElement(ElementName = "VERSION")] public string VERSION { get; set; }

        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "STARTTIME")]
        public string STARTTIME { get; set; }

        [XmlElement(ElementName = "NUMPOINTS")]
        public string NUMPOINTS { get; set; }

        [XmlElement(ElementName = "TIMEINTERVAL")]
        public string TIMEINTERVAL { get; set; }

        [XmlElement(ElementName = "ATTLISTList")]
        public ATTLISTList ATTLISTList { get; set; }
    }

    [XmlRoot(ElementName = "TILE")]
    public class TILE
    {
        [XmlElement(ElementName = "FILENAME")] public string FILENAME { get; set; }

        [XmlElement(ElementName = "ULCOLOFFSET")]
        public string ULCOLOFFSET { get; set; }

        [XmlElement(ElementName = "ULROWOFFSET")]
        public string ULROWOFFSET { get; set; }

        [XmlElement(ElementName = "URCOLOFFSET")]
        public string URCOLOFFSET { get; set; }

        [XmlElement(ElementName = "URROWOFFSET")]
        public string URROWOFFSET { get; set; }

        [XmlElement(ElementName = "LRCOLOFFSET")]
        public string LRCOLOFFSET { get; set; }

        [XmlElement(ElementName = "LRROWOFFSET")]
        public string LRROWOFFSET { get; set; }

        [XmlElement(ElementName = "LLCOLOFFSET")]
        public string LLCOLOFFSET { get; set; }

        [XmlElement(ElementName = "LLROWOFFSET")]
        public string LLROWOFFSET { get; set; }

        [XmlElement(ElementName = "ULLON")] public string ULLON { get; set; }
        [XmlElement(ElementName = "ULLAT")] public string ULLAT { get; set; }
        [XmlElement(ElementName = "URLON")] public string URLON { get; set; }
        [XmlElement(ElementName = "URLAT")] public string URLAT { get; set; }
        [XmlElement(ElementName = "LRLON")] public string LRLON { get; set; }
        [XmlElement(ElementName = "LRLAT")] public string LRLAT { get; set; }
        [XmlElement(ElementName = "LLLON")] public string LLLON { get; set; }
        [XmlElement(ElementName = "LLLAT")] public string LLLAT { get; set; }
    }

    [XmlRoot(ElementName = "TIL")]
    public class TIL
    {
        [XmlElement(ElementName = "BANDID")] public string BANDID { get; set; }
        [XmlElement(ElementName = "NUMTILES")] public string NUMTILES { get; set; }

        [XmlElement(ElementName = "TILESIZEX")]
        public int TILESIZEX { get; set; }

        [XmlElement(ElementName = "TILESIZEY")]
        public int TILESIZEY { get; set; }

        [XmlElement(ElementName = "TILEUNITS")]
        public string TILEUNITS { get; set; }

        [XmlElement(ElementName = "TILEOVERLAP")]
        public string TILEOVERLAP { get; set; }

        [XmlElement(ElementName = "TILE")] public TILE TILE { get; set; }
    }

    [XmlRoot(ElementName = "PRINCIPAL_DISTANCE")]
    public class PRINCIPAL_DISTANCE
    {
        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "PD")] public string PD { get; set; }
    }

    [XmlRoot(ElementName = "OPTICAL_DISTORTION")]
    public class OPTICAL_DISTORTION
    {
        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "POLYORDER")]
        public string POLYORDER { get; set; }

        [XmlElement(ElementName = "ALISTList")]
        public string ALISTList { get; set; }

        [XmlElement(ElementName = "BLISTList")]
        public string BLISTList { get; set; }
    }

    [XmlRoot(ElementName = "PERSPECTIVE_CENTER")]
    public class PERSPECTIVE_CENTER
    {
        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "CX")] public string CX { get; set; }
        [XmlElement(ElementName = "CY")] public string CY { get; set; }
        [XmlElement(ElementName = "CZ")] public string CZ { get; set; }
    }

    [XmlRoot(ElementName = "CAMERA_ATTITUDE")]
    public class CAMERA_ATTITUDE
    {
        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "QCS1")] public string QCS1 { get; set; }
        [XmlElement(ElementName = "QCS2")] public string QCS2 { get; set; }
        [XmlElement(ElementName = "QCS3")] public string QCS3 { get; set; }
        [XmlElement(ElementName = "QCS4")] public string QCS4 { get; set; }
    }

    [XmlRoot(ElementName = "DETECTOR_ARRAY")]
    public class DETECTOR_ARRAY
    {
        [XmlElement(ElementName = "DETARRID")] public string DETARRID { get; set; }

        [XmlElement(ElementName = "DETORIGINX")]
        public string DETORIGINX { get; set; }

        [XmlElement(ElementName = "DETORIGINY")]
        public string DETORIGINY { get; set; }

        [XmlElement(ElementName = "DETROTANGLE")]
        public string DETROTANGLE { get; set; }

        [XmlElement(ElementName = "DETPITCH")] public string DETPITCH { get; set; }
    }

    [XmlRoot(ElementName = "DETECTOR_MOUNTING")]
    public class DETECTOR_MOUNTING
    {
        [XmlElement(ElementName = "GENERATIONTIME")]
        public string GENERATIONTIME { get; set; }

        [XmlElement(ElementName = "BAND_P")] public BAND BAND_P { get; set; }
    }

    [XmlRoot(ElementName = "GEO")]
    public class GEO
    {
        [XmlElement(ElementName = "EFFECTIVETIME")]
        public string EFFECTIVETIME { get; set; }

        [XmlElement(ElementName = "MODELGENERATIONTIME")]
        public string MODELGENERATIONTIME { get; set; }

        [XmlElement(ElementName = "SATID")] public string SATID { get; set; }

        [XmlElement(ElementName = "GEOMODELLEVEL")]
        public string GEOMODELLEVEL { get; set; }

        [XmlElement(ElementName = "PRINCIPAL_DISTANCE")]
        public PRINCIPAL_DISTANCE PRINCIPAL_DISTANCE { get; set; }

        [XmlElement(ElementName = "OPTICAL_DISTORTION")]
        public OPTICAL_DISTORTION OPTICAL_DISTORTION { get; set; }

        [XmlElement(ElementName = "PERSPECTIVE_CENTER")]
        public PERSPECTIVE_CENTER PERSPECTIVE_CENTER { get; set; }

        [XmlElement(ElementName = "CAMERA_ATTITUDE")]
        public CAMERA_ATTITUDE CAMERA_ATTITUDE { get; set; }

        [XmlElement(ElementName = "DETECTOR_MOUNTING")]
        public DETECTOR_MOUNTING DETECTOR_MOUNTING { get; set; }
    }

    [XmlRoot(ElementName = "LINENUMCOEFList")]
    public class LINENUMCOEFList
    {
        [XmlElement(ElementName = "LINENUMCOEF")]
        public string LINENUMCOEF { get; set; }
    }

    [XmlRoot(ElementName = "LINEDENCOEFList")]
    public class LINEDENCOEFList
    {
        [XmlElement(ElementName = "LINEDENCOEF")]
        public string LINEDENCOEF { get; set; }
    }

    [XmlRoot(ElementName = "SAMPNUMCOEFList")]
    public class SAMPNUMCOEFList
    {
        [XmlElement(ElementName = "SAMPNUMCOEF")]
        public string SAMPNUMCOEF { get; set; }
    }

    [XmlRoot(ElementName = "SAMPDENCOEFList")]
    public class SAMPDENCOEFList
    {
        [XmlElement(ElementName = "SAMPDENCOEF")]
        public string SAMPDENCOEF { get; set; }
    }

    [XmlRoot(ElementName = "RPB")]
    public class RPB
    {
        [XmlElement(ElementName = "SATID")] public string SATID { get; set; }
        [XmlElement(ElementName = "BANDID")] public string BANDID { get; set; }
        [XmlElement(ElementName = "SPECID")] public string SPECID { get; set; }
        [XmlElement(ElementName = "IMAGE")] public IMAGE IMAGE { get; set; }
    }

    [XmlRoot(ElementName = "isd")]
    public class Isd
    {
        [XmlElement(ElementName = "IMD")] public IMD IMD { get; set; }
        [XmlElement(ElementName = "EPH")] public EPH EPH { get; set; }
        [XmlElement(ElementName = "ATT")] public ATT ATT { get; set; }
        [XmlElement(ElementName = "TIL")] public TIL TIL { get; set; }
        [XmlElement(ElementName = "GEO")] public GEO GEO { get; set; }
        [XmlElement(ElementName = "RPB")] public RPB RPB { get; set; }
    }
}
