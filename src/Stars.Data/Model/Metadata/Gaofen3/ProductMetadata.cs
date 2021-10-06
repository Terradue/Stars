using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Terradue.Stars.Data.Model.Metadata.Gaofen3 
{
	[XmlRoot(ElementName="valueMGC")]
	public class ValueMGC {
		[XmlElement(ElementName="HH")]
		public string HH { get; set; }
		[XmlElement(ElementName="HV")]
		public string HV { get; set; }
		[XmlElement(ElementName="VH")]
		public string VH { get; set; }
		[XmlElement(ElementName="VV")]
		public string VV { get; set; }
	}

	[XmlRoot(ElementName="wave")]
	public class Wave {
		[XmlElement(ElementName="waveCode")]
		public string WaveCode { get; set; }
		[XmlElement(ElementName="centerLookAngle")]
		public string CenterLookAngle { get; set; }
		[XmlElement(ElementName="prf")]
		public string Prf { get; set; }
		[XmlElement(ElementName="proBandwidth")]
		public string ProBandwidth { get; set; }
		[XmlElement(ElementName="sampleRate")]
		public string SampleRate { get; set; }
		[XmlElement(ElementName="sampleDelay")]
		public string SampleDelay { get; set; }
		[XmlElement(ElementName="bandWidth")]
		public string BandWidth { get; set; }
		[XmlElement(ElementName="pulseWidth")]
		public string PulseWidth { get; set; }
		[XmlElement(ElementName="frameLength")]
		public string FrameLength { get; set; }
		[XmlElement(ElementName="compression")]
		public string Compression { get; set; }
		[XmlElement(ElementName="baqBlock")]
		public string BaqBlock { get; set; }
		[XmlElement(ElementName="valueMGC")]
		public ValueMGC ValueMGC { get; set; }
		[XmlElement(ElementName="groundVelocity")]
		public string GroundVelocity { get; set; }
		[XmlElement(ElementName="averageAltitude")]
		public string AverageAltitude { get; set; }
	}

	[XmlRoot(ElementName="waveParams")]
	public class WaveParams {
		[XmlElement(ElementName="wave")]
		public Wave Wave { get; set; }
		[XmlAttribute(AttributeName="num")]
		public string Num { get; set; }
	}

	[XmlRoot(ElementName="start")]
	public class Start {
		[XmlElement(ElementName="prfCounter")]
		public string PrfCounter { get; set; }
		[XmlElement(ElementName="transmitBeamNum")]
		public string TransmitBeamNum { get; set; }
		[XmlElement(ElementName="receiveBeamNum")]
		public string ReceiveBeamNum { get; set; }
		[XmlElement(ElementName="scanNum")]
		public string ScanNum { get; set; }
		[XmlElement(ElementName="squintAngle")]
		public string SquintAngle { get; set; }
	}

	[XmlRoot(ElementName="center")]
	public class Center {
		[XmlElement(ElementName="prfCounter")]
		public string PrfCounter { get; set; }
		[XmlElement(ElementName="transmitBeamNum")]
		public string TransmitBeamNum { get; set; }
		[XmlElement(ElementName="receiveBeamNum")]
		public string ReceiveBeamNum { get; set; }
		[XmlElement(ElementName="scanNum")]
		public string ScanNum { get; set; }
		[XmlElement(ElementName="squintAngle")]
		public string SquintAngle { get; set; }
		[XmlElement(ElementName="latitude")]
		public string Latitude { get; set; }
		[XmlElement(ElementName="longitude")]
		public string Longitude { get; set; }
	}

	[XmlRoot(ElementName="end")]
	public class End {
		[XmlElement(ElementName="prfCounter")]
		public string PrfCounter { get; set; }
		[XmlElement(ElementName="transmitBeamNum")]
		public string TransmitBeamNum { get; set; }
		[XmlElement(ElementName="receiveBeamNum")]
		public string ReceiveBeamNum { get; set; }
		[XmlElement(ElementName="scanNum")]
		public string ScanNum { get; set; }
		[XmlElement(ElementName="squintAngle")]
		public string SquintAngle { get; set; }
	}

	[XmlRoot(ElementName="spotlightPara")]
	public class SpotlightPara {
		[XmlElement(ElementName="start")]
		public Start Start { get; set; }
		[XmlElement(ElementName="center")]
		public Center Center { get; set; }
		[XmlElement(ElementName="end")]
		public End End { get; set; }
		[XmlElement(ElementName="scanStep")]
		public string ScanStep { get; set; }
		[XmlElement(ElementName="scanSwichNum")]
		public string ScanSwichNum { get; set; }
		[XmlElement(ElementName="startScanAngle")]
		public string StartScanAngle { get; set; }
	}

	[XmlRoot(ElementName="polar")]
	public class Polar {
		[XmlElement(ElementName="polarMode")]
		public string PolarMode { get; set; }
	}

	[XmlRoot(ElementName="polarParams")]
	public class PolarParams {
		[XmlElement(ElementName="polar")]
		public Polar Polar { get; set; }
		[XmlAttribute(AttributeName="num")]
		public string Num { get; set; }
	}

	[XmlRoot(ElementName="sensor")]
	public class Sensor {
		[XmlElement(ElementName="sensorID")]
		public string SensorID { get; set; }
		[XmlElement(ElementName="imagingMode")]
		public string ImagingMode { get; set; }
		[XmlElement(ElementName="lamda")]
		public string Lamda { get; set; }
		[XmlElement(ElementName="RadarCenterFrequency")]
		public string RadarCenterFrequency { get; set; }
		[XmlElement(ElementName="waveParams")]
		public WaveParams WaveParams { get; set; }
		[XmlElement(ElementName="spotlightPara")]
		public SpotlightPara SpotlightPara { get; set; }
		[XmlElement(ElementName="lookDirection")]
		public string LookDirection { get; set; }
		[XmlElement(ElementName="antennaMode")]
		public string AntennaMode { get; set; }
		[XmlElement(ElementName="agcMode")]
		public string AgcMode { get; set; }
		[XmlElement(ElementName="polarParams")]
		public PolarParams PolarParams { get; set; }
	}

	[XmlRoot(ElementName="platform")]
	public class Platform {
		[XmlElement(ElementName="CenterTime")]
		public string CenterTime { get; set; }
		[XmlElement(ElementName="Rs")]
		public string Rs { get; set; }
		[XmlElement(ElementName="satVelocity")]
		public string SatVelocity { get; set; }
		[XmlElement(ElementName="RollAngle")]
		public string RollAngle { get; set; }
		[XmlElement(ElementName="PitchAngle")]
		public string PitchAngle { get; set; }
		[XmlElement(ElementName="YawAngle")]
		public string YawAngle { get; set; }
		[XmlElement(ElementName="Xs")]
		public string Xs { get; set; }
		[XmlElement(ElementName="Ys")]
		public string Ys { get; set; }
		[XmlElement(ElementName="Zs")]
		public string Zs { get; set; }
		[XmlElement(ElementName="Vxs")]
		public string Vxs { get; set; }
		[XmlElement(ElementName="Vys")]
		public string Vys { get; set; }
		[XmlElement(ElementName="Vzs")]
		public string Vzs { get; set; }
	}

	[XmlRoot(ElementName="GPSParam")]
	public class GPSParam {
		[XmlElement(ElementName="TimeStamp")]
		public string TimeStamp { get; set; }
		[XmlElement(ElementName="xPosition")]
		public string XPosition { get; set; }
		[XmlElement(ElementName="yPosition")]
		public string YPosition { get; set; }
		[XmlElement(ElementName="zPosition")]
		public string ZPosition { get; set; }
		[XmlElement(ElementName="xVelocity")]
		public string XVelocity { get; set; }
		[XmlElement(ElementName="yVelocity")]
		public string YVelocity { get; set; }
		[XmlElement(ElementName="zVelocity")]
		public string ZVelocity { get; set; }
	}

	[XmlRoot(ElementName="GPS")]
	public class GPS {
		[XmlElement(ElementName="GPSParam")]
		public List<GPSParam> GPSParam { get; set; }
	}

	[XmlRoot(ElementName="ATTIParam")]
	public class ATTIParam {
		[XmlElement(ElementName="TimeStamp")]
		public string TimeStamp { get; set; }
		[XmlElement(ElementName="yawAngle")]
		public string YawAngle { get; set; }
		[XmlElement(ElementName="rollAngle")]
		public string RollAngle { get; set; }
		[XmlElement(ElementName="pitchAngle")]
		public string PitchAngle { get; set; }
	}

	[XmlRoot(ElementName="ATTI")]
	public class ATTI {
		[XmlElement(ElementName="ATTIParam")]
		public List<ATTIParam> ATTIParam { get; set; }
	}

	[XmlRoot(ElementName="productinfo")]
	public class Productinfo {
		[XmlElement(ElementName="NominalResolution")]
		public string NominalResolution { get; set; }
		[XmlElement(ElementName="WidthInMeters")]
		public string WidthInMeters { get; set; }
		[XmlElement(ElementName="productLevel")]
		public string ProductLevel { get; set; }
		[XmlElement(ElementName="productType")]
		public string ProductType { get; set; }
		[XmlElement(ElementName="productFormat")]
		public string ProductFormat { get; set; }
		[XmlElement(ElementName="productGentime")]
		public string ProductGentime { get; set; }
		[XmlElement(ElementName="productPolar")]
		public string ProductPolar { get; set; }
	}

	[XmlRoot(ElementName="imagingTime")]
	public class ImagingTime {
		[XmlElement(ElementName="start")]
		public string Start { get; set; }
		[XmlElement(ElementName="end")]
		public string End { get; set; }
	}

	[XmlRoot(ElementName="topLeft")]
	public class TopLeft {
		[XmlElement(ElementName="latitude")]
		public string Latitude { get; set; }
		[XmlElement(ElementName="longitude")]
		public string Longitude { get; set; }
	}

	[XmlRoot(ElementName="topRight")]
	public class TopRight {
		[XmlElement(ElementName="latitude")]
		public string Latitude { get; set; }
		[XmlElement(ElementName="longitude")]
		public string Longitude { get; set; }
	}

	[XmlRoot(ElementName="bottomLeft")]
	public class BottomLeft {
		[XmlElement(ElementName="latitude")]
		public string Latitude { get; set; }
		[XmlElement(ElementName="longitude")]
		public string Longitude { get; set; }
	}

	[XmlRoot(ElementName="bottomRight")]
	public class BottomRight {
		[XmlElement(ElementName="latitude")]
		public string Latitude { get; set; }
		[XmlElement(ElementName="longitude")]
		public string Longitude { get; set; }
	}

	[XmlRoot(ElementName="corner")]
	public class Corner {
		[XmlElement(ElementName="topLeft")]
		public TopLeft TopLeft { get; set; }
		[XmlElement(ElementName="topRight")]
		public TopRight TopRight { get; set; }
		[XmlElement(ElementName="bottomLeft")]
		public BottomLeft BottomLeft { get; set; }
		[XmlElement(ElementName="bottomRight")]
		public BottomRight BottomRight { get; set; }
	}

	[XmlRoot(ElementName="QualifyValue")]
	public class QualifyValue {
		[XmlElement(ElementName="HH")]
		public string HH { get; set; }
		[XmlElement(ElementName="HV")]
		public string HV { get; set; }
		[XmlElement(ElementName="VH")]
		public string VH { get; set; }
		[XmlElement(ElementName="VV")]
		public string VV { get; set; }
	}

	[XmlRoot(ElementName="echoSaturation")]
	public class EchoSaturation {
		[XmlElement(ElementName="HH")]
		public string HH { get; set; }
		[XmlElement(ElementName="HV")]
		public string HV { get; set; }
		[XmlElement(ElementName="VH")]
		public string VH { get; set; }
		[XmlElement(ElementName="VV")]
		public string VV { get; set; }
	}

	[XmlRoot(ElementName="imageinfo")]
	public class Imageinfo {
		[XmlElement(ElementName="imagingTime")]
		public ImagingTime ImagingTime { get; set; }
		[XmlElement(ElementName="nearRange")]
		public string NearRange { get; set; }
		[XmlElement(ElementName="refRange")]
		public string RefRange { get; set; }
		[XmlElement(ElementName="eqvFs")]
		public string EqvFs { get; set; }
		[XmlElement(ElementName="eqvPRF")]
		public string EqvPRF { get; set; }
		[XmlElement(ElementName="center")]
		public Center Center { get; set; }
		[XmlElement(ElementName="corner")]
		public Corner Corner { get; set; }
		[XmlElement(ElementName="width")]
		public string Width { get; set; }
		[XmlElement(ElementName="height")]
		public string Height { get; set; }
		[XmlElement(ElementName="widthspace")]
		public string Widthspace { get; set; }
		[XmlElement(ElementName="heightspace")]
		public string Heightspace { get; set; }
		[XmlElement(ElementName="sceneShift")]
		public string SceneShift { get; set; }
		[XmlElement(ElementName="imagebit")]
		public string Imagebit { get; set; }
		[XmlElement(ElementName="QualifyValue")]
		public QualifyValue QualifyValue { get; set; }
		[XmlElement(ElementName="echoSaturation")]
		public EchoSaturation EchoSaturation { get; set; }
	}

	[XmlRoot(ElementName="CalibrationConst")]
	public class CalibrationConst {
		[XmlElement(ElementName="HH")]
		public string HH { get; set; }
		[XmlElement(ElementName="HV")]
		public string HV { get; set; }
		[XmlElement(ElementName="VH")]
		public string VH { get; set; }
		[XmlElement(ElementName="VV")]
		public string VV { get; set; }
	}

	[XmlRoot(ElementName="DopplerCentroidCoefficients")]
	public class DopplerCentroidCoefficients {
		[XmlElement(ElementName="d0")]
		public string D0 { get; set; }
		[XmlElement(ElementName="d1")]
		public string D1 { get; set; }
		[XmlElement(ElementName="d2")]
		public string D2 { get; set; }
		[XmlElement(ElementName="d3")]
		public string D3 { get; set; }
		[XmlElement(ElementName="d4")]
		public string D4 { get; set; }
	}

	[XmlRoot(ElementName="DopplerRateValuesCoefficients")]
	public class DopplerRateValuesCoefficients {
		[XmlElement(ElementName="r0")]
		public string R0 { get; set; }
		[XmlElement(ElementName="r1")]
		public string R1 { get; set; }
		[XmlElement(ElementName="r2")]
		public string R2 { get; set; }
		[XmlElement(ElementName="r3")]
		public string R3 { get; set; }
		[XmlElement(ElementName="r4")]
		public string R4 { get; set; }
	}

	[XmlRoot(ElementName="processinfo")]
	public class Processinfo {
		[XmlElement(ElementName="EphemerisData")]
		public string EphemerisData { get; set; }
		[XmlElement(ElementName="AttitudeData")]
		public string AttitudeData { get; set; }
		[XmlElement(ElementName="algorithm")]
		public string Algorithm { get; set; }
		[XmlElement(ElementName="CalibrationConst")]
		public CalibrationConst CalibrationConst { get; set; }
		[XmlElement(ElementName="AzFdc0")]
		public string AzFdc0 { get; set; }
		[XmlElement(ElementName="AzFdc1")]
		public string AzFdc1 { get; set; }
		[XmlElement(ElementName="MultilookRange")]
		public string MultilookRange { get; set; }
		[XmlElement(ElementName="MultilookAzimuth")]
		public string MultilookAzimuth { get; set; }
		[XmlElement(ElementName="DoIQComp")]
		public string DoIQComp { get; set; }
		[XmlElement(ElementName="DoChaComp")]
		public string DoChaComp { get; set; }
		[XmlElement(ElementName="DoFPInnerImbalanceComp")]
		public string DoFPInnerImbalanceComp { get; set; }
		[XmlElement(ElementName="DoFPCalibration")]
		public string DoFPCalibration { get; set; }
		[XmlElement(ElementName="DoFdcEst")]
		public string DoFdcEst { get; set; }
		[XmlElement(ElementName="DoFrEst")]
		public string DoFrEst { get; set; }
		[XmlElement(ElementName="RangeWeightType")]
		public string RangeWeightType { get; set; }
		[XmlElement(ElementName="RangeWeightPara")]
		public string RangeWeightPara { get; set; }
		[XmlElement(ElementName="AzimuthWeightType")]
		public string AzimuthWeightType { get; set; }
		[XmlElement(ElementName="AzimuthWeightPara")]
		public string AzimuthWeightPara { get; set; }
		[XmlElement(ElementName="Sidelobe_Suppress_Flag")]
		public string Sidelobe_Suppress_Flag { get; set; }
		[XmlElement(ElementName="Speckle_Suppress_Flag")]
		public string Speckle_Suppress_Flag { get; set; }
		[XmlElement(ElementName="EarthModel")]
		public string EarthModel { get; set; }
		[XmlElement(ElementName="ProjectModel")]
		public string ProjectModel { get; set; }
		[XmlElement(ElementName="DEMModel")]
		public string DEMModel { get; set; }
		[XmlElement(ElementName="QualifyModel")]
		public string QualifyModel { get; set; }
		[XmlElement(ElementName="RadiometricModel")]
		public string RadiometricModel { get; set; }
		[XmlElement(ElementName="incidenceAngleNearRange")]
		public string IncidenceAngleNearRange { get; set; }
		[XmlElement(ElementName="incidenceAngleFarRange")]
		public string IncidenceAngleFarRange { get; set; }
		[XmlElement(ElementName="InnerCalibration")]
		public string InnerCalibration { get; set; }
		[XmlElement(ElementName="DifferentBetweenDifferentChannel")]
		public string DifferentBetweenDifferentChannel { get; set; }
		[XmlElement(ElementName="NumberOfPoleHHMissingLines")]
		public string NumberOfPoleHHMissingLines { get; set; }
		[XmlElement(ElementName="NumberOfPoleHVMissingLines")]
		public string NumberOfPoleHVMissingLines { get; set; }
		[XmlElement(ElementName="NumberOfPoleVHMissingLines")]
		public string NumberOfPoleVHMissingLines { get; set; }
		[XmlElement(ElementName="NumberOfPoleVVMissingLines")]
		public string NumberOfPoleVVMissingLines { get; set; }
		[XmlElement(ElementName="ReceiverSettableGain")]
		public string ReceiverSettableGain { get; set; }
		[XmlElement(ElementName="ProcessorGainCorrection")]
		public string ProcessorGainCorrection { get; set; }
		[XmlElement(ElementName="ElevationPatternCorrection")]
		public string ElevationPatternCorrection { get; set; }
		[XmlElement(ElementName="AmuthPatternCorrection")]
		public string AmuthPatternCorrection { get; set; }
		[XmlElement(ElementName="RangeLookBandWidth")]
		public string RangeLookBandWidth { get; set; }
		[XmlElement(ElementName="AzimuthLookBandWidth")]
		public string AzimuthLookBandWidth { get; set; }
		[XmlElement(ElementName="TotalProcessedAzimuthBandWidth")]
		public string TotalProcessedAzimuthBandWidth { get; set; }
		[XmlElement(ElementName="DopplerParametersReferenceTime")]
		public string DopplerParametersReferenceTime { get; set; }
		[XmlElement(ElementName="DopplerCentroidCoefficients")]
		public DopplerCentroidCoefficients DopplerCentroidCoefficients { get; set; }
		[XmlElement(ElementName="DopplerRateValuesCoefficients")]
		public DopplerRateValuesCoefficients DopplerRateValuesCoefficients { get; set; }
		[XmlElement(ElementName="DEM")]
		public string DEM { get; set; }
	}

	[XmlRoot(ElementName="product")]
	public class ProductMetadata {
		[XmlElement(ElementName="segmentID")]
		public string SegmentID { get; set; }
		[XmlElement(ElementName="sceneID")]
		public string SceneID { get; set; }
		[XmlElement(ElementName="satellite")]
		public string Satellite { get; set; }
		[XmlElement(ElementName="orbitID")]
		public string OrbitID { get; set; }
		[XmlElement(ElementName="orbitType")]
		public string OrbitType { get; set; }
		[XmlElement(ElementName="attiType")]
		public string AttiType { get; set; }
		[XmlElement(ElementName="Direction")]
		public string Direction { get; set; }
		[XmlElement(ElementName="productID")]
		public string ProductID { get; set; }
		[XmlElement(ElementName="DocumentIdentifier")]
		public string DocumentIdentifier { get; set; }
		[XmlElement(ElementName="ReceiveTime")]
		public string ReceiveTime { get; set; }
		[XmlElement(ElementName="IsZeroDopplerSteering")]
		public string IsZeroDopplerSteering { get; set; }
		[XmlElement(ElementName="Station")]
		public string Station { get; set; }
		[XmlElement(ElementName="sensor")]
		public Sensor Sensor { get; set; }
		[XmlElement(ElementName="platform")]
		public Platform Platform { get; set; }
		[XmlElement(ElementName="GPS")]
		public GPS GPS { get; set; }
		[XmlElement(ElementName="ATTI")]
		public ATTI ATTI { get; set; }
		[XmlElement(ElementName="productinfo")]
		public Productinfo Productinfo { get; set; }
		[XmlElement(ElementName="imageinfo")]
		public Imageinfo Imageinfo { get; set; }
		[XmlElement(ElementName="processinfo")]
		public Processinfo Processinfo { get; set; }
	}

}
