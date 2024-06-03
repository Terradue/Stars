// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Product.cs

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Terradue.Stars.Data.Model.Metadata.Rcm
{

    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Product));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Product)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "securityAttributes", Namespace = "rcmGsProductSchema")]
    public class SecurityAttributes
    {

        [XmlElement(ElementName = "securityClassification", Namespace = "rcmGsProductSchema")]
        public string SecurityClassification { get; set; }

        [XmlElement(ElementName = "specialHandlingRequired", Namespace = "rcmGsProductSchema")]
        public bool SpecialHandlingRequired { get; set; }

        [XmlElement(ElementName = "specialHandlingInstructions", Namespace = "rcmGsProductSchema")]
        public string SpecialHandlingInstructions { get; set; }
    }

    [XmlRoot(ElementName = "rank", Namespace = "rcmGsProductSchema")]
    public class Rank
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "settableGain", Namespace = "rcmGsProductSchema")]
    public class SettableGain
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "radarCenterFrequency", Namespace = "rcmGsProductSchema")]
    public class RadarCenterFrequency
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "pulseRepetitionFrequency", Namespace = "rcmGsProductSchema")]
    public class PulseRepetitionFrequency
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "prfInformation", Namespace = "rcmGsProductSchema")]
    public class PrfInformation
    {

        [XmlElement(ElementName = "rawLine", Namespace = "rcmGsProductSchema")]
        public int RawLine { get; set; }

        [XmlElement(ElementName = "pulseRepetitionFrequency", Namespace = "rcmGsProductSchema")]
        public PulseRepetitionFrequency PulseRepetitionFrequency { get; set; }

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "pulseLength", Namespace = "rcmGsProductSchema")]
    public class PulseLength
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "pulseBandwidth", Namespace = "rcmGsProductSchema")]
    public class PulseBandwidth
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "samplingWindowStartTimeFirstRawLine", Namespace = "rcmGsProductSchema")]
    public class SamplingWindowStartTimeFirstRawLine
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "samplingWindowStartTimeLastRawLine", Namespace = "rcmGsProductSchema")]
    public class SamplingWindowStartTimeLastRawLine
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "numberOfSwstChanges", Namespace = "rcmGsProductSchema")]
    public class NumberOfSwstChanges
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "adcSamplingRate", Namespace = "rcmGsProductSchema")]
    public class AdcSamplingRate
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "samplesPerEchoLine", Namespace = "rcmGsProductSchema")]
    public class SamplesPerEchoLine
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "radarParameters", Namespace = "rcmGsProductSchema")]
    public class RadarParameters
    {

        [XmlElement(ElementName = "acquisitionType", Namespace = "rcmGsProductSchema")]
        public string AcquisitionType { get; set; }

        [XmlElement(ElementName = "beams", Namespace = "rcmGsProductSchema")]
        public string Beams { get; set; }

        [XmlElement(ElementName = "polarizations", Namespace = "rcmGsProductSchema")]
        public string Polarizations { get; set; }

        [XmlElement(ElementName = "pulses", Namespace = "rcmGsProductSchema")]
        public string Pulses { get; set; }

        [XmlElement(ElementName = "rank", Namespace = "rcmGsProductSchema")]
        public Rank Rank { get; set; }

        [XmlElement(ElementName = "settableGain", Namespace = "rcmGsProductSchema")]
        public List<SettableGain> SettableGain { get; set; }

        [XmlElement(ElementName = "radarCenterFrequency", Namespace = "rcmGsProductSchema")]
        public RadarCenterFrequency RadarCenterFrequency { get; set; }

        [XmlElement(ElementName = "prfInformation", Namespace = "rcmGsProductSchema")]
        public PrfInformation PrfInformation { get; set; }

        [XmlElement(ElementName = "pulseLength", Namespace = "rcmGsProductSchema")]
        public PulseLength PulseLength { get; set; }

        [XmlElement(ElementName = "pulseBandwidth", Namespace = "rcmGsProductSchema")]
        public PulseBandwidth PulseBandwidth { get; set; }

        [XmlElement(ElementName = "samplingWindowStartTimeFirstRawLine", Namespace = "rcmGsProductSchema")]
        public List<SamplingWindowStartTimeFirstRawLine> SamplingWindowStartTimeFirstRawLine { get; set; }

        [XmlElement(ElementName = "samplingWindowStartTimeLastRawLine", Namespace = "rcmGsProductSchema")]
        public List<SamplingWindowStartTimeLastRawLine> SamplingWindowStartTimeLastRawLine { get; set; }

        [XmlElement(ElementName = "numberOfSwstChanges", Namespace = "rcmGsProductSchema")]
        public List<NumberOfSwstChanges> NumberOfSwstChanges { get; set; }

        [XmlElement(ElementName = "antennaPointing", Namespace = "rcmGsProductSchema")]
        public string AntennaPointing { get; set; }

        [XmlElement(ElementName = "adcSamplingRate", Namespace = "rcmGsProductSchema")]
        public AdcSamplingRate AdcSamplingRate { get; set; }

        [XmlElement(ElementName = "zeroDopplerSteeringFlag", Namespace = "rcmGsProductSchema")]
        public string ZeroDopplerSteeringFlag { get; set; }

        [XmlElement(ElementName = "satOrientationRefFrame", Namespace = "rcmGsProductSchema")]
        public string SatOrientationRefFrame { get; set; }

        [XmlElement(ElementName = "rawBitsPerSample", Namespace = "rcmGsProductSchema")]
        public int RawBitsPerSample { get; set; }

        [XmlElement(ElementName = "samplesPerEchoLine", Namespace = "rcmGsProductSchema")]
        public SamplesPerEchoLine SamplesPerEchoLine { get; set; }

        [XmlElement(ElementName = "steppedReceiveMode", Namespace = "rcmGsProductSchema")]
        public bool SteppedReceiveMode { get; set; }
    }

    [XmlRoot(ElementName = "numberOfMissingLines", Namespace = "rcmGsProductSchema")]
    public class NumberOfMissingLines
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "bias", Namespace = "rcmGsProductSchema")]
    public class Bias
    {

        [XmlAttribute(AttributeName = "dataStream", Namespace = "")]
        public string DataStream { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "standardDeviation", Namespace = "rcmGsProductSchema")]
    public class StandardDeviation
    {

        [XmlAttribute(AttributeName = "dataStream", Namespace = "")]
        public string DataStream { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "phaseOrthogonality", Namespace = "rcmGsProductSchema")]
    public class PhaseOrthogonality
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rawDataHistogram", Namespace = "rcmGsProductSchema")]
    public class RawDataHistogram
    {

        [XmlAttribute(AttributeName = "dataStream", Namespace = "")]
        public string DataStream { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rawDataAnalysis", Namespace = "rcmGsProductSchema")]
    public class RawDataAnalysis
    {

        [XmlElement(ElementName = "bias", Namespace = "rcmGsProductSchema")]
        public List<Bias> Bias { get; set; }

        [XmlElement(ElementName = "standardDeviation", Namespace = "rcmGsProductSchema")]
        public List<StandardDeviation> StandardDeviation { get; set; }

        [XmlElement(ElementName = "gainImbalance", Namespace = "rcmGsProductSchema")]
        public double GainImbalance { get; set; }

        [XmlElement(ElementName = "phaseOrthogonality", Namespace = "rcmGsProductSchema")]
        public PhaseOrthogonality PhaseOrthogonality { get; set; }

        [XmlElement(ElementName = "rawDataHistogram", Namespace = "rcmGsProductSchema")]
        public List<RawDataHistogram> RawDataHistogram { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rawDataAttributes", Namespace = "rcmGsProductSchema")]
    public class RawDataAttributes
    {

        [XmlElement(ElementName = "numberOfInputDataGaps", Namespace = "rcmGsProductSchema")]
        public int NumberOfInputDataGaps { get; set; }

        [XmlElement(ElementName = "gapSize", Namespace = "rcmGsProductSchema")]
        public int GapSize { get; set; }

        [XmlElement(ElementName = "numberOfMissingLines", Namespace = "rcmGsProductSchema")]
        public List<NumberOfMissingLines> NumberOfMissingLines { get; set; }

        [XmlElement(ElementName = "rawDataAnalysis", Namespace = "rcmGsProductSchema")]
        public List<RawDataAnalysis> RawDataAnalysis { get; set; }
    }

    [XmlRoot(ElementName = "xPosition", Namespace = "rcmGsProductSchema")]
    public class XPosition
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "yPosition", Namespace = "rcmGsProductSchema")]
    public class YPosition
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "zPosition", Namespace = "rcmGsProductSchema")]
    public class ZPosition
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "xVelocity", Namespace = "rcmGsProductSchema")]
    public class XVelocity
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "yVelocity", Namespace = "rcmGsProductSchema")]
    public class YVelocity
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "zVelocity", Namespace = "rcmGsProductSchema")]
    public class ZVelocity
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "stateVector", Namespace = "rcmGsProductSchema")]
    public class StateVector
    {

        [XmlElement(ElementName = "timeStamp", Namespace = "rcmGsProductSchema")]
        public DateTime TimeStamp { get; set; }

        [XmlElement(ElementName = "xPosition", Namespace = "rcmGsProductSchema")]
        public XPosition XPosition { get; set; }

        [XmlElement(ElementName = "yPosition", Namespace = "rcmGsProductSchema")]
        public YPosition YPosition { get; set; }

        [XmlElement(ElementName = "zPosition", Namespace = "rcmGsProductSchema")]
        public ZPosition ZPosition { get; set; }

        [XmlElement(ElementName = "xVelocity", Namespace = "rcmGsProductSchema")]
        public XVelocity XVelocity { get; set; }

        [XmlElement(ElementName = "yVelocity", Namespace = "rcmGsProductSchema")]
        public YVelocity YVelocity { get; set; }

        [XmlElement(ElementName = "zVelocity", Namespace = "rcmGsProductSchema")]
        public ZVelocity ZVelocity { get; set; }
    }

    [XmlRoot(ElementName = "orbitInformation", Namespace = "rcmGsProductSchema")]
    public class OrbitInformation
    {

        [XmlElement(ElementName = "passDirection", Namespace = "rcmGsProductSchema")]
        public string PassDirection { get; set; }

        [XmlElement(ElementName = "orbitDataSource", Namespace = "rcmGsProductSchema")]
        public string OrbitDataSource { get; set; }

        [XmlElement(ElementName = "withinOrbitTube", Namespace = "rcmGsProductSchema")]
        public bool WithinOrbitTube { get; set; }

        [XmlElement(ElementName = "orbitDataFileName", Namespace = "rcmGsProductSchema")]
        public string OrbitDataFileName { get; set; }

        [XmlElement(ElementName = "stateVector", Namespace = "rcmGsProductSchema")]
        public List<StateVector> StateVector { get; set; }
    }

    [XmlRoot(ElementName = "yaw", Namespace = "rcmGsProductSchema")]
    public class Yaw
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "roll", Namespace = "rcmGsProductSchema")]
    public class Roll
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "pitch", Namespace = "rcmGsProductSchema")]
    public class Pitch
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "attitudeAngles", Namespace = "rcmGsProductSchema")]
    public class AttitudeAngles
    {

        [XmlElement(ElementName = "timeStamp", Namespace = "rcmGsProductSchema")]
        public DateTime TimeStamp { get; set; }

        [XmlElement(ElementName = "yaw", Namespace = "rcmGsProductSchema")]
        public Yaw Yaw { get; set; }

        [XmlElement(ElementName = "roll", Namespace = "rcmGsProductSchema")]
        public Roll Roll { get; set; }

        [XmlElement(ElementName = "pitch", Namespace = "rcmGsProductSchema")]
        public Pitch Pitch { get; set; }
    }

    [XmlRoot(ElementName = "attitudeInformation", Namespace = "rcmGsProductSchema")]
    public class AttitudeInformation
    {

        [XmlElement(ElementName = "attitudeDataSource", Namespace = "rcmGsProductSchema")]
        public string AttitudeDataSource { get; set; }

        [XmlElement(ElementName = "attitudeOffsetsApplied", Namespace = "rcmGsProductSchema")]
        public bool AttitudeOffsetsApplied { get; set; }

        [XmlElement(ElementName = "attitudeAngles", Namespace = "rcmGsProductSchema")]
        public List<AttitudeAngles> AttitudeAngles { get; set; }
    }

    [XmlRoot(ElementName = "orbitAndAttitude", Namespace = "rcmGsProductSchema")]
    public class OrbitAndAttitude
    {

        [XmlElement(ElementName = "orbitInformation", Namespace = "rcmGsProductSchema")]
        public OrbitInformation OrbitInformation { get; set; }

        [XmlElement(ElementName = "attitudeInformation", Namespace = "rcmGsProductSchema")]
        public AttitudeInformation AttitudeInformation { get; set; }
    }

    [XmlRoot(ElementName = "sourceAttributes", Namespace = "rcmGsProductSchema")]
    public class SourceAttributes
    {

        [XmlElement(ElementName = "satellite", Namespace = "rcmGsProductSchema")]
        public string Satellite { get; set; }

        [XmlElement(ElementName = "sensor", Namespace = "rcmGsProductSchema")]
        public string Sensor { get; set; }

        [XmlElement(ElementName = "polarizationDataMode", Namespace = "rcmGsProductSchema")]
        public string PolarizationDataMode { get; set; }

        [XmlElement(ElementName = "downlinkSegmentId", Namespace = "rcmGsProductSchema")]
        public string DownlinkSegmentId { get; set; }

        [XmlElement(ElementName = "inputDatasetFacilityId", Namespace = "rcmGsProductSchema")]
        public string InputDatasetFacilityId { get; set; }

        [XmlElement(ElementName = "beamMode", Namespace = "rcmGsProductSchema")]
        public string BeamMode { get; set; }

        [XmlElement(ElementName = "beamModeDefinitionId", Namespace = "rcmGsProductSchema")]
        public int BeamModeDefinitionId { get; set; }

        [XmlElement(ElementName = "beamModeVersion", Namespace = "rcmGsProductSchema")]
        public int BeamModeVersion { get; set; }

        [XmlElement(ElementName = "beamModeMnemonic", Namespace = "rcmGsProductSchema")]
        public string BeamModeMnemonic { get; set; }

        [XmlElement(ElementName = "rawDataStartTime", Namespace = "rcmGsProductSchema")]
        public DateTime RawDataStartTime { get; set; }

        [XmlElement(ElementName = "radarParameters", Namespace = "rcmGsProductSchema")]
        public RadarParameters RadarParameters { get; set; }

        [XmlElement(ElementName = "rawDataAttributes", Namespace = "rcmGsProductSchema")]
        public RawDataAttributes RawDataAttributes { get; set; }

        [XmlElement(ElementName = "orbitAndAttitude", Namespace = "rcmGsProductSchema")]
        public OrbitAndAttitude OrbitAndAttitude { get; set; }
    }

    [XmlRoot(ElementName = "generalProcessingInformation", Namespace = "rcmGsProductSchema")]
    public class GeneralProcessingInformation
    {

        [XmlElement(ElementName = "productType", Namespace = "rcmGsProductSchema")]
        public string ProductType { get; set; }

        [XmlElement(ElementName = "polarizationsInProduct", Namespace = "rcmGsProductSchema")]
        public string PolarizationsInProduct { get; set; }

        [XmlElement(ElementName = "processingFacility", Namespace = "rcmGsProductSchema")]
        public string ProcessingFacility { get; set; }

        [XmlElement(ElementName = "processingTime", Namespace = "rcmGsProductSchema")]
        public DateTime ProcessingTime { get; set; }

        [XmlElement(ElementName = "softwareVersion", Namespace = "rcmGsProductSchema")]
        public string SoftwareVersion { get; set; }

        [XmlElement(ElementName = "processingMode", Namespace = "rcmGsProductSchema")]
        public string ProcessingMode { get; set; }

        [XmlElement(ElementName = "processingPriority", Namespace = "rcmGsProductSchema")]
        public string ProcessingPriority { get; set; }
    }

    [XmlRoot(ElementName = "atmosphericPropagationDelay", Namespace = "rcmGsProductSchema")]
    public class AtmosphericPropagationDelay
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "numberOfLinesProcessed", Namespace = "rcmGsProductSchema")]
    public class NumberOfLinesProcessed
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "rangeLookBandwidth", Namespace = "rcmGsProductSchema")]
    public class RangeLookBandwidth
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "totalProcessedRangeBandwidth", Namespace = "rcmGsProductSchema")]
    public class TotalProcessedRangeBandwidth
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "azimuthLookBandwidth", Namespace = "rcmGsProductSchema")]
    public class AzimuthLookBandwidth
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "totalProcessedAzimuthBandwidth", Namespace = "rcmGsProductSchema")]
    public class TotalProcessedAzimuthBandwidth
    {

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "azimuthWindow", Namespace = "rcmGsProductSchema")]
    public class AzimuthWindow
    {

        [XmlElement(ElementName = "windowName", Namespace = "rcmGsProductSchema")]
        public string WindowName { get; set; }

        [XmlElement(ElementName = "windowCoefficient", Namespace = "rcmGsProductSchema")]
        public double WindowCoefficient { get; set; }

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rangeWindow", Namespace = "rcmGsProductSchema")]
    public class RangeWindow
    {

        [XmlElement(ElementName = "windowName", Namespace = "rcmGsProductSchema")]
        public string WindowName { get; set; }

        [XmlElement(ElementName = "windowCoefficient", Namespace = "rcmGsProductSchema")]
        public double WindowCoefficient { get; set; }
    }

    [XmlRoot(ElementName = "satelliteHeight", Namespace = "rcmGsProductSchema")]
    public class SatelliteHeight
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sarProcessingInformation", Namespace = "rcmGsProductSchema")]
    public class SarProcessingInformation
    {

        [XmlElement(ElementName = "lutApplied", Namespace = "rcmGsProductSchema")]
        public string LutApplied { get; set; }

        [XmlElement(ElementName = "perPolarizationScaling", Namespace = "rcmGsProductSchema")]
        public bool PerPolarizationScaling { get; set; }

        [XmlElement(ElementName = "atmosphericCorrection", Namespace = "rcmGsProductSchema")]
        public bool AtmosphericCorrection { get; set; }

        [XmlElement(ElementName = "elevationPatternCorrection", Namespace = "rcmGsProductSchema")]
        public bool ElevationPatternCorrection { get; set; }

        [XmlElement(ElementName = "rangeSpreadingLossCorrection", Namespace = "rcmGsProductSchema")]
        public bool RangeSpreadingLossCorrection { get; set; }

        [XmlElement(ElementName = "pulseDependentGainCorrection", Namespace = "rcmGsProductSchema")]
        public bool PulseDependentGainCorrection { get; set; }

        [XmlElement(ElementName = "receiverSettableGainCorrection", Namespace = "rcmGsProductSchema")]
        public bool ReceiverSettableGainCorrection { get; set; }

        [XmlElement(ElementName = "rawDataCorrection", Namespace = "rcmGsProductSchema")]
        public bool RawDataCorrection { get; set; }

        [XmlElement(ElementName = "bistaticCorrectionApplied", Namespace = "rcmGsProductSchema")]
        public bool BistaticCorrectionApplied { get; set; }

        [XmlElement(ElementName = "rangeReferenceFunctionSource", Namespace = "rcmGsProductSchema")]
        public string RangeReferenceFunctionSource { get; set; }

        [XmlElement(ElementName = "atmosphericPropagationDelay", Namespace = "rcmGsProductSchema")]
        public AtmosphericPropagationDelay AtmosphericPropagationDelay { get; set; }

        [XmlElement(ElementName = "dopplerSource", Namespace = "rcmGsProductSchema")]
        public string DopplerSource { get; set; }

        [XmlElement(ElementName = "estimatedRollAngleUsed", Namespace = "rcmGsProductSchema")]
        public bool EstimatedRollAngleUsed { get; set; }

        [XmlElement(ElementName = "zeroDopplerTimeFirstLine", Namespace = "rcmGsProductSchema")]
        public DateTime ZeroDopplerTimeFirstLine { get; set; }

        [XmlElement(ElementName = "zeroDopplerTimeLastLine", Namespace = "rcmGsProductSchema")]
        public DateTime ZeroDopplerTimeLastLine { get; set; }

        [XmlElement(ElementName = "numberOfLinesProcessed", Namespace = "rcmGsProductSchema")]
        public List<NumberOfLinesProcessed> NumberOfLinesProcessed { get; set; }

        [XmlElement(ElementName = "numberOfRangeLooks", Namespace = "rcmGsProductSchema")]
        public int NumberOfRangeLooks { get; set; }

        [XmlElement(ElementName = "rangeLookBandwidth", Namespace = "rcmGsProductSchema")]
        public RangeLookBandwidth RangeLookBandwidth { get; set; }

        [XmlElement(ElementName = "totalProcessedRangeBandwidth", Namespace = "rcmGsProductSchema")]
        public TotalProcessedRangeBandwidth TotalProcessedRangeBandwidth { get; set; }

        [XmlElement(ElementName = "numberOfAzimuthLooks", Namespace = "rcmGsProductSchema")]
        public int NumberOfAzimuthLooks { get; set; }

        [XmlElement(ElementName = "scalarLookWeights", Namespace = "rcmGsProductSchema")]
        public string ScalarLookWeights { get; set; }

        [XmlElement(ElementName = "azimuthLookBandwidth", Namespace = "rcmGsProductSchema")]
        public AzimuthLookBandwidth AzimuthLookBandwidth { get; set; }

        [XmlElement(ElementName = "totalProcessedAzimuthBandwidth", Namespace = "rcmGsProductSchema")]
        public TotalProcessedAzimuthBandwidth TotalProcessedAzimuthBandwidth { get; set; }

        [XmlElement(ElementName = "azimuthWindow", Namespace = "rcmGsProductSchema")]
        public AzimuthWindow AzimuthWindow { get; set; }

        [XmlElement(ElementName = "rangeWindow", Namespace = "rcmGsProductSchema")]
        public RangeWindow RangeWindow { get; set; }

        [XmlElement(ElementName = "satelliteHeight", Namespace = "rcmGsProductSchema")]
        public SatelliteHeight SatelliteHeight { get; set; }
    }

    [XmlRoot(ElementName = "sideLobeLevel", Namespace = "rcmGsProductSchema")]
    public class SideLobeLevel
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "integratedSideLobeRatio", Namespace = "rcmGsProductSchema")]
    public class IntegratedSideLobeRatio
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "chirpQuality", Namespace = "rcmGsProductSchema")]
    public class ChirpQuality
    {

        [XmlElement(ElementName = "replicaQualityValid", Namespace = "rcmGsProductSchema")]
        public bool ReplicaQualityValid { get; set; }

        [XmlElement(ElementName = "crossCorrelationWidth", Namespace = "rcmGsProductSchema")]
        public double CrossCorrelationWidth { get; set; }

        [XmlElement(ElementName = "sideLobeLevel", Namespace = "rcmGsProductSchema")]
        public SideLobeLevel SideLobeLevel { get; set; }

        [XmlElement(ElementName = "integratedSideLobeRatio", Namespace = "rcmGsProductSchema")]
        public IntegratedSideLobeRatio IntegratedSideLobeRatio { get; set; }

        [XmlElement(ElementName = "crossCorrelationPeakLoc", Namespace = "rcmGsProductSchema")]
        public double CrossCorrelationPeakLoc { get; set; }
    }

    [XmlRoot(ElementName = "chirpPower", Namespace = "rcmGsProductSchema")]
    public class ChirpPower
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "chirp", Namespace = "rcmGsProductSchema")]
    public class Chirp
    {

        [XmlElement(ElementName = "chirpQuality", Namespace = "rcmGsProductSchema")]
        public ChirpQuality ChirpQuality { get; set; }

        [XmlElement(ElementName = "chirpPower", Namespace = "rcmGsProductSchema")]
        public ChirpPower ChirpPower { get; set; }

        [XmlElement(ElementName = "amplitudeCoefficients", Namespace = "rcmGsProductSchema")]
        public string AmplitudeCoefficients { get; set; }

        [XmlElement(ElementName = "phaseCoefficients", Namespace = "rcmGsProductSchema")]
        public string PhaseCoefficients { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlAttribute(AttributeName = "pulse", Namespace = "")]
        public int Pulse { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "slantRangeTimeToFirstRangeSample", Namespace = "rcmGsProductSchema")]
    public class SlantRangeTimeToFirstRangeSample
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "groundRangeOrigin", Namespace = "rcmGsProductSchema")]
    public class GroundRangeOrigin
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "slantRangeToGroundRange", Namespace = "rcmGsProductSchema")]
    public class SlantRangeToGroundRange
    {

        [XmlElement(ElementName = "zeroDopplerAzimuthTime", Namespace = "rcmGsProductSchema")]
        public DateTime ZeroDopplerAzimuthTime { get; set; }

        [XmlElement(ElementName = "slantRangeTimeToFirstRangeSample", Namespace = "rcmGsProductSchema")]
        public SlantRangeTimeToFirstRangeSample SlantRangeTimeToFirstRangeSample { get; set; }

        [XmlElement(ElementName = "groundRangeOrigin", Namespace = "rcmGsProductSchema")]
        public GroundRangeOrigin GroundRangeOrigin { get; set; }

        [XmlElement(ElementName = "groundToSlantRangeCoefficients", Namespace = "rcmGsProductSchema")]
        public string GroundToSlantRangeCoefficients { get; set; }
    }

    [XmlRoot(ElementName = "imageGenerationParameters", Namespace = "rcmGsProductSchema")]
    public class ImageGenerationParameters
    {

        [XmlElement(ElementName = "generalProcessingInformation", Namespace = "rcmGsProductSchema")]
        public GeneralProcessingInformation GeneralProcessingInformation { get; set; }

        [XmlElement(ElementName = "sarProcessingInformation", Namespace = "rcmGsProductSchema")]
        public SarProcessingInformation SarProcessingInformation { get; set; }

        [XmlElement(ElementName = "chirp", Namespace = "rcmGsProductSchema")]
        public List<Chirp> Chirp { get; set; }

        [XmlElement(ElementName = "slantRangeToGroundRange", Namespace = "rcmGsProductSchema")]
        public SlantRangeToGroundRange SlantRangeToGroundRange { get; set; }

        [XmlElement(ElementName = "dynamicProcessingParameterFile", Namespace = "rcmGsProductSchema")]
        public List<string> DynamicProcessingParameterFile { get; set; }
    }

    [XmlRoot(ElementName = "lookupTableFileName", Namespace = "rcmGsProductSchema")]
    public class LookupTableFileName
    {

        [XmlAttribute(AttributeName = "sarCalibrationType", Namespace = "")]
        public string SarCalibrationType { get; set; }

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "noiseLevelFileName", Namespace = "rcmGsProductSchema")]
    public class NoiseLevelFileName
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "bitsPerSample", Namespace = "rcmGsProductSchema")]
    public class BitsPerSample
    {

        [XmlAttribute(AttributeName = "dataStream", Namespace = "")]
        public string DataStream { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "sampledPixelSpacing", Namespace = "rcmGsProductSchema")]
    public class SampledPixelSpacing
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sampledLineSpacing", Namespace = "rcmGsProductSchema")]
    public class SampledLineSpacing
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sampledPixelSpacingTime", Namespace = "rcmGsProductSchema")]
    public class SampledPixelSpacingTime
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sampledLineSpacingTime", Namespace = "rcmGsProductSchema")]
    public class SampledLineSpacingTime
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rasterAttributes", Namespace = "rcmGsProductSchema")]
    public class RasterAttributes
    {

        [XmlElement(ElementName = "sampleType", Namespace = "rcmGsProductSchema")]
        public string SampleType { get; set; }

        [XmlElement(ElementName = "dataType", Namespace = "rcmGsProductSchema")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "bitsPerSample", Namespace = "rcmGsProductSchema")]
        public BitsPerSample BitsPerSample { get; set; }

        [XmlElement(ElementName = "sampledPixelSpacing", Namespace = "rcmGsProductSchema")]
        public SampledPixelSpacing SampledPixelSpacing { get; set; }

        [XmlElement(ElementName = "sampledLineSpacing", Namespace = "rcmGsProductSchema")]
        public SampledLineSpacing SampledLineSpacing { get; set; }

        [XmlElement(ElementName = "sampledPixelSpacingTime", Namespace = "rcmGsProductSchema")]
        public SampledPixelSpacingTime SampledPixelSpacingTime { get; set; }

        [XmlElement(ElementName = "sampledLineSpacingTime", Namespace = "rcmGsProductSchema")]
        public SampledLineSpacingTime SampledLineSpacingTime { get; set; }

        [XmlElement(ElementName = "lineTimeOrdering", Namespace = "rcmGsProductSchema")]
        public string LineTimeOrdering { get; set; }

        [XmlElement(ElementName = "pixelTimeOrdering", Namespace = "rcmGsProductSchema")]
        public string PixelTimeOrdering { get; set; }
    }

    [XmlRoot(ElementName = "semiMajorAxis", Namespace = "rcmGsProductSchema")]
    public class SemiMajorAxis
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "semiMinorAxis", Namespace = "rcmGsProductSchema")]
    public class SemiMinorAxis
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "datumShiftParameters", Namespace = "rcmGsProductSchema")]
    public class DatumShiftParameters
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "geodeticTerrainHeight", Namespace = "rcmGsProductSchema")]
    public class GeodeticTerrainHeight
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ellipsoidParameters", Namespace = "rcmGsProductSchema")]
    public class EllipsoidParameters
    {

        [XmlElement(ElementName = "ellipsoidName", Namespace = "rcmGsProductSchema")]
        public string EllipsoidName { get; set; }

        [XmlElement(ElementName = "semiMajorAxis", Namespace = "rcmGsProductSchema")]
        public SemiMajorAxis SemiMajorAxis { get; set; }

        [XmlElement(ElementName = "semiMinorAxis", Namespace = "rcmGsProductSchema")]
        public SemiMinorAxis SemiMinorAxis { get; set; }

        [XmlElement(ElementName = "datumShiftParameters", Namespace = "rcmGsProductSchema")]
        public DatumShiftParameters DatumShiftParameters { get; set; }

        [XmlElement(ElementName = "geodeticTerrainHeight", Namespace = "rcmGsProductSchema")]
        public GeodeticTerrainHeight GeodeticTerrainHeight { get; set; }
    }

    [XmlRoot(ElementName = "imageCoordinate", Namespace = "rcmGsProductSchema")]
    public class ImageCoordinate
    {

        [XmlElement(ElementName = "line", Namespace = "rcmGsProductSchema")]
        public double Line { get; set; }

        [XmlElement(ElementName = "pixel", Namespace = "rcmGsProductSchema")]
        public double Pixel { get; set; }
    }

    [XmlRoot(ElementName = "latitude", Namespace = "rcmGsProductSchema")]
    public class Latitude
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "longitude", Namespace = "rcmGsProductSchema")]
    public class Longitude
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "height", Namespace = "rcmGsProductSchema")]
    public class Height
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "geodeticCoordinate", Namespace = "rcmGsProductSchema")]
    public class GeodeticCoordinate
    {

        [XmlElement(ElementName = "latitude", Namespace = "rcmGsProductSchema")]
        public Latitude Latitude { get; set; }

        [XmlElement(ElementName = "longitude", Namespace = "rcmGsProductSchema")]
        public Longitude Longitude { get; set; }

        [XmlElement(ElementName = "height", Namespace = "rcmGsProductSchema")]
        public Height Height { get; set; }
    }

    [XmlRoot(ElementName = "imageTiePoint", Namespace = "rcmGsProductSchema")]
    public class ImageTiePoint
    {

        [XmlElement(ElementName = "imageCoordinate", Namespace = "rcmGsProductSchema")]
        public ImageCoordinate ImageCoordinate { get; set; }

        [XmlElement(ElementName = "geodeticCoordinate", Namespace = "rcmGsProductSchema")]
        public GeodeticCoordinate GeodeticCoordinate { get; set; }
    }

    [XmlRoot(ElementName = "geolocationGrid", Namespace = "rcmGsProductSchema")]
    public class GeolocationGrid
    {

        [XmlElement(ElementName = "imageTiePoint", Namespace = "rcmGsProductSchema")]
        public List<ImageTiePoint> ImageTiePoint { get; set; }
    }

    [XmlRoot(ElementName = "biasError", Namespace = "rcmGsProductSchema")]
    public class BiasError
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "randomError", Namespace = "rcmGsProductSchema")]
    public class RandomError
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "latitudeOffset", Namespace = "rcmGsProductSchema")]
    public class LatitudeOffset
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "longitudeOffset", Namespace = "rcmGsProductSchema")]
    public class LongitudeOffset
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "heightOffset", Namespace = "rcmGsProductSchema")]
    public class HeightOffset
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "rationalFunctions", Namespace = "rcmGsProductSchema")]
    public class RationalFunctions
    {

        [XmlElement(ElementName = "biasError", Namespace = "rcmGsProductSchema")]
        public BiasError BiasError { get; set; }

        [XmlElement(ElementName = "randomError", Namespace = "rcmGsProductSchema")]
        public RandomError RandomError { get; set; }

        [XmlElement(ElementName = "lineFitQuality", Namespace = "rcmGsProductSchema")]
        public double LineFitQuality { get; set; }

        [XmlElement(ElementName = "pixelFitQuality", Namespace = "rcmGsProductSchema")]
        public double PixelFitQuality { get; set; }

        [XmlElement(ElementName = "lineOffset", Namespace = "rcmGsProductSchema")]
        public int LineOffset { get; set; }

        [XmlElement(ElementName = "pixelOffset", Namespace = "rcmGsProductSchema")]
        public int PixelOffset { get; set; }

        [XmlElement(ElementName = "latitudeOffset", Namespace = "rcmGsProductSchema")]
        public LatitudeOffset LatitudeOffset { get; set; }

        [XmlElement(ElementName = "longitudeOffset", Namespace = "rcmGsProductSchema")]
        public LongitudeOffset LongitudeOffset { get; set; }

        [XmlElement(ElementName = "heightOffset", Namespace = "rcmGsProductSchema")]
        public HeightOffset HeightOffset { get; set; }

        [XmlElement(ElementName = "lineScale", Namespace = "rcmGsProductSchema")]
        public int LineScale { get; set; }

        [XmlElement(ElementName = "pixelScale", Namespace = "rcmGsProductSchema")]
        public int PixelScale { get; set; }

        [XmlElement(ElementName = "latitudeScale", Namespace = "rcmGsProductSchema")]
        public double LatitudeScale { get; set; }

        [XmlElement(ElementName = "longitudeScale", Namespace = "rcmGsProductSchema")]
        public double LongitudeScale { get; set; }

        [XmlElement(ElementName = "heightScale", Namespace = "rcmGsProductSchema")]
        public double HeightScale { get; set; }

        [XmlElement(ElementName = "lineNumeratorCoefficients", Namespace = "rcmGsProductSchema")]
        public string LineNumeratorCoefficients { get; set; }

        [XmlElement(ElementName = "lineDenominatorCoefficients", Namespace = "rcmGsProductSchema")]
        public string LineDenominatorCoefficients { get; set; }

        [XmlElement(ElementName = "pixelNumeratorCoefficients", Namespace = "rcmGsProductSchema")]
        public string PixelNumeratorCoefficients { get; set; }

        [XmlElement(ElementName = "pixelDenominatorCoefficients", Namespace = "rcmGsProductSchema")]
        public string PixelDenominatorCoefficients { get; set; }
    }

    [XmlRoot(ElementName = "geographicInformation", Namespace = "rcmGsProductSchema")]
    public class GeographicInformation
    {

        [XmlElement(ElementName = "ellipsoidParameters", Namespace = "rcmGsProductSchema")]
        public EllipsoidParameters EllipsoidParameters { get; set; }

        [XmlElement(ElementName = "geolocationGrid", Namespace = "rcmGsProductSchema")]
        public GeolocationGrid GeolocationGrid { get; set; }

        [XmlElement(ElementName = "rationalFunctions", Namespace = "rcmGsProductSchema")]
        public RationalFunctions RationalFunctions { get; set; }

        [XmlElement(ElementName = "mapProjection", Namespace = "rcmGsProductSchema")]
        public MapProjection mapProjection { get; set; }
    }

    [XmlRoot(ElementName = "mapProjection", Namespace = "rcmGsProductSchema")]
    public class MapProjection
    {

        [XmlElement(ElementName = "utmProjectionParameters", Namespace = "rcmGsProductSchema")]
        public UtmProjectionParameters utmProjectionParameters { get; set; }
    }

    [XmlRoot(ElementName = "utmProjectionParameters", Namespace = "rcmGsProductSchema")]
    public class UtmProjectionParameters
    {

        [XmlElement(ElementName = "utmZone", Namespace = "rcmGsProductSchema")]
        public int utmZone { get; set; }

        [XmlElement(ElementName = "hemisphere", Namespace = "rcmGsProductSchema")]
        public string hemisphere { get; set; }
    }

    [XmlRoot(ElementName = "imageReferenceAttributes", Namespace = "rcmGsProductSchema")]
    public class ImageReferenceAttributes
    {

        [XmlElement(ElementName = "productFormat", Namespace = "rcmGsProductSchema")]
        public string ProductFormat { get; set; }

        [XmlElement(ElementName = "outputMediaInterleaving", Namespace = "rcmGsProductSchema")]
        public string OutputMediaInterleaving { get; set; }

        [XmlElement(ElementName = "lookupTableFileName", Namespace = "rcmGsProductSchema")]
        public List<LookupTableFileName> LookupTableFileName { get; set; }

        [XmlElement(ElementName = "incidenceAngleFileName", Namespace = "rcmGsProductSchema")]
        public string IncidenceAngleFileName { get; set; }

        [XmlElement(ElementName = "noiseLevelFileName", Namespace = "rcmGsProductSchema")]
        public List<NoiseLevelFileName> NoiseLevelFileName { get; set; }

        [XmlElement(ElementName = "rasterAttributes", Namespace = "rcmGsProductSchema")]
        public RasterAttributes RasterAttributes { get; set; }

        [XmlElement(ElementName = "geographicInformation", Namespace = "rcmGsProductSchema")]
        public GeographicInformation GeographicInformation { get; set; }
    }

    [XmlRoot(ElementName = "ipdf", Namespace = "rcmGsProductSchema")]
    public class Ipdf
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "incAngNearRng", Namespace = "rcmGsProductSchema")]
    public class IncAngNearRng
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "incAngFarRng", Namespace = "rcmGsProductSchema")]
    public class IncAngFarRng
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "slantRangeNearEdge", Namespace = "rcmGsProductSchema")]
    public class SlantRangeNearEdge
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "slantRangeFarEdge", Namespace = "rcmGsProductSchema")]
    public class SlantRangeFarEdge
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "mean", Namespace = "rcmGsProductSchema")]
    public class Mean
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sigma", Namespace = "rcmGsProductSchema")]
    public class Sigma
    {

        [XmlAttribute(AttributeName = "pole", Namespace = "")]
        public string Pole { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "imageAttributes", Namespace = "rcmGsProductSchema")]
    public class ImageAttributes
    {

        [XmlElement(ElementName = "ipdf", Namespace = "rcmGsProductSchema")]
        public List<Ipdf> Ipdf { get; set; }

        [XmlElement(ElementName = "pixelOffset", Namespace = "rcmGsProductSchema")]
        public int PixelOffset { get; set; }

        [XmlElement(ElementName = "lineOffset", Namespace = "rcmGsProductSchema")]
        public int LineOffset { get; set; }

        [XmlElement(ElementName = "numLines", Namespace = "rcmGsProductSchema")]
        public int NumLines { get; set; }

        [XmlElement(ElementName = "samplesPerLine", Namespace = "rcmGsProductSchema")]
        public int SamplesPerLine { get; set; }

        [XmlElement(ElementName = "incAngNearRng", Namespace = "rcmGsProductSchema")]
        public IncAngNearRng IncAngNearRng { get; set; }

        [XmlElement(ElementName = "incAngFarRng", Namespace = "rcmGsProductSchema")]
        public IncAngFarRng IncAngFarRng { get; set; }

        [XmlElement(ElementName = "slantRangeNearEdge", Namespace = "rcmGsProductSchema")]
        public SlantRangeNearEdge SlantRangeNearEdge { get; set; }

        [XmlElement(ElementName = "slantRangeFarEdge", Namespace = "rcmGsProductSchema")]
        public SlantRangeFarEdge SlantRangeFarEdge { get; set; }

        [XmlElement(ElementName = "mean", Namespace = "rcmGsProductSchema")]
        public List<Mean> Mean { get; set; }

        [XmlElement(ElementName = "sigma", Namespace = "rcmGsProductSchema")]
        public List<Sigma> Sigma { get; set; }

        [XmlAttribute(AttributeName = "sampleType", Namespace = "")]
        public string SampleType { get; set; }

        [XmlAttribute(AttributeName = "beam", Namespace = "")]
        public string Beam { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "sceneAttributes", Namespace = "rcmGsProductSchema")]
    public class SceneAttributes
    {

        [XmlElement(ElementName = "numberOfEntries", Namespace = "rcmGsProductSchema")]
        public int NumberOfEntries { get; set; }

        [XmlElement(ElementName = "imageAttributes", Namespace = "rcmGsProductSchema")]
        public ImageAttributes ImageAttributes { get; set; }
    }

    [XmlRoot(ElementName = "dopplerCentroidReferenceTime", Namespace = "rcmGsProductSchema")]
    public class DopplerCentroidReferenceTime
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "dopplerCentroidEstimate", Namespace = "rcmGsProductSchema")]
    public class DopplerCentroidEstimate
    {

        [XmlElement(ElementName = "timeOfDopplerCentroidEstimate", Namespace = "rcmGsProductSchema")]
        public DateTime TimeOfDopplerCentroidEstimate { get; set; }

        [XmlElement(ElementName = "dopplerAmbiguity", Namespace = "rcmGsProductSchema")]
        public int DopplerAmbiguity { get; set; }

        [XmlElement(ElementName = "dopplerCentroidReferenceTime", Namespace = "rcmGsProductSchema")]
        public DopplerCentroidReferenceTime DopplerCentroidReferenceTime { get; set; }

        [XmlElement(ElementName = "dopplerCentroidCoefficients", Namespace = "rcmGsProductSchema")]
        public string DopplerCentroidCoefficients { get; set; }

        [XmlElement(ElementName = "dopplerCentroidConfidence", Namespace = "rcmGsProductSchema")]
        public double DopplerCentroidConfidence { get; set; }
    }

    [XmlRoot(ElementName = "dopplerCentroid", Namespace = "rcmGsProductSchema")]
    public class DopplerCentroid
    {

        [XmlElement(ElementName = "numberOfEstimates", Namespace = "rcmGsProductSchema")]
        public int NumberOfEstimates { get; set; }

        [XmlElement(ElementName = "dopplerCentroidEstimate", Namespace = "rcmGsProductSchema")]
        public DopplerCentroidEstimate DopplerCentroidEstimate { get; set; }
    }

    [XmlRoot(ElementName = "dopplerRateReferenceTime", Namespace = "rcmGsProductSchema")]
    public class DopplerRateReferenceTime
    {

        [XmlAttribute(AttributeName = "units", Namespace = "")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "dopplerRateEstimate", Namespace = "rcmGsProductSchema")]
    public class DopplerRateEstimate
    {

        [XmlElement(ElementName = "timeOfDopplerRateEstimate", Namespace = "rcmGsProductSchema")]
        public DateTime TimeOfDopplerRateEstimate { get; set; }

        [XmlElement(ElementName = "dopplerRateReferenceTime", Namespace = "rcmGsProductSchema")]
        public DopplerRateReferenceTime DopplerRateReferenceTime { get; set; }

        [XmlElement(ElementName = "dopplerRateCoefficients", Namespace = "rcmGsProductSchema")]
        public string DopplerRateCoefficients { get; set; }
    }

    [XmlRoot(ElementName = "dopplerRate", Namespace = "rcmGsProductSchema")]
    public class DopplerRate
    {

        [XmlElement(ElementName = "numberOfEstimates", Namespace = "rcmGsProductSchema")]
        public int NumberOfEstimates { get; set; }

        [XmlElement(ElementName = "dopplerRateEstimate", Namespace = "rcmGsProductSchema")]
        public DopplerRateEstimate DopplerRateEstimate { get; set; }
    }

    [XmlRoot(ElementName = "product", Namespace = "rcmGsProductSchema")]
    public class Product
    {

        [XmlElement(ElementName = "productId", Namespace = "rcmGsProductSchema")]
        public string ProductId { get; set; }

        [XmlElement(ElementName = "productAnnotation", Namespace = "rcmGsProductSchema")]
        public string ProductAnnotation { get; set; }

        [XmlElement(ElementName = "productApplication", Namespace = "rcmGsProductSchema")]
        public string ProductApplication { get; set; }

        [XmlElement(ElementName = "documentIdentifier", Namespace = "rcmGsProductSchema")]
        public string DocumentIdentifier { get; set; }

        [XmlElement(ElementName = "securityAttributes", Namespace = "rcmGsProductSchema")]
        public SecurityAttributes SecurityAttributes { get; set; }

        [XmlElement(ElementName = "sourceAttributes", Namespace = "rcmGsProductSchema")]
        public SourceAttributes SourceAttributes { get; set; }

        [XmlElement(ElementName = "imageGenerationParameters", Namespace = "rcmGsProductSchema")]
        public ImageGenerationParameters ImageGenerationParameters { get; set; }

        [XmlElement(ElementName = "imageReferenceAttributes", Namespace = "rcmGsProductSchema")]
        public ImageReferenceAttributes ImageReferenceAttributes { get; set; }

        [XmlElement(ElementName = "sceneAttributes", Namespace = "rcmGsProductSchema")]
        public SceneAttributes SceneAttributes { get; set; }

        [XmlElement(ElementName = "dopplerCentroid", Namespace = "rcmGsProductSchema")]
        public DopplerCentroid DopplerCentroid { get; set; }

        [XmlElement(ElementName = "dopplerRate", Namespace = "rcmGsProductSchema")]
        public DopplerRate DopplerRate { get; set; }

        [XmlAttribute(AttributeName = "xmlns", Namespace = "")]
        public string Xmlns { get; set; }

        [XmlAttribute(AttributeName = "copyright", Namespace = "")]
        public string Copyright { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }

        [XmlText]
        public string Text { get; set; }
    }



    // public class Auxiliary : Product {}

    public class BEAM_MODE
    {
        public const string LOW_RESOLUTION_100 = "Low Resolution 100 m";
        public const string MEDIUM_RESOLUTION_50 = "Medium Resolution 50 m";
        public const string MEDIUM_RESOLUTION_30 = "Medium Resolution 30 m";
        public const string MEDIUM_RESOLUTION_16 = "Medium Resolution 16 m";
        public const string HIGH_RESOLUTION_5 = "High Resolution 5 m";
        public const string VERY_HIGH_RESOLUTION_3 = "Very High Resolution 3 m";
        public const string LOW_NOISE = "Low Noise";
        public const string SHIP_DETECTION = "Ship Detection";
        public const string SPOTLIGHT = "Spotlight";
        public const string QUAD_POLARIZATION = "Quad-Polarization";
    }

    public class ORBIT_STATE
    {
        public const string ASCENDING = "Ascending";
        public const string DESCENDING = "Descending";
    }

}
