using System.Xml.Serialization;

namespace Terradue.Stars.Data.Model.Metadata.Resursp
{
    [XmlRoot(ElementName = "Passport")]
    public class Passport
    {
        [XmlElement(ElementName = "cDeviceName")]
        public string CDeviceName { get; set; }

        [XmlElement(ElementName = "nBitsPerPixel")]
        public string NBitsPerPixel { get; set; }
    }

    [XmlRoot(ElementName = "Geographic")]
    public class Geographic
    {
        [XmlElement(ElementName = "aNWLat")] public string ANWLat { get; set; }
        [XmlElement(ElementName = "aNWLong")] public string ANWLong { get; set; }
        [XmlElement(ElementName = "aNELat")] public string ANELat { get; set; }
        [XmlElement(ElementName = "aNELong")] public string ANELong { get; set; }
        [XmlElement(ElementName = "aSELat")] public string ASELat { get; set; }
        [XmlElement(ElementName = "aSELong")] public string ASELong { get; set; }
        [XmlElement(ElementName = "aSWLat")] public string ASWLat { get; set; }
        [XmlElement(ElementName = "aSWLong")] public string ASWLong { get; set; }
        [XmlElement(ElementName = "aMidLat")] public string AMidLat { get; set; }
        [XmlElement(ElementName = "aMidLong")] public string AMidLong { get; set; }
    }

    [XmlRoot(ElementName = "CoordinateSystem")]
    public class CoordinateSystem
    {
        [XmlElement(ElementName = "cCoordSystName")]
        public string CCoordSystName { get; set; }

        [XmlElement(ElementName = "nCoordSystCode")]
        public string NCoordSystCode { get; set; }

        [XmlElement(ElementName = "cZoneName")]
        public string CZoneName { get; set; }

        [XmlElement(ElementName = "nZoneCode")]
        public string NZoneCode { get; set; }

        [XmlElement(ElementName = "cTrMethodName")]
        public string CTrMethodName { get; set; }

        [XmlElement(ElementName = "nTrMethodCode")]
        public string NTrMethodCode { get; set; }

        [XmlElement(ElementName = "cLUnitName")]
        public string CLUnitName { get; set; }

        [XmlElement(ElementName = "nLUnitCode")]
        public string NLUnitCode { get; set; }

        [XmlElement(ElementName = "nLUnitA")] public string NLUnitA { get; set; }

        [XmlElement(ElementName = "cAnUnitName")]
        public string CAnUnitName { get; set; }

        [XmlElement(ElementName = "nAnUnitCode")]
        public string NAnUnitCode { get; set; }

        [XmlElement(ElementName = "nAnUnitA")] public string NAnUnitA { get; set; }

        [XmlElement(ElementName = "nLonOfOrig")]
        public string NLonOfOrig { get; set; }

        [XmlElement(ElementName = "nLatOfOrig")]
        public string NLatOfOrig { get; set; }

        [XmlElement(ElementName = "nScAtOrig")]
        public string NScAtOrig { get; set; }

        [XmlElement(ElementName = "nFalsEast")]
        public string NFalsEast { get; set; }

        [XmlElement(ElementName = "nFalsNord")]
        public string NFalsNord { get; set; }

        [XmlElement(ElementName = "cGeoDatName")]
        public string CGeoDatName { get; set; }

        [XmlElement(ElementName = "nGeoDatCode")]
        public string NGeoDatCode { get; set; }

        [XmlElement(ElementName = "cPrMeridianName")]
        public string CPrMeridianName { get; set; }

        [XmlElement(ElementName = "nPrMeridianCode")]
        public string NPrMeridianCode { get; set; }

        [XmlElement(ElementName = "aGrinvValue")]
        public string AGrinvValue { get; set; }

        [XmlElement(ElementName = "cDatum_Name")]
        public string CDatum_Name { get; set; }

        [XmlElement(ElementName = "nDatum_Code")]
        public string NDatum_Code { get; set; }

        [XmlElement(ElementName = "nDX")] public string NDX { get; set; }
        [XmlElement(ElementName = "nDY")] public string NDY { get; set; }
        [XmlElement(ElementName = "nDZ")] public string NDZ { get; set; }
        [XmlElement(ElementName = "nWX")] public string NWX { get; set; }
        [XmlElement(ElementName = "nWY")] public string NWY { get; set; }
        [XmlElement(ElementName = "nWZ")] public string NWZ { get; set; }
        [XmlElement(ElementName = "nLScale")] public string NLScale { get; set; }

        [XmlElement(ElementName = "cEllipsoidName")]
        public string CEllipsoidName { get; set; }

        [XmlElement(ElementName = "nEllipsoidCode")]
        public string NEllipsoidCode { get; set; }

        [XmlElement(ElementName = "nSemiMajorAxis")]
        public string NSemiMajorAxis { get; set; }

        [XmlElement(ElementName = "nSemiMinorAxis")]
        public string NSemiMinorAxis { get; set; }

        [XmlElement(ElementName = "nInversFlatt")]
        public string NInversFlatt { get; set; }

        [XmlElement(ElementName = "cEllipsLUnitName")]
        public string CEllipsLUnitName { get; set; }

        [XmlElement(ElementName = "nEllipsLUnitCode")]
        public string NEllipsLUnitCode { get; set; }

        [XmlElement(ElementName = "nEllipsLUnitA")]
        public string NEllipsLUnitA { get; set; }
    }

    [XmlRoot(ElementName = "Normal")]
    public class Normal
    {
        [XmlElement(ElementName = "nBitsPerPixel")]
        public string NBitsPerPixel { get; set; }

