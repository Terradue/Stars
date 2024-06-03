using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace Terradue.Stars.Data.Model.Metadata.Kompsat3
{

    [XmlRoot(ElementName = "Projection")]
    public class Projection
    {

        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "Parameter")]
        public string Parameter { get; set; }

    }

    [XmlRoot(ElementName = "DynamicRange")]
    public class DynamicRange
    {

        [XmlElement(ElementName = "DesignMinimum")]
        public string DesignMinimum { get; set; }

        [XmlElement(ElementName = "DesignMaximum")]
        public string DesignMaximum { get; set; }

    }

    [XmlRoot(ElementName = "BrowseImageSize")]
    public class BrowseImageSize
    {

        [XmlElement(ElementName = "Width")]
        public string Width { get; set; }

        [XmlElement(ElementName = "Height")]
        public string Height { get; set; }

    }

    [XmlRoot(ElementName = "BrowseImage")]
    public class BrowseImage
    {

        [XmlElement(ElementName = "BrowseImageFileName")]
        public string BrowseImageFileName { get; set; }

        [XmlElement(ElementName = "BrowseImageSize")]
        public BrowseImageSize BrowseImageSize { get; set; }

    }

    [XmlRoot(ElementName = "ThumbnailImageSize")]
    public class ThumbnailImageSize
    {

        [XmlElement(ElementName = "Width")]
        public string Width { get; set; }

        [XmlElement(ElementName = "Height")]
        public string Height { get; set; }

    }

    [XmlRoot(ElementName = "ThumbnailImage")]
    public class ThumbnailImage
    {

        [XmlElement(ElementName = "ThumbnailImageFileName")]
        public string ThumbnailImageFileName { get; set; }

        [XmlElement(ElementName = "ThumbnailImageSize")]
        public ThumbnailImageSize ThumbnailImageSize { get; set; }

    }

    [XmlRoot(ElementName = "General")]
    public class General
    {

        [XmlElement(ElementName = "Satellite")]
        public string Satellite { get; set; }

        [XmlElement(ElementName = "Sensor")]
        public string Sensor { get; set; }

        [XmlElement(ElementName = "OrbitNumber")]
        public int OrbitNumber { get; set; }

        [XmlElement(ElementName = "OrbitDirection")]
        public string OrbitDirection { get; set; }

        [XmlElement(ElementName = "PassID")]
        public string PassID { get; set; }

        [XmlElement(ElementName = "ProductLevel")]
        public string ProductLevel { get; set; }

        [XmlElement(ElementName = "ImageFormat")]
        public string ImageFormat { get; set; }

        [XmlElement(ElementName = "ImagingMode")]
        public string ImagingMode { get; set; }

        [XmlElement(ElementName = "Projection")]
        public Projection Projection { get; set; }

        [XmlElement(ElementName = "EllipsoidType")]
        public string EllipsoidType { get; set; }

        [XmlElement(ElementName = "ResamplingMethod")]
        public string ResamplingMethod { get; set; }

        [XmlElement(ElementName = "DesignBitsPerPixel")]
        public string DesignBitsPerPixel { get; set; }

        [XmlElement(ElementName = "DynamicRange")]
        public DynamicRange DynamicRange { get; set; }

        [XmlElement(ElementName = "BrowseImage")]
        public BrowseImage BrowseImage { get; set; }

        [XmlElement(ElementName = "ThumbnailImage")]
        public ThumbnailImage ThumbnailImage { get; set; }

        [XmlElement(ElementName = "ApplyMTFC")]
        public string ApplyMTFC { get; set; }

        [XmlElement(ElementName = "ApplyPODPAD")]
        public string ApplyPODPAD { get; set; }

        [XmlElement(ElementName = "ApplyOODOPAD")]
        public string ApplyOODOPAD { get; set; }

        [XmlElement(ElementName = "ApplyPixelBurst")]
        public string ApplyPixelBurst { get; set; }

        [XmlElement(ElementName = "ApplyAttitudeBias")]
        public string ApplyAttitudeBias { get; set; }

        [XmlElement(ElementName = "ApplyRNUC")]
        public string ApplyRNUC { get; set; }

        [XmlElement(ElementName = "CreateDate")]
        public string CreateDate { get; set; }

        [XmlElement(ElementName = "AverageHeight")]
        public string AverageHeight { get; set; }

        [XmlElement(ElementName = "ProductID")]
        public string ProductID { get; set; }

        [XmlElement(ElementName = "PMSVersionNo")]
        public string PMSVersionNo { get; set; }

    }

    [XmlRoot(ElementName = "Position")]
    public class Position
    {

        [XmlElement(ElementName = "X")]
        public string X { get; set; }

        [XmlElement(ElementName = "Y")]
        public string Y { get; set; }

        [XmlElement(ElementName = "Z")]
        public string Z { get; set; }

    }

    [XmlRoot(ElementName = "Velocity")]
    public class Velocity
    {

        [XmlElement(ElementName = "VX")]
        public string VX { get; set; }

        [XmlElement(ElementName = "VY")]
        public string VY { get; set; }

        [XmlElement(ElementName = "VZ")]
        public string VZ { get; set; }

    }

    [XmlRoot(ElementName = "Attitude")]
    public class Attitude
    {

        [XmlElement(ElementName = "R")]
        public string R { get; set; }

        [XmlElement(ElementName = "P")]
        public string P { get; set; }

        [XmlElement(ElementName = "Y")]
        public string Y { get; set; }

    }

    [XmlRoot(ElementName = "SunAngle")]
    public class SunAngle
    {

        [XmlElement(ElementName = "Azimuth")]
        public double Azimuth { get; set; }

        [XmlElement(ElementName = "Elevation")]
        public double Elevation { get; set; }

    }

    [XmlRoot(ElementName = "MetadataBlock")]
    public class MetadataBlock
    {

        [XmlElement(ElementName = "Time")]
        public string Time { get; set; }

        [XmlElement(ElementName = "Position")]
        public Position Position { get; set; }

        [XmlElement(ElementName = "Velocity")]
        public Velocity Velocity { get; set; }

        [XmlElement(ElementName = "Attitude")]
        public Attitude Attitude { get; set; }

        [XmlElement(ElementName = "SunAngle")]
        public SunAngle SunAngle { get; set; }

    }

    [XmlRoot(ElementName = "Metadata")]
    public class Metadata
    {

        [XmlElement(ElementName = "MetadataBlock")]
        public List<MetadataBlock> MetadataBlock { get; set; }

    }

    [XmlRoot(ElementName = "ImagingStartTime")]
    public class ImagingStartTime
    {

        [XmlElement(ElementName = "UTC")]
        public string UTC { get; set; }

        [XmlElement(ElementName = "JulianDay")]
        public string JulianDay { get; set; }

        [XmlElement(ElementName = "JulianFraction")]
        public string JulianFraction { get; set; }

    }

    [XmlRoot(ElementName = "ImagingCenterTime")]
    public class ImagingCenterTime
    {

        [XmlElement(ElementName = "UTC")]
        public string UTC { get; set; }

        [XmlElement(ElementName = "JulianDay")]
        public string JulianDay { get; set; }

        [XmlElement(ElementName = "JulianFraction")]
        public string JulianFraction { get; set; }

    }

    [XmlRoot(ElementName = "ImagingEndTime")]
    public class ImagingEndTime
    {

        [XmlElement(ElementName = "UTC")]
        public string UTC { get; set; }

        [XmlElement(ElementName = "JulianDay")]
        public string JulianDay { get; set; }

        [XmlElement(ElementName = "JulianFraction")]
        public string JulianFraction { get; set; }

    }

    [XmlRoot(ElementName = "ImagingTime")]
    public class ImagingTime
    {

        [XmlElement(ElementName = "ImagingStartTime")]
        public ImagingStartTime ImagingStartTime { get; set; }

        [XmlElement(ElementName = "ImagingCenterTime")]
        public ImagingCenterTime ImagingCenterTime { get; set; }

        [XmlElement(ElementName = "ImagingEndTime")]
        public ImagingEndTime ImagingEndTime { get; set; }

        [XmlElement(ElementName = "ImagingDuration")]
        public string ImagingDuration { get; set; }

        [XmlElement(ElementName = "LineScanTime")]
        public string LineScanTime { get; set; }

    }

    [XmlRoot(ElementName = "ImageSize")]
    public class ImageSize
    {

        [XmlElement(ElementName = "Width")]
        public int Width { get; set; }

        [XmlElement(ElementName = "Height")]
        public int Height { get; set; }

    }

    [XmlRoot(ElementName = "ImageCoordCenter")]
    public class ImageCoordCenter
    {

        [XmlElement(ElementName = "Column")]
        public int Column { get; set; }

        [XmlElement(ElementName = "Row")]
        public int Row { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogCenter")]
    public class ImageGeogCenter
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogTL")]
    public class ImageGeogTL
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogTC")]
    public class ImageGeogTC
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogTR")]
    public class ImageGeogTR
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogBL")]
    public class ImageGeogBL
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogBC")]
    public class ImageGeogBC
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImageGeogBR")]
    public class ImageGeogBR
    {

        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

    }

    [XmlRoot(ElementName = "ImagingCoordinates")]
    public class ImagingCoordinates
    {

        [XmlElement(ElementName = "ImageCoordCenter")]
        public ImageCoordCenter ImageCoordCenter { get; set; }

        [XmlElement(ElementName = "ImageGeogCenter")]
        public ImageGeogCenter ImageGeogCenter { get; set; }

        [XmlElement(ElementName = "ImageGeogTL")]
        public ImageGeogTL ImageGeogTL { get; set; }

        [XmlElement(ElementName = "ImageGeogTC")]
        public ImageGeogTC ImageGeogTC { get; set; }

        [XmlElement(ElementName = "ImageGeogTR")]
        public ImageGeogTR ImageGeogTR { get; set; }

        [XmlElement(ElementName = "ImageGeogBL")]
        public ImageGeogBL ImageGeogBL { get; set; }

        [XmlElement(ElementName = "ImageGeogBC")]
        public ImageGeogBC ImageGeogBC { get; set; }

        [XmlElement(ElementName = "ImageGeogBR")]
        public ImageGeogBR ImageGeogBR { get; set; }

    }

    [XmlRoot(ElementName = "Angle")]
    public class Angle
    {

        [XmlElement(ElementName = "Roll")]
        public double Roll { get; set; }

        [XmlElement(ElementName = "Pitch")]
        public double Pitch { get; set; }

        [XmlElement(ElementName = "Yaw")]
        public double Yaw { get; set; }

        [XmlElement(ElementName = "Offnadir")]
        public double Offnadir { get; set; }

        [XmlElement(ElementName = "Incidence")]
        public double Incidence { get; set; }

        [XmlElement(ElementName = "Azimuth")]
        public double Azimuth { get; set; }

    }

    [XmlRoot(ElementName = "Section")]
    public class Section
    {

        [XmlElement(ElementName = "Cloud")]
        public string Cloud { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

    }

    [XmlRoot(ElementName = "CloudCover")]
    public class CloudCover
    {

        [XmlElement(ElementName = "Average")]
        public double Average { get; set; }

        [XmlElement(ElementName = "Section")]
        public List<Section> Section { get; set; }

    }

    [XmlRoot(ElementName = "DNRange")]
    public class DNRange
    {

        [XmlElement(ElementName = "MinimumDN")]
        public string MinimumDN { get; set; }

        [XmlElement(ElementName = "MaximumDN")]
        public string MaximumDN { get; set; }

    }

    [XmlRoot(ElementName = "CollectedGSD")]
    public class CollectedGSD
    {

        [XmlElement(ElementName = "Column")]
        public string Column { get; set; }

        [XmlElement(ElementName = "Row")]
        public string Row { get; set; }

    }

    [XmlRoot(ElementName = "ImageGSD")]
    public class ImageGSD
    {

        [XmlElement(ElementName = "Column")]
        public double Column { get; set; }

        [XmlElement(ElementName = "Row")]
        public double Row { get; set; }

    }

    [XmlRoot(ElementName = "SatellitePosition")]
    public class SatellitePosition
    {

        [XmlElement(ElementName = "Altitude")]
        public string Altitude { get; set; }

        [XmlElement(ElementName = "SSPLatitude")]
        public string SSPLatitude { get; set; }

        [XmlElement(ElementName = "SSPLongitude")]
        public string SSPLongitude { get; set; }

    }

    [XmlRoot(ElementName = "RadianceConversion")]
    public class RadianceConversion
    {

        [XmlElement(ElementName = "Gain", IsNullable = true)]
        public string GainString
        {
            get
            {
                return Gain.HasValue ? Gain.Value.ToString() : null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Gain = Convert.ToDouble(value, CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        [XmlIgnore]
        public double? Gain { get; set; }

        [XmlElement(ElementName = "Offset", IsNullable = true)]
        public string OffsetString
        {
            get
            {
                return Offset.HasValue ? Offset.Value.ToString() : null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Offset = Convert.ToDouble(value, CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        [XmlIgnore]
        public double? Offset { get; set; }

    }

    [XmlRoot("Image")]
    public class OneImage : IImage
    {

        [XmlElement(ElementName = "ImageFileName")]
        public string ImageFileName { get; set; }

        [XmlElement(ElementName = "ImageLevel")]
        public string ImageLevel { get; set; }

        [XmlElement(ElementName = "ImageColor")]
        public string ImageColor { get; set; }

        [XmlElement(ElementName = "ImagingTime")]
        public ImagingTime ImagingTime { get; set; }

        [XmlElement(ElementName = "ImageSize")]
        public ImageSize ImageSize { get; set; }

        [XmlElement(ElementName = "ImagingCoordinates")]
        public ImagingCoordinates ImagingCoordinates { get; set; }

        [XmlElement(ElementName = "Angle")]
        public Angle Angle { get; set; }

        [XmlElement(ElementName = "CloudCover")]
        public CloudCover CloudCover { get; set; }

        [XmlElement(ElementName = "DNRange")]
        public DNRange DNRange { get; set; }

        [XmlElement(ElementName = "CollectedGSD")]
        public CollectedGSD CollectedGSD { get; set; }

        [XmlElement(ElementName = "ImageGSD")]
        public ImageGSD ImageGSD { get; set; }

        [XmlElement(ElementName = "SatellitePosition")]
        public SatellitePosition SatellitePosition { get; set; }

        [XmlElement(ElementName = "ImageQuality")]
        public string ImageQuality { get; set; }

        [XmlElement(ElementName = "Bandwidth")]
        public string Bandwidth { get; set; }

        [XmlElement(ElementName = "RadianceConversion")]
        public RadianceConversion RadianceConversion { get; set; }

        [XmlElement(ElementName = "FocalLength")]
        public string FocalLength { get; set; }

        [XmlElement(ElementName = "CCDAlignment")]
        public string CCDAlignment { get; set; }

    }

    public interface IImage
    {
        string ImageFileName { get; set; }

        string ImageLevel { get; set; }

        string ImageColor { get; set; }

        ImagingTime ImagingTime { get; set; }

        ImageSize ImageSize { get; set; }

        ImagingCoordinates ImagingCoordinates { get; set; }

        Angle Angle { get; set; }

        CloudCover CloudCover { get; set; }

        DNRange DNRange { get; set; }

        CollectedGSD CollectedGSD { get; set; }

        ImageGSD ImageGSD { get; set; }

        SatellitePosition SatellitePosition { get; set; }

        string ImageQuality { get; set; }

        string Bandwidth { get; set; }

        RadianceConversion RadianceConversion { get; set; }

        string FocalLength { get; set; }

        string CCDAlignment { get; set; }


    }


    [XmlRoot(ElementName = "Auxiliary")]
    public class Auxiliary
    {

        [XmlElement(ElementName = "General")]
        public General General { get; set; }

        [XmlElement(ElementName = "Metadata")]
        public Metadata Metadata { get; set; }

        Dictionary<string, OneImage> images;

        [XmlIgnore]
        public Dictionary<string, OneImage> Images
        {
            get
            {
                // Ensure keys is never null.
                return (images = images ?? new Dictionary<string, OneImage>());
            }
            set
            {
                images = value;
            }
        }

        [XmlElement("Image")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public XmlKeyTextValueListWrapper<OneImage> XmlKeys
        {
            get
            {
                return new XmlKeyTextValueListWrapper<OneImage>(() => Images);
            }
            set
            {
                value.CopyTo(Images);
            }
        }

        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

    }

}
