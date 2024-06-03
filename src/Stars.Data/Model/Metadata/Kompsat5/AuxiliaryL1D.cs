// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AuxiliaryL1D.cs

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Terradue.Stars.Data.Model.Metadata.Kompsat5
{

    [XmlRoot(ElementName = "BurstsPerSubswath")]
    public class BurstsPerSubswath
    {

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlElement(ElementName = "FinalOnBoardTime")]
        public double FinalOnBoardTime { get; set; }

        [XmlElement(ElementName = "InitialOnBoardTime")]
        public double InitialOnBoardTime { get; set; }

        [XmlElement(ElementName = "AzimuthFirstTime")]
        public double AzimuthFirstTime { get; set; }

        [XmlElement(ElementName = "AzimuthLastTime")]
        public double AzimuthLastTime { get; set; }

        [XmlElement(ElementName = "AzimuthRampCode")]
        public string AzimuthRampCode { get; set; }

        [XmlElement(ElementName = "AzimuthRampCodeChangeLines")]
        public int AzimuthRampCodeChangeLines { get; set; }

        [XmlElement(ElementName = "AzimuthSteering")]
        public string AzimuthSteering { get; set; }

        [XmlElement(ElementName = "ElevationRampCode")]
        public int ElevationRampCode { get; set; }

        [XmlElement(ElementName = "ElevationRampCodeChangeLines")]
        public string ElevationRampCodeChangeLines { get; set; }

        [XmlElement(ElementName = "ElevationSteering")]
        public string ElevationSteering { get; set; }

        [XmlElement(ElementName = "RangeFirstTimeChangeLines")]
        public string RangeFirstTimeChangeLines { get; set; }

        [XmlElement(ElementName = "RangeFirstTimes")]
        public string RangeFirstTimes { get; set; }

        [XmlElement(ElementName = "ReceiverGain")]
        public int ReceiverGain { get; set; }

        [XmlElement(ElementName = "ReceiverGainChangeLines")]
        public string ReceiverGainChangeLines { get; set; }

        [XmlElement(ElementName = "RAWAmbiguousDopplerCentroid")]
        public string RAWAmbiguousDopplerCentroid { get; set; }

        [XmlElement(ElementName = "RAWBias")]
        public string RAWBias { get; set; }

        [XmlElement(ElementName = "RAWGainImbalance")]
        public string RAWGainImbalance { get; set; }

        [XmlElement(ElementName = "RAWIQNormality")]
        public string RAWIQNormality { get; set; }

        [XmlElement(ElementName = "RAWMissingBlocksStartLines")]
        public int RAWMissingBlocksStartLines { get; set; }

        [XmlElement(ElementName = "RAWMissingLinesperBlock")]
        public int RAWMissingLinesperBlock { get; set; }

        [XmlElement(ElementName = "RAWMissingLinesPercentage")]
        public int RAWMissingLinesPercentage { get; set; }

        [XmlElement(ElementName = "RAWOverSaturatedPercentage")]
        public string RAWOverSaturatedPercentage { get; set; }

        [XmlElement(ElementName = "RAWPhaseUniformity")]
        public string RAWPhaseUniformity { get; set; }

        [XmlElement(ElementName = "RAWStandardDeviation")]
        public string RAWStandardDeviation { get; set; }

        [XmlElement(ElementName = "RAWUnderSaturatedPercentage")]
        public string RAWUnderSaturatedPercentage { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BurstsPerSubswaths")]
    public class BurstsPerSubswaths
    {

        [XmlElement(ElementName = "BurstsPerSubswath")]
        public List<object> BurstsPerSubswath { get; set; }
    }

    [XmlRoot(ElementName = "GIM")]
    public class GIM
    {

        [XmlElement(ElementName = "LayoverPixelValue")]
        public int LayoverPixelValue { get; set; }

        [XmlElement(ElementName = "ShadowingPixelValue")]
        public int ShadowingPixelValue { get; set; }

        [XmlElement(ElementName = "RescalingFactor")]
        public double RescalingFactor { get; set; }

        [XmlElement(ElementName = "Offset")]
        public int Offset { get; set; }

        [XmlElement(ElementName = "InvalidValue")]
        public int InvalidValue { get; set; }

        [XmlElement(ElementName = "LayoverPixelPercentage")]
        public int LayoverPixelPercentage { get; set; }

        [XmlElement(ElementName = "ShadowingPixelPercentage")]
        public int ShadowingPixelPercentage { get; set; }
    }

    [XmlRoot(ElementName = "SubSwath")]
    public class SubSwath
    {

        [XmlElement(ElementName = "AntennaBeamCode")]
        public int AntennaBeamCode { get; set; }

        [XmlElement(ElementName = "AntennaBeamElevation")]
        public double AntennaBeamElevation { get; set; }

        [XmlElement(ElementName = "AzimuthInstrumentGeometricResolution")]
        public double AzimuthInstrumentGeometricResolution { get; set; }

        [XmlElement(ElementName = "BeamID")]
        public string BeamID { get; set; }

        [XmlElement(ElementName = "BeamOffNadirAngle")]
        public double BeamOffNadirAngle { get; set; }

        [XmlElement(ElementName = "BurstsperSubswath")]
        public int BurstsperSubswath { get; set; }

        [XmlElement(ElementName = "EchoSamplingWindowLength")]
        public int EchoSamplingWindowLength { get; set; }

        [XmlElement(ElementName = "GroundRangeInstrumentGeometricResolution")]
        public double GroundRangeInstrumentGeometricResolution { get; set; }

        [XmlElement(ElementName = "LinesperBurst")]
        public int LinesperBurst { get; set; }

        [XmlElement(ElementName = "PassBandIFFilter")]
        public int PassBandIFFilter { get; set; }

        [XmlElement(ElementName = "Polarisation")]
        public string Polarisation { get; set; }

        [XmlElement(ElementName = "PRF")]
        public double PRF { get; set; }

        [XmlElement(ElementName = "RangeChirpLength")]
        public double RangeChirpLength { get; set; }

        [XmlElement(ElementName = "RangeChirpRate")]
        public double RangeChirpRate { get; set; }

        [XmlElement(ElementName = "RangeChirpSamples")]
        public int RangeChirpSamples { get; set; }

        [XmlElement(ElementName = "Rank")]
        public int Rank { get; set; }

        [XmlElement(ElementName = "ReferenceDechirpingTime")]
        public double ReferenceDechirpingTime { get; set; }

        [XmlElement(ElementName = "SamplingRate")]
        public int SamplingRate { get; set; }

        [XmlElement(ElementName = "SyntheticApertureDuration")]
        public double SyntheticApertureDuration { get; set; }

        [XmlElement(ElementName = "AzimuthBandwidthperLook")]
        public double AzimuthBandwidthperLook { get; set; }

        [XmlElement(ElementName = "AzimuthFocusingBandwidth")]
        public double AzimuthFocusingBandwidth { get; set; }

        [XmlElement(ElementName = "AzimuthFocusingTransitionBandwidth")]
        public int AzimuthFocusingTransitionBandwidth { get; set; }

        [XmlElement(ElementName = "AzimuthMultilookingTransitionBandwidth")]
        public int AzimuthMultilookingTransitionBandwidth { get; set; }

        [XmlElement(ElementName = "ECEFBeamPointingforProcessing")]
        public string ECEFBeamPointingforProcessing { get; set; }

        [XmlElement(ElementName = "RangeBandwidthperLook")]
        public double RangeBandwidthperLook { get; set; }

        [XmlElement(ElementName = "RangeFocusingBandwidth")]
        public double RangeFocusingBandwidth { get; set; }

        [XmlElement(ElementName = "RangeFocusingTransitionBandwidth")]
        public double RangeFocusingTransitionBandwidth { get; set; }

        [XmlElement(ElementName = "RangeMultilookingTransitionBandwidth")]
        public int RangeMultilookingTransitionBandwidth { get; set; }

        [XmlElement(ElementName = "RAWStatisticsBlockSize")]
        public string RAWStatisticsBlockSize { get; set; }

        [XmlElement(ElementName = "InternalPower")]
        public string InternalPower { get; set; }

        [XmlElement(ElementName = "CentreGeodeticCoordinates")]
        public string CentreGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "AzimuthAntennaPatternGains")]
        public string AzimuthAntennaPatternGains { get; set; }

        [XmlElement(ElementName = "AzimuthAntennaPatternOrigin")]
        public double AzimuthAntennaPatternOrigin { get; set; }

        [XmlElement(ElementName = "AzimuthAntennaPatternResolution")]
        public double AzimuthAntennaPatternResolution { get; set; }

        [XmlElement(ElementName = "CalibrationConstant")]
        public double CalibrationConstant { get; set; }

        [XmlElement(ElementName = "CalibrationConstantEstimationUTC")]
        public string CalibrationConstantEstimationUTC { get; set; }

        [XmlElement(ElementName = "RangeAntennaPatternGains")]
        public string RangeAntennaPatternGains { get; set; }

        [XmlElement(ElementName = "RangeAntennaPatternOrigin")]
        public string RangeAntennaPatternOrigin { get; set; }

        [XmlElement(ElementName = "RangeAntennaPatternResolution")]
        public double RangeAntennaPatternResolution { get; set; }

        [XmlElement(ElementName = "AlongTrackVector")]
        public object AlongTrackVector { get; set; }

        [XmlElement(ElementName = "AzimuthCalibrationFactor")]
        public object AzimuthCalibrationFactor { get; set; }

        [XmlElement(ElementName = "ReferenceInternalPower")]
        public int ReferenceInternalPower { get; set; }

        [XmlElement(ElementName = "DopplerAmbiguity")]
        public int DopplerAmbiguity { get; set; }

        [XmlElement(ElementName = "BurstsPerSubswaths")]
        public BurstsPerSubswaths BurstsPerSubswaths { get; set; }

        [XmlElement(ElementName = "GIM")]
        public GIM GIM { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "SBI")]
        public MBI SBI { get; set; }
    }

    [XmlRoot(ElementName = "SubSwaths")]
    public class SubSwaths
    {

        [XmlElement(ElementName = "SubSwath")]
        public List<SubSwath> SubSwath { get; set; }
    }

    [XmlRoot(ElementName = "QLK")]
    public class QLK
    {

        [XmlElement(ElementName = "QuickLookColumnsOrder")]
        public string QuickLookColumnsOrder { get; set; }

        [XmlElement(ElementName = "QuickLookLinesOrder")]
        public string QuickLookLinesOrder { get; set; }

        [XmlElement(ElementName = "QuickLookColumnSpacing")]
        public int QuickLookColumnSpacing { get; set; }

        [XmlElement(ElementName = "QuickLookLineSpacing")]
        public int QuickLookLineSpacing { get; set; }

        [XmlElement(ElementName = "QuickLookProjectionID")]
        public string QuickLookProjectionID { get; set; }
    }

    [XmlRoot(ElementName = "MBI")]
    public class MBI
    {

        [XmlElement(ElementName = "ZeroDopplerAzimuthFirstTime")]
        public double ZeroDopplerAzimuthFirstTime { get; set; }

        [XmlElement(ElementName = "ZeroDopplerAzimuthLastTime")]
        public double ZeroDopplerAzimuthLastTime { get; set; }

        [XmlElement(ElementName = "ZeroDopplerRangeFirstTime")]
        public double ZeroDopplerRangeFirstTime { get; set; }

        [XmlElement(ElementName = "ZeroDopplerRangeLastTime")]
        public double ZeroDopplerRangeLastTime { get; set; }

        [XmlElement(ElementName = "ColumnSpacing")]
        public string ColumnSpacing { get; set; }

        [XmlElement(ElementName = "EquivalentNumberofLooks")]
        public double EquivalentNumberofLooks { get; set; }

        [XmlElement(ElementName = "LineSpacing")]
        public string LineSpacing { get; set; }

        [XmlElement(ElementName = "BottomLeftGeodeticCoordinates")]
        public string BottomLeftGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "BottomRightGeodeticCoordinates")]
        public string BottomRightGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "FarEarlyGeodeticCoordinates")]
        public string FarEarlyGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "FarIncidenceAngle")]
        public double FarIncidenceAngle { get; set; }

        [XmlElement(ElementName = "FarLateGeodeticCoordinates")]
        public string FarLateGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "FarLookAngle")]
        public double FarLookAngle { get; set; }

        [XmlElement(ElementName = "NearEarlyGeodeticCoordinates")]
        public string NearEarlyGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "NearIncidenceAngle")]
        public double NearIncidenceAngle { get; set; }

        [XmlElement(ElementName = "NearLateGeodeticCoordinates")]
        public string NearLateGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "NearLookAngle")]
        public double NearLookAngle { get; set; }

        [XmlElement(ElementName = "TopLeftGeodeticCoordinates")]
        public string TopLeftGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "TopRightGeodeticCoordinates")]
        public string TopRightGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "ImageMax")]
        public string ImageMax { get; set; }

        [XmlElement(ElementName = "ImageMean")]
        public string ImageMean { get; set; }

        [XmlElement(ElementName = "ImageMin")]
        public string ImageMin { get; set; }

        [XmlElement(ElementName = "ImageOverSaturatedPercentage")]
        public string ImageOverSaturatedPercentage { get; set; }

        [XmlElement(ElementName = "ImageStandardDeviation")]
        public string ImageStandardDeviation { get; set; }

        [XmlElement(ElementName = "ImageUnderSaturatedPercentage")]
        public string ImageUnderSaturatedPercentage { get; set; }
    }

    [XmlRoot(ElementName = "Root")]
    public class Root
    {

        [XmlElement(ElementName = "AcquisitionStationID")]
        public string AcquisitionStationID { get; set; }

        [XmlElement(ElementName = "DeliveryMode")]
        public string DeliveryMode { get; set; }

        [XmlElement(ElementName = "DownlinkStartUTC")]
        public string DownlinkStartUTC { get; set; }

        [XmlElement(ElementName = "DownlinkStopUTC")]
        public string DownlinkStopUTC { get; set; }

        [XmlElement(ElementName = "MissionID")]
        public string MissionID { get; set; }

        [XmlElement(ElementName = "ProcessingCentre")]
        public string ProcessingCentre { get; set; }

        [XmlElement(ElementName = "ProductFilename")]
        public string ProductFilename { get; set; }

        [XmlElement(ElementName = "ProductSpecificationDocument")]
        public string ProductSpecificationDocument { get; set; }

        [XmlElement(ElementName = "ProductType")]
        public string ProductType { get; set; }

        [XmlElement(ElementName = "SatelliteID")]
        public string SatelliteID { get; set; }

        [XmlElement(ElementName = "BitsperSample")]
        public int BitsperSample { get; set; }

        [XmlElement(ElementName = "ColumnsOrder")]
        public string ColumnsOrder { get; set; }

        [XmlElement(ElementName = "ImageLayers")]
        public int ImageLayers { get; set; }

        [XmlElement(ElementName = "ImageScale")]
        public string ImageScale { get; set; }

        [XmlElement(ElementName = "LinesOrder")]
        public string LinesOrder { get; set; }

        [XmlElement(ElementName = "SampleFormat")]
        public string SampleFormat { get; set; }

        [XmlElement(ElementName = "SamplesperPixel")]
        public int SamplesperPixel { get; set; }

        [XmlElement(ElementName = "LeapSign")]
        public int LeapSign { get; set; }

        [XmlElement(ElementName = "LeapUTC")]
        public string LeapUTC { get; set; }

        [XmlElement(ElementName = "OrbitNumber")]
        public int OrbitNumber { get; set; }

        [XmlElement(ElementName = "ProgrammedImageID")]
        public int ProgrammedImageID { get; set; }

        [XmlElement(ElementName = "SceneSensingStartUTC")]
        public string SceneSensingStartUTC { get; set; }

        [XmlElement(ElementName = "SceneSensingStopUTC")]
        public string SceneSensingStopUTC { get; set; }

        [XmlElement(ElementName = "SelectiveAvailabilityStatus")]
        public string SelectiveAvailabilityStatus { get; set; }

        [XmlElement(ElementName = "AcquisitionMode")]
        public string AcquisitionMode { get; set; }

        [XmlElement(ElementName = "AntennaLength")]
        public double AntennaLength { get; set; }

        [XmlElement(ElementName = "AzimuthBeamwidth")]
        public double AzimuthBeamwidth { get; set; }

        [XmlElement(ElementName = "LookSide")]
        public string LookSide { get; set; }

        [XmlElement(ElementName = "MultiBeamID")]
        public string MultiBeamID { get; set; }

        [XmlElement(ElementName = "OriginalBitQuantisation")]
        public int OriginalBitQuantisation { get; set; }

        [XmlElement(ElementName = "RadarFrequency")]
        public double RadarFrequency { get; set; }

        [XmlElement(ElementName = "RadarWavelength")]
        public double RadarWavelength { get; set; }

        [XmlElement(ElementName = "SubswathsNumber")]
        public int SubswathsNumber { get; set; }

        [XmlElement(ElementName = "GeometricConformity")]
        public double GeometricConformity { get; set; }

        [XmlElement(ElementName = "GroundRangeGeometricResolution")]
        public double GroundRangeGeometricResolution { get; set; }

        [XmlElement(ElementName = "AzimuthGeometricResolution")]
        public double AzimuthGeometricResolution { get; set; }

        [XmlElement(ElementName = "AttitudeQuaternions")]
        public string AttitudeQuaternions { get; set; }

        [XmlElement(ElementName = "AttitudeTimes")]
        public string AttitudeTimes { get; set; }

        [XmlElement(ElementName = "ECEFSatellitePosition")]
        public string ECEFSatellitePosition { get; set; }

        [XmlElement(ElementName = "ECEFSatelliteVelocity")]
        public string ECEFSatelliteVelocity { get; set; }

        [XmlElement(ElementName = "ECEFSatelliteAcceleration")]
        public string ECEFSatelliteAcceleration { get; set; }

        [XmlElement(ElementName = "NumberofStateVectors")]
        public int NumberofStateVectors { get; set; }

        [XmlElement(ElementName = "NumberofAttitudedata")]
        public int NumberofAttitudedata { get; set; }

        [XmlElement(ElementName = "OrbitDirection")]
        public string OrbitDirection { get; set; }

        [XmlElement(ElementName = "PitchAngle")]
        public string PitchAngle { get; set; }

        [XmlElement(ElementName = "PitchRate")]
        public string PitchRate { get; set; }

        [XmlElement(ElementName = "RollAngle")]
        public string RollAngle { get; set; }

        [XmlElement(ElementName = "RollRate")]
        public string RollRate { get; set; }

        [XmlElement(ElementName = "SatelliteHeight")]
        public double SatelliteHeight { get; set; }

        [XmlElement(ElementName = "StateVectorsTimes")]
        public string StateVectorsTimes { get; set; }

        [XmlElement(ElementName = "YawAngle")]
        public string YawAngle { get; set; }

        [XmlElement(ElementName = "YawRate")]
        public string YawRate { get; set; }

        [XmlElement(ElementName = "AzimuthPolynomialReferenceTime")]
        public double AzimuthPolynomialReferenceTime { get; set; }

        [XmlElement(ElementName = "CentroidvsAzimuthTimePolynomial")]
        public string CentroidvsAzimuthTimePolynomial { get; set; }

        [XmlElement(ElementName = "CentroidvsRangeTimePolynomial")]
        public string CentroidvsRangeTimePolynomial { get; set; }

        [XmlElement(ElementName = "DopplerAmbiguityEstimationMethod")]
        public string DopplerAmbiguityEstimationMethod { get; set; }

        [XmlElement(ElementName = "DopplerCentroidEstimationMethod")]
        public string DopplerCentroidEstimationMethod { get; set; }

        [XmlElement(ElementName = "DopplerRateEstimationMethod")]
        public string DopplerRateEstimationMethod { get; set; }

        [XmlElement(ElementName = "DopplerRatevsAzimuthTimePolynomial")]
        public string DopplerRatevsAzimuthTimePolynomial { get; set; }

        [XmlElement(ElementName = "DopplerRatevsRangeTimePolynomial")]
        public string DopplerRatevsRangeTimePolynomial { get; set; }

        [XmlElement(ElementName = "RangePolynomialReferenceTime")]
        public double RangePolynomialReferenceTime { get; set; }

        [XmlElement(ElementName = "AzimuthFocusingWeightingCoefficient")]
        public double AzimuthFocusingWeightingCoefficient { get; set; }

        [XmlElement(ElementName = "AzimuthFocusingWeightingFunction")]
        public string AzimuthFocusingWeightingFunction { get; set; }

        [XmlElement(ElementName = "AzimuthMultilookingWeightingCoefficient")]
        public int AzimuthMultilookingWeightingCoefficient { get; set; }

        [XmlElement(ElementName = "AzimuthMultilookingWeightingFunction")]
        public string AzimuthMultilookingWeightingFunction { get; set; }

        [XmlElement(ElementName = "AzimuthProcessingNumberofLooks")]
        public int AzimuthProcessingNumberofLooks { get; set; }

        [XmlElement(ElementName = "ECEFBeamCentreDirectionforProcessing")]
        public string ECEFBeamCentreDirectionforProcessing { get; set; }

        [XmlElement(ElementName = "FocusingAlgorithmID")]
        public string FocusingAlgorithmID { get; set; }

        [XmlElement(ElementName = "InvalidValue")]
        public int InvalidValue { get; set; }

        [XmlElement(ElementName = "L0SoftwareVersion")]
        public string L0SoftwareVersion { get; set; }

        [XmlElement(ElementName = "L1ASoftwareVersion")]
        public string L1ASoftwareVersion { get; set; }

        [XmlElement(ElementName = "L1BSoftwareVersion")]
        public string L1BSoftwareVersion { get; set; }

        [XmlElement(ElementName = "L1DSoftwareVersion")]
        public string L1DSoftwareVersion { get; set; }

        [XmlElement(ElementName = "LightSpeed")]
        public int LightSpeed { get; set; }

        [XmlElement(ElementName = "DespeckleTechnique")]
        public string DespeckleTechnique { get; set; }

        [XmlElement(ElementName = "DespeckleDirection")]
        public string DespeckleDirection { get; set; }

        [XmlElement(ElementName = "ProductErrorFlag")]
        public int ProductErrorFlag { get; set; }

        [XmlElement(ElementName = "ProductGenerationUTC")]
        public string ProductGenerationUTC { get; set; }

        [XmlElement(ElementName = "RangeFocusingWeightingCoefficient")]
        public double RangeFocusingWeightingCoefficient { get; set; }

        [XmlElement(ElementName = "RangeFocusingWeightingFunction")]
        public string RangeFocusingWeightingFunction { get; set; }

        [XmlElement(ElementName = "RangeMultilookingWeightingCoefficient")]
        public double RangeMultilookingWeightingCoefficient { get; set; }

        [XmlElement(ElementName = "RangeMultilookingWeightingFunction")]
        public string RangeMultilookingWeightingFunction { get; set; }

        [XmlElement(ElementName = "RangeProcessingNumberofLooks")]
        public int RangeProcessingNumberofLooks { get; set; }

        [XmlElement(ElementName = "ReferenceUTC")]
        public string ReferenceUTC { get; set; }

        [XmlElement(ElementName = "ReplicaReconstructionMethod")]
        public string ReplicaReconstructionMethod { get; set; }

        [XmlElement(ElementName = "RescalingFactor")]
        public double RescalingFactor { get; set; }

        [XmlElement(ElementName = "DespeckleFilterMovingWindow")]
        public int DespeckleFilterMovingWindow { get; set; }

        [XmlElement(ElementName = "DespeckleFilter")]
        public string DespeckleFilter { get; set; }

        [XmlElement(ElementName = "DatumRotation")]
        public string DatumRotation { get; set; }

        [XmlElement(ElementName = "DatumScale")]
        public int DatumScale { get; set; }

        [XmlElement(ElementName = "DatumShift")]
        public string DatumShift { get; set; }

        [XmlElement(ElementName = "EllipsoidDesignator")]
        public string EllipsoidDesignator { get; set; }

        [XmlElement(ElementName = "EllipsoidSemimajorAxis")]
        public int EllipsoidSemimajorAxis { get; set; }

        [XmlElement(ElementName = "EllipsoidSemiminorAxis")]
        public int EllipsoidSemiminorAxis { get; set; }

        [XmlElement(ElementName = "GroundProjectionReferenceSurface")]
        public string GroundProjectionReferenceSurface { get; set; }

        [XmlElement(ElementName = "MapProjectionCentre")]
        public string MapProjectionCentre { get; set; }

        [XmlElement(ElementName = "MapProjectionScaleFactor")]
        public int MapProjectionScaleFactor { get; set; }

        [XmlElement(ElementName = "MapProjectionZone")]
        public int MapProjectionZone { get; set; }

        [XmlElement(ElementName = "ProjectionID")]
        public string ProjectionID { get; set; }

        [XmlElement(ElementName = "AzimuthCoverage")]
        public double AzimuthCoverage { get; set; }

        [XmlElement(ElementName = "CentreEarthRadius")]
        public double CentreEarthRadius { get; set; }

        [XmlElement(ElementName = "GroundRangeCoverage")]
        public double GroundRangeCoverage { get; set; }

        [XmlElement(ElementName = "SceneCentreGeodeticCoordinates")]
        public string SceneCentreGeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "SceneOrientation")]
        public int SceneOrientation { get; set; }

        [XmlElement(ElementName = "TerrainElevationStandardDeviation")]
        public string TerrainElevationStandardDeviation { get; set; }

        [XmlElement(ElementName = "TerrainMaximumElevation")]
        public int TerrainMaximumElevation { get; set; }

        [XmlElement(ElementName = "TerrainMeanElevation")]
        public string TerrainMeanElevation { get; set; }

        [XmlElement(ElementName = "TerrainMinimumElevation")]
        public int TerrainMinimumElevation { get; set; }

        [XmlElement(ElementName = "ADCCharacterization")]
        public string ADCCharacterization { get; set; }

        [XmlElement(ElementName = "ADCCompensation")]
        public int ADCCompensation { get; set; }

        [XmlElement(ElementName = "AntennaPatternCompensationReferenceSurface")]
        public string AntennaPatternCompensationReferenceSurface { get; set; }

        [XmlElement(ElementName = "AzimuthAntennaPatternCompensationGeometry")]
        public string AzimuthAntennaPatternCompensationGeometry { get; set; }

        [XmlElement(ElementName = "CalibrationConstantCompensationFlag")]
        public int CalibrationConstantCompensationFlag { get; set; }

        [XmlElement(ElementName = "IncidenceAngleCompensationGeometry")]
        public string IncidenceAngleCompensationGeometry { get; set; }

        [XmlElement(ElementName = "IncidenceAngleCompensationReferenceSurface")]
        public string IncidenceAngleCompensationReferenceSurface { get; set; }

        [XmlElement(ElementName = "RangeAntennaPatternCompensationGeometry")]
        public string RangeAntennaPatternCompensationGeometry { get; set; }

        [XmlElement(ElementName = "RangeSpreadingLossCompensationGeometry")]
        public string RangeSpreadingLossCompensationGeometry { get; set; }

        [XmlElement(ElementName = "ReferenceIncidenceAngle")]
        public int ReferenceIncidenceAngle { get; set; }

        [XmlElement(ElementName = "ReferenceSlantRange")]
        public int ReferenceSlantRange { get; set; }

        [XmlElement(ElementName = "ReferenceSlantRangeExponent")]
        public double ReferenceSlantRangeExponent { get; set; }

        [XmlElement(ElementName = "DopplerAmbiguityConfidenceMeasureThreshold")]
        public double DopplerAmbiguityConfidenceMeasureThreshold { get; set; }

        [XmlElement(ElementName = "DopplerAmbiguityThreshold")]
        public int DopplerAmbiguityThreshold { get; set; }

        [XmlElement(ElementName = "DopplerCentroidConfidenceMeasureThreshold")]
        public double DopplerCentroidConfidenceMeasureThreshold { get; set; }

        [XmlElement(ElementName = "DopplerCentroidEstimationAccuracyThreshold")]
        public int DopplerCentroidEstimationAccuracyThreshold { get; set; }

        [XmlElement(ElementName = "DopplerRateConfidenceMeasureThreshold")]
        public double DopplerRateConfidenceMeasureThreshold { get; set; }

        [XmlElement(ElementName = "DopplerRateEstimationAccuracyThreshold")]
        public double DopplerRateEstimationAccuracyThreshold { get; set; }

        [XmlElement(ElementName = "ImageOverSaturatedPercentageThreshold")]
        public int ImageOverSaturatedPercentageThreshold { get; set; }

        [XmlElement(ElementName = "ImageUnderSaturatedPercentageThreshold")]
        public int ImageUnderSaturatedPercentageThreshold { get; set; }

        [XmlElement(ElementName = "RAWBiasThreshold")]
        public double RAWBiasThreshold { get; set; }

        [XmlElement(ElementName = "RAWGainImbalanceThreshold")]
        public double RAWGainImbalanceThreshold { get; set; }

        [XmlElement(ElementName = "RAWIQNormalityThreshold")]
        public double RAWIQNormalityThreshold { get; set; }

        [XmlElement(ElementName = "RAWMissingLinesperBlockThreshold")]
        public int RAWMissingLinesperBlockThreshold { get; set; }

        [XmlElement(ElementName = "RAWMissingLinesPercentageThreshold")]
        public double RAWMissingLinesPercentageThreshold { get; set; }

        [XmlElement(ElementName = "RAWOverSaturatedPercentageThreshold")]
        public double RAWOverSaturatedPercentageThreshold { get; set; }

        [XmlElement(ElementName = "RAWPhaseUniformityThreshold")]
        public double RAWPhaseUniformityThreshold { get; set; }

        [XmlElement(ElementName = "RAWUnderSaturatedPercentageThreshold")]
        public double RAWUnderSaturatedPercentageThreshold { get; set; }

        [XmlElement(ElementName = "AttitudeProductCategory")]
        public string AttitudeProductCategory { get; set; }

        [XmlElement(ElementName = "DopplerAmbiguityConfidenceMeasure")]
        public int DopplerAmbiguityConfidenceMeasure { get; set; }

        [XmlElement(ElementName = "DopplerCentroidConfidenceMeasure")]
        public double DopplerCentroidConfidenceMeasure { get; set; }

        [XmlElement(ElementName = "DopplerCentroidEstimationAccuracy")]
        public int DopplerCentroidEstimationAccuracy { get; set; }

        [XmlElement(ElementName = "DopplerRateConfidenceMeasure")]
        public double DopplerRateConfidenceMeasure { get; set; }

        [XmlElement(ElementName = "DopplerRateEstimationAccuracy")]
        public double DopplerRateEstimationAccuracy { get; set; }

        [XmlElement(ElementName = "OrbitProductCategory")]
        public string OrbitProductCategory { get; set; }

        [XmlElement(ElementName = "SPFMeanIntensityRatio")]
        public int SPFMeanIntensityRatio { get; set; }

        [XmlElement(ElementName = "SPFStandardDeviationIntensityRatio")]
        public int SPFStandardDeviationIntensityRatio { get; set; }

        [XmlElement(ElementName = "SubSwaths")]
        public SubSwaths SubSwaths { get; set; }

        [XmlElement(ElementName = "QLK")]
        public QLK QLK { get; set; }

        [XmlElement(ElementName = "MBI")]
        public MBI MBI { get; set; }
    }

    [XmlRoot(ElementName = "Auxiliary")]
    public class Auxiliary
    {

        [XmlElement(ElementName = "Root")]
        public Root Root { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd")]
        public string Xsd { get; set; }

        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation")]
        public string NoNamespaceSchemaLocation { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class ACQUISITION_MODE
    {
        public const string HIGH_RESOLUTION = "HIGH RESOLUTION";
        public const string ENHANCED_HIGH_RESOLUTION = "ENHANCED HIGH RESOLUTION";
        public const string ULTRA_HIGH_RESOLUTION = "ULTRA HIGH RESOLUTION";
        public const string STANDARD = "STANDARD";
        public const string ENHANCED_STANDARD = "ENHANCED STANDARD";
        public const string WIDE_SWATH = "WIDE SWATH";
        public const string ENHANCED_WIDE_SWATH = "ENHANCED WIDE SWATH";
    }

    public class ORBIT_STATE
    {
        public const string ASCENDING = "ASCENDING";
        public const string DESCENDING = "DESCENDING";
    }

}