        [XmlElement(ElementName = "nNChannel")]
        public string NNChannel { get; set; }

        [XmlElement(ElementName = "nSRMinChannel1")]
        public string NSRMinChannel1 { get; set; }

        [XmlElement(ElementName = "nSRMinChannel2")]
        public string NSRMinChannel2 { get; set; }

        [XmlElement(ElementName = "nSRMinChannel3")]
        public string NSRMinChannel3 { get; set; }

        [XmlElement(ElementName = "nSRMinChannel4")]
        public string NSRMinChannel4 { get; set; }

        [XmlElement(ElementName = "nSRMinChannel5")]
        public string NSRMinChannel5 { get; set; }

        [XmlElement(ElementName = "nSRMaxChannel1")]
        public string NSRMaxChannel1 { get; set; }

        [XmlElement(ElementName = "nSRMaxChannel2")]
        public string NSRMaxChannel2 { get; set; }

        [XmlElement(ElementName = "nSRMaxChannel3")]
        public string NSRMaxChannel3 { get; set; }

        [XmlElement(ElementName = "nSRMaxChannel4")]
        public string NSRMaxChannel4 { get; set; }

        [XmlElement(ElementName = "nSRMaxChannel5")]
        public string NSRMaxChannel5 { get; set; }

        [XmlElement(ElementName = "nWidth")] public string NWidth { get; set; }
        [XmlElement(ElementName = "nHeight")] public string NHeight { get; set; }

        [XmlElement(ElementName = "dSceneDate")]
        public string DSceneDate { get; set; }

        [XmlElement(ElementName = "tSceneTime")]
        public string TSceneTime { get; set; }

        [XmlElement(ElementName = "nDeltaTime")]
        public string NDeltaTime { get; set; }

        [XmlElement(ElementName = "aSunAzim")] public string ASunAzim { get; set; }

        [XmlElement(ElementName = "aSunElevC")]
        public string ASunElevC { get; set; }

        [XmlElement(ElementName = "aAngleSum")]
        public string AAngleSum { get; set; }

        [XmlElement(ElementName = "aAzimutScan")]
        public string AAzimutScan { get; set; }

        [XmlElement(ElementName = "nPixelImgSrc")]
        public string NPixelImgSrc { get; set; }

        [XmlElement(ElementName = "nPixelImg")]
        public string NPixelImg { get; set; }

        [XmlElement(ElementName = "nAlev")] public string NAlev { get; set; }
        [XmlElement(ElementName = "nRMSAlev")] public string NRMSAlev { get; set; }
        [XmlElement(ElementName = "cLevel")] public string CLevel { get; set; }

        [XmlElement(ElementName = "cRadiometric")]
        public string CRadiometric { get; set; }

        [XmlElement(ElementName = "cInterpol")]
        public string CInterpol { get; set; }

        [XmlElement(ElementName = "nLUpNord")] public string NLUpNord { get; set; }
        [XmlElement(ElementName = "nLUpEast")] public string NLUpEast { get; set; }
        [XmlElement(ElementName = "nRUpNord")] public string NRUpNord { get; set; }
        [XmlElement(ElementName = "nRUpEast")] public string NRUpEast { get; set; }

        [XmlElement(ElementName = "nRDownNord")]
        public string NRDownNord { get; set; }

        [XmlElement(ElementName = "nRDownEast")]
        public string NRDownEast { get; set; }

        [XmlElement(ElementName = "nLDownNord")]
        public string NLDownNord { get; set; }

        [XmlElement(ElementName = "nLDownEast")]
        public string NLDownEast { get; set; }
    }

    [XmlRoot(ElementName = "AbsoluteCalibr")]
    public class AbsoluteCalibr
    {
        [XmlElement(ElementName = "cFormula")]
        public string CFormula { get; set; }
        [XmlElement(ElementName = "nDone")]
        public string NDone { get; set; }
        [XmlElement(ElementName = "bMult")]
        public string BMult { get; set; }
        [XmlElement(ElementName = "bAdd")]
        public string BAdd { get; set; }
    }

    [XmlRoot(ElementName = "SPP_ROOT")]
    public class SPP_ROOT
    {
        [XmlElement(ElementName = "Version")] public string Version { get; set; }

        [XmlElement(ElementName = "nNumberKA")]
        public string NNumberKA { get; set; }

        [XmlElement(ElementName = "cCodeKA")] public string CCodeKA { get; set; }
        [XmlElement(ElementName = "cCodePPI")] public string CCodePPI { get; set; }

        [XmlElement(ElementName = "dDateHeaderFile")]
        public string DDateHeaderFile { get; set; }

        [XmlElement(ElementName = "cOrganization")]
        public string COrganization { get; set; }

        [XmlElement(ElementName = "cProgramm")]
        public string CProgramm { get; set; }

        [XmlElement(ElementName = "cFormatNamePassport")]
        public string CFormatNamePassport { get; set; }

        [XmlElement(ElementName = "cDataFileName")]
        public string CDataFileName { get; set; }

        [XmlElement(ElementName = "cFileName_quicklook")]
        public string CFileName_quicklook { get; set; }

        [XmlElement(ElementName = "Passport")] public Passport Passport { get; set; }

        [XmlElement(ElementName = "Geographic")]
        public Geographic Geographic { get; set; }

        [XmlElement(ElementName = "CoordinateSystem")]
        public CoordinateSystem CoordinateSystem { get; set; }

        [XmlElement(ElementName = "Normal")]
        public Normal Normal { get; set; }

        [XmlElement(ElementName = "AbsoluteCalibr")]
        public AbsoluteCalibr AbsoluteCalibr { get; set; }
    }
}
