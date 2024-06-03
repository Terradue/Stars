/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System.Xml.Serialization;
namespace Terradue.Stars.Data.Model.Metadata.Gaofen
{
    [XmlRoot(ElementName = "ProductMetaData")]
    public class ProductMetaData
    {
        [XmlElement(ElementName = "SatelliteID")]
        public string SatelliteID { get; set; }
        [XmlElement(ElementName = "SensorID")]
        public string SensorID { get; set; }
        [XmlElement(ElementName = "ReceiveTime")]
        public string ReceiveTime { get; set; }
        [XmlElement(ElementName = "OrbitID")]
        public string OrbitID { get; set; }
        [XmlElement(ElementName = "ProduceType")]
        public string ProduceType { get; set; }
        [XmlElement(ElementName = "SceneID")]
        public string SceneID { get; set; }
        [XmlElement(ElementName = "ProductID")]
        public string ProductID { get; set; }
        [XmlElement(ElementName = "ProductLevel")]
        public string ProductLevel { get; set; }
        [XmlElement(ElementName = "ProductQuality")]
        public string ProductQuality { get; set; }
        [XmlElement(ElementName = "ProductQualityReport")]
        public string ProductQualityReport { get; set; }
        [XmlElement(ElementName = "ProductFormat")]
        public string ProductFormat { get; set; }
        [XmlElement(ElementName = "ProduceTime")]
        public string ProduceTime { get; set; }
        [XmlElement(ElementName = "Bands")]
        public string Bands { get; set; }
        [XmlElement(ElementName = "ScenePath")]
        public string ScenePath { get; set; }
        [XmlElement(ElementName = "SceneRow")]
        public string SceneRow { get; set; }
        [XmlElement(ElementName = "SatPath")]
        public string SatPath { get; set; }
        [XmlElement(ElementName = "SatRow")]
        public string SatRow { get; set; }
        [XmlElement(ElementName = "SceneCount")]
        public string SceneCount { get; set; }
        [XmlElement(ElementName = "SceneShift")]
        public string SceneShift { get; set; }
        [XmlElement(ElementName = "StartTime")]
        public string StartTime { get; set; }
        [XmlElement(ElementName = "EndTime")]
        public string EndTime { get; set; }
        [XmlElement(ElementName = "CenterTime")]
        public string CenterTime { get; set; }
        [XmlElement(ElementName = "ImageGSD")]
        public string ImageGSD { get; set; }
        [XmlElement(ElementName = "WidthInPixels")]
        public int WidthInPixels { get; set; }
        [XmlElement(ElementName = "HeightInPixels")]
        public int HeightInPixels { get; set; }
        [XmlElement(ElementName = "WidthInMeters")]
        public string WidthInMeters { get; set; }
        [XmlElement(ElementName = "HeightInMeters")]
        public string HeightInMeters { get; set; }
        [XmlElement(ElementName = "CloudPercent")]
        public string CloudPercent { get; set; }
        [XmlElement(ElementName = "QualityInfo")]
        public string QualityInfo { get; set; }
        [XmlElement(ElementName = "PixelBits")]
        public string PixelBits { get; set; }
        [XmlElement(ElementName = "ValidPixelBits")]
        public string ValidPixelBits { get; set; }
        [XmlElement(ElementName = "RollViewingAngle")]
        public string RollViewingAngle { get; set; }
        [XmlElement(ElementName = "PitchViewingAngle")]
        public string PitchViewingAngle { get; set; }
        [XmlElement(ElementName = "RollSatelliteAngle")]
        public string RollSatelliteAngle { get; set; }
        [XmlElement(ElementName = "PitchSatelliteAngle")]
        public string PitchSatelliteAngle { get; set; }
        [XmlElement(ElementName = "YawSatelliteAngle")]
        public string YawSatelliteAngle { get; set; }
        [XmlElement(ElementName = "SolarAzimuth")]
        public string SolarAzimuth { get; set; }
        [XmlElement(ElementName = "SolarZenith")]
        public string SolarZenith { get; set; }
        [XmlElement(ElementName = "SatelliteAzimuth")]
        public string SatelliteAzimuth { get; set; }
        [XmlElement(ElementName = "SatelliteZenith")]
        public string SatelliteZenith { get; set; }
        [XmlElement(ElementName = "Gain")]
        public string Gain { get; set; }
        [XmlElement(ElementName = "Bias")]
        public string Bias { get; set; }
        [XmlElement(ElementName = "GainMode")]
        public string GainMode { get; set; }
        [XmlElement(ElementName = "IntegrationTime")]
        public string IntegrationTime { get; set; }
        [XmlElement(ElementName = "IntegrationLevel")]
        public string IntegrationLevel { get; set; }
        [XmlElement(ElementName = "MapProjection")]
        public string MapProjection { get; set; }
        [XmlElement(ElementName = "EarthEllipsoid")]
        public string EarthEllipsoid { get; set; }
        [XmlElement(ElementName = "ZoneNo")]
        public string ZoneNo { get; set; }
        [XmlElement(ElementName = "ResamplingKernel")]
        public string ResamplingKernel { get; set; }
        [XmlElement(ElementName = "HeightMode")]
        public string HeightMode { get; set; }
        [XmlElement(ElementName = "MtfCorrection")]
        public string MtfCorrection { get; set; }
        [XmlElement(ElementName = "RelativeCorrectionData")]
        public string RelativeCorrectionData { get; set; }
        [XmlElement(ElementName = "TopLeftLatitude")]
        public string TopLeftLatitude { get; set; }
        [XmlElement(ElementName = "TopLeftLongitude")]
        public string TopLeftLongitude { get; set; }
        [XmlElement(ElementName = "TopRightLatitude")]
        public string TopRightLatitude { get; set; }
        [XmlElement(ElementName = "TopRightLongitude")]
        public string TopRightLongitude { get; set; }
        [XmlElement(ElementName = "BottomRightLatitude")]
        public string BottomRightLatitude { get; set; }
        [XmlElement(ElementName = "BottomRightLongitude")]
        public string BottomRightLongitude { get; set; }
        [XmlElement(ElementName = "BottomLeftLatitude")]
        public string BottomLeftLatitude { get; set; }
        [XmlElement(ElementName = "BottomLeftLongitude")]
        public string BottomLeftLongitude { get; set; }
        [XmlElement(ElementName = "TopLeftMapX")]
        public string TopLeftMapX { get; set; }
        [XmlElement(ElementName = "TopLeftMapY")]
        public string TopLeftMapY { get; set; }
        [XmlElement(ElementName = "TopRightMapX")]
        public string TopRightMapX { get; set; }
        [XmlElement(ElementName = "TopRightMapY")]
        public string TopRightMapY { get; set; }
        [XmlElement(ElementName = "BottomRightMapX")]
        public string BottomRightMapX { get; set; }
        [XmlElement(ElementName = "BottomRightMapY")]
        public string BottomRightMapY { get; set; }
        [XmlElement(ElementName = "BottomLeftMapX")]
        public string BottomLeftMapX { get; set; }
        [XmlElement(ElementName = "BottomLeftMapY")]
        public string BottomLeftMapY { get; set; }
    }

}
