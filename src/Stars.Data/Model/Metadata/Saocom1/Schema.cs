using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Terradue.Stars.Data.Model.Metadata.Saocom1
{
    [XmlRoot(ElementName = "LinesStep")]
    public class LinesStep
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SamplesStep")]
    public class SamplesStep
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "LinesStart")]
    public class LinesStart
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SamplesStart")]
    public class SamplesStart
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "RasterInfo")]
    public class RasterInfo
    {
        [XmlElement(ElementName = "FileName")]
        public string FileName { get; set; }
        [XmlElement(ElementName = "Lines")]
        public int Lines { get; set; }
        [XmlElement(ElementName = "Samples")]
        public int Samples { get; set; }
        [XmlElement(ElementName = "HeaderOffsetBytes")]
        public string HeaderOffsetBytes { get; set; }
        [XmlElement(ElementName = "RowPrefixBytes")]
        public string RowPrefixBytes { get; set; }
        [XmlElement(ElementName = "ByteOrder")]
        public string ByteOrder { get; set; }
        [XmlElement(ElementName = "CellType")]
        public string CellType { get; set; }
        [XmlElement(ElementName = "LinesStep")]
        public LinesStep LinesStep { get; set; }
        [XmlElement(ElementName = "SamplesStep")]
        public SamplesStep SamplesStep { get; set; }
        [XmlElement(ElementName = "LinesStart")]
        public LinesStart LinesStart { get; set; }
        [XmlElement(ElementName = "SamplesStart")]
        public SamplesStart SamplesStart { get; set; }
        [XmlElement(ElementName = "InvalidSample")]
        public string InvalidSample { get; set; }
        [XmlElement(ElementName = "RasterFormat")]
        public string RasterFormat { get; set; }
    }

    [XmlRoot(ElementName = "ProjectionParameters")]
    public class ProjectionParameters
    {
        [XmlAttribute(AttributeName = "Format")]
        public string Format { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "DataSetInfo")]
    public class DataSetInfo
    {
        [XmlElement(ElementName = "SensorName")]
        public string SensorName { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "SenseDate")]
        public string SenseDate { get; set; }
        [XmlElement(ElementName = "AcquisitionMode")]
        public string AcquisitionMode { get; set; }
        [XmlElement(ElementName = "ImageType")]
        public string ImageType { get; set; }
        [XmlElement(ElementName = "Projection")]
        public string Projection { get; set; }
        [XmlElement(ElementName = "ProjectionParameters")]
        public ProjectionParameters ProjectionParameters { get; set; }
        [XmlElement(ElementName = "AcquisitionStation")]
        public string AcquisitionStation { get; set; }
        [XmlElement(ElementName = "ProcessingCenter")]
        public string ProcessingCenter { get; set; }
        [XmlElement(ElementName = "ProcessingDate")]
        public string ProcessingDate { get; set; }
        [XmlElement(ElementName = "ProcessingSoftware")]
        public string ProcessingSoftware { get; set; }
        [XmlElement(ElementName = "fc_hz")]
        public string Fc_hz { get; set; }
        [XmlElement(ElementName = "SideLooking")]
        public string SideLooking { get; set; }
    }

    [XmlRoot(ElementName = "AzimuthSteeringRateReferenceTime")]
    public class AzimuthSteeringRateReferenceTime
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "val")]
    public class Val
    {
        [XmlAttribute(AttributeName = "N")]
        public string N { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "AzimuthSteeringRatePol")]
    public class AzimuthSteeringRatePol
    {
        [XmlElement(ElementName = "val")]
        public List<Val> Val { get; set; }
    }

    [XmlRoot(ElementName = "SwathInfo")]
    public class SwathInfo
    {
        [XmlElement(ElementName = "Swath")]
        public string Swath { get; set; }
        [XmlElement(ElementName = "SwathAcquisitionOrder")]
        public string SwathAcquisitionOrder { get; set; }
        [XmlElement(ElementName = "Polarization")]
        public string Polarization { get; set; }
        [XmlElement(ElementName = "Rank")]
        public string Rank { get; set; }
        [XmlElement(ElementName = "RangeDelayBias")]
        public string RangeDelayBias { get; set; }
        [XmlElement(ElementName = "AcquisitionStartTime")]
        public string AcquisitionStartTime { get; set; }
        [XmlElement(ElementName = "AzimuthSteeringRateReferenceTime")]
        public AzimuthSteeringRateReferenceTime AzimuthSteeringRateReferenceTime { get; set; }
        [XmlElement(ElementName = "AzimuthSteeringRatePol")]
        public AzimuthSteeringRatePol AzimuthSteeringRatePol { get; set; }
        [XmlElement(ElementName = "AcquisitionPRF")]
        public string AcquisitionPRF { get; set; }
        [XmlElement(ElementName = "EchoesPerBurst")]
        public string EchoesPerBurst { get; set; }
        [XmlElement(ElementName = "NominalResolution")]
        public NominalResolution NominalResolution { get; set; }
    }

    [XmlRoot(ElementName = "NominalResolution")]
    public class NominalResolution
    {
        [XmlElement(ElementName = "Azimuth")]
        public UnitValueDouble Azimuth { get; set; }
        [XmlElement(ElementName = "Range")]
        public UnitValueDouble Range { get; set; }
    }

    public class UnitValueDouble
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public double Value { get; set; }
    }

    [XmlRoot(ElementName = "SamplingConstants")]
    public class SamplingConstants
    {
        [XmlElement(ElementName = "frg_hz")]
        public UnitValueDouble frg_hz { get; set; }
        [XmlElement(ElementName = "Brg_hz")]
        public UnitValueDouble Brg_hz { get; set; }
        [XmlElement(ElementName = "PSrg_m")]
        public UnitValueDouble PSrg_m { get; set; }
        [XmlElement(ElementName = "faz_hz")]
        public UnitValueDouble faz_hz { get; set; }
        [XmlElement(ElementName = "Baz_hz")]
        public UnitValueDouble Baz_hz { get; set; }
        [XmlElement(ElementName = "PSaz_m")]
        public UnitValueDouble PSaz_m { get; set; }
    }

    [XmlRoot(ElementName = "DataStatistics")]
    public class DataStatistics
    {
        [XmlElement(ElementName = "NumSamples")]
        public string NumSamples { get; set; }
        [XmlElement(ElementName = "MaxI")]
        public string MaxI { get; set; }
        [XmlElement(ElementName = "MinI")]
        public string MinI { get; set; }
        [XmlElement(ElementName = "MaxQ")]
        public string MaxQ { get; set; }
        [XmlElement(ElementName = "MinQ")]
        public string MinQ { get; set; }
        [XmlElement(ElementName = "SumI")]
        public string SumI { get; set; }
        [XmlElement(ElementName = "SumQ")]
        public string SumQ { get; set; }
        [XmlElement(ElementName = "Sum2I")]
        public string Sum2I { get; set; }
        [XmlElement(ElementName = "Sum2Q")]
        public string Sum2Q { get; set; }
        [XmlElement(ElementName = "StdDevI")]
        public string StdDevI { get; set; }
        [XmlElement(ElementName = "StdDevQ")]
        public string StdDevQ { get; set; }
    }

    [XmlRoot(ElementName = "pSV_m")]
    public class PSV_m
    {
        [XmlElement(ElementName = "val")]
        public List<Val> Val { get; set; }
    }

    [XmlRoot(ElementName = "vSV_mOs")]
    public class VSV_mOs
    {
        [XmlElement(ElementName = "val")]
        public List<Val> Val { get; set; }
    }

    [XmlRoot(ElementName = "StateVectorData")]
    public class StateVectorData
    {
        [XmlElement(ElementName = "OrbitNumber")]
        public string OrbitNumber { get; set; }
        [XmlElement(ElementName = "Track")]
        public string Track { get; set; }
        [XmlElement(ElementName = "OrbitDirection")]
        public string OrbitDirection { get; set; }
        [XmlElement(ElementName = "pSV_m")]
        public PSV_m PSV_m { get; set; }
        [XmlElement(ElementName = "vSV_mOs")]
        public VSV_mOs VSV_mOs { get; set; }
        [XmlElement(ElementName = "t_ref_Utc")]
        public string T_ref_Utc { get; set; }
        [XmlElement(ElementName = "dtSV_s")]
        public string DtSV_s { get; set; }
        [XmlElement(ElementName = "nSV_n")]
        public string NSV_n { get; set; }
    }

    [XmlRoot(ElementName = "Point")]
    public class Point
    {
        [XmlElement(ElementName = "val")]
        public List<Val> Val { get; set; }
    }

    public class Coordinates
    {
        [XmlElement(ElementName = "Point")]
        public Point Point { get; set; }
    }

    [XmlRoot(ElementName = "GroundCornerPoints")]
    public class GroundCornerPoints
    {
        [XmlElement(ElementName = "EastingGridSize")]
        public string EastingGridSize { get; set; }
        [XmlElement(ElementName = "NorthingGridSize")]
        public string NorthingGridSize { get; set; }
        [XmlElement(ElementName = "NorthWest")]
        public Coordinates NorthWest { get; set; }
        [XmlElement(ElementName = "NorthEast")]
        public Coordinates NorthEast { get; set; }
        [XmlElement(ElementName = "SouthWest")]
        public Coordinates SouthWest { get; set; }
        [XmlElement(ElementName = "SouthEast")]
        public Coordinates SouthEast { get; set; }
        [XmlElement(ElementName = "Center")]
        public Coordinates Center { get; set; }
    }

    [XmlRoot(ElementName = "TEC")]
    public class TEC
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "FaradayRotation")]
    public class FaradayRotation
    {
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "IonosphericParameters")]
    public class IonosphericParameters
    {
        [XmlElement(ElementName = "TEC")]
        public TEC TEC { get; set; }
        [XmlElement(ElementName = "FaradayRotation")]
        public FaradayRotation FaradayRotation { get; set; }
    }

    [XmlRoot(ElementName = "Channel")]
    public class Channel
    {
        [XmlElement(ElementName = "RasterInfo")]
        public RasterInfo RasterInfo { get; set; }
        [XmlElement(ElementName = "DataSetInfo")]
        public DataSetInfo DataSetInfo { get; set; }
        [XmlElement(ElementName = "SwathInfo")]
        public SwathInfo SwathInfo { get; set; }
        [XmlElement(ElementName = "SamplingConstants")]
        public SamplingConstants SamplingConstants { get; set; }
        [XmlElement(ElementName = "DataStatistics")]
        public DataStatistics DataStatistics { get; set; }
        [XmlElement(ElementName = "StateVectorData")]
        public StateVectorData StateVectorData { get; set; }
        [XmlElement(ElementName = "GroundCornerPoints")]
        public GroundCornerPoints GroundCornerPoints { get; set; }
        [XmlElement(ElementName = "IonosphericParameters")]
        public IonosphericParameters IonosphericParameters { get; set; }
        [XmlAttribute(AttributeName = "Number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "Total")]
        public string Total { get; set; }
    }

    [XmlRoot(ElementName = "SAOCOM_XMLProduct")]
    public class SAOCOM_XMLProduct
    {
        [XmlElement(ElementName = "NumberOfChannels")]
        public string NumberOfChannels { get; set; }
        [XmlElement(ElementName = "VersionNumber")]
        public string VersionNumber { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Channel")]
        public Channel[] Channel { get; set; }
        [XmlAttribute(AttributeName = "at", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string At { get; set; }
    }

    // =========================================================================
    // .xemt files (incomplete classes)
    // =========================================================================

    [XmlRoot(ElementName = "xemt", Namespace = "http://www.conae.gov.ar/CUSS/XEMT")]
    public class XEMT
    {
        [XmlElement(ElementName = "product", Namespace = "")]
        public Product Product { get; set; }
    }

    [XmlRoot(ElementName = "product")]
    public class Product
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "productType")]
        public ProductType ProductType { get; set; }
        [XmlElement(ElementName = "features")]
        public Features Features { get; set; }
    }

    [XmlRoot(ElementName = "productType")]
    public class ProductType
    {
        [XmlElement(ElementName = "main")]
        public string Main { get; set; }
        [XmlElement(ElementName = "sub")]
        public ProductTypeSub Sub { get; set; }
    }

    public class ProductTypeSub
    {
        [XmlElement(ElementName = "platform")]
        public string Platform { get; set; }
        [XmlElement(ElementName = "sensor")]
        public string Sensor { get; set; }
        [XmlElement(ElementName = "procLevel")]
        public string ProcLevel { get; set; }
    }

    [XmlRoot(ElementName = "features")]
    public class Features
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "abstract")]
        public string Abstract { get; set; }
        [XmlElement(ElementName = "scene")]
        public Production Production { get; set; }
        [XmlElement(ElementName = "acquisition")]
        public Acquisition Acquisition { get; set; }
        [XmlElement(ElementName = "geographicAttributes")]
        public GeographicAttributes GeographicAttributes { get; set; }

    }

    [XmlRoot(ElementName = "production")]
    public class Production
    {
        [XmlElement(ElementName = "countryID")]
        public string CountryId { get; set; }
        [XmlElement(ElementName = "agencyID")]
        public string AgencyId { get; set; }
        [XmlElement(ElementName = "facilityID")]
        public string FacilityId { get; set; }
        [XmlElement(ElementName = "serviceID")]
        public string ServiceId { get; set; }
        [XmlElement(ElementName = "productionTime")]
        public DateTime ProductionTime { get; set; }
    }

    [XmlRoot(ElementName = "acquisition")]
    public class Acquisition
    {
        [XmlElement(ElementName = "parameters")]
        public AcquisitionParameters Parameters { get; set; }
    }

    [XmlRoot(ElementName = "parameters")]
    public class AcquisitionParameters
    {
        [XmlElement(ElementName = "acqID")]
        public string AcquisitionId { get; set; }
        [XmlElement(ElementName = "referenceID")]
        public string ReferenceId { get; set; }
        [XmlElement(ElementName = "fc")]
        public string Fc { get; set; }
        [XmlElement(ElementName = "acqMode")]
        public string AcquisitionMode { get; set; }
        [XmlElement(ElementName = "polMode")]
        public string PolarizationMode { get; set; }
        [XmlElement(ElementName = "beamID")]
        public string BeamId { get; set; }
        [XmlElement(ElementName = "acquiredPols")]
        public string AcquiredPolarizations { get; set; }
        [XmlElement(ElementName = "sideLooking")]
        public string SideLooking { get; set; }
    }

    [XmlRoot(ElementName = "geographicAttributes")]
    public class GeographicAttributes
    {
        [XmlElement(ElementName = "pathRow")]
        public PathRow PathRow { get; set; }
    }

    [XmlRoot(ElementName = "pathRow")]
    public class PathRow
    {
        [XmlElement(ElementName = "Path")]
        public int Path { get; set; }
        [XmlElement(ElementName = "Row")]
        public int Row { get; set; }
    }

    // =========================================================================
    // parameters2.xml
    // =========================================================================

    [XmlRoot(ElementName = "parameters", Namespace = "http://www.conae.gov.ar/CGSS/XPNet")]
    public class ParameterFile
    {
        [XmlElement(ElementName = "inputs")]
        public Inputs Inputs { get; set; }
        [XmlElement(ElementName = "outputs")]
        public Outputs Outputs { get; set; }
    }

    [XmlRoot(ElementName = "inputs")]
    public class Inputs
    {
        [XmlElement(ElementName = "parameter")]
        public List<Parameter> Parameters { get; set; }
    }

    [XmlRoot(ElementName = "outputs")]
    public class Outputs
    {
        [XmlElement(ElementName = "parameter")]
        public List<Parameter> Parameters { get; set; }
    }

    [XmlRoot(ElementName = "parameter")]
    public class Parameter
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }


}