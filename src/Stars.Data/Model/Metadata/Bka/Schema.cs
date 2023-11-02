using System.Xml.Serialization;

namespace Terradue.Stars.Data.Model.Metadata.Bka
{

    [XmlRoot(ElementName = "Metadata")]
    public class BkaMetadata
    {

        [XmlElement(ElementName = "Version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "Production")]
        public BkaProduction Production { get; set; }

        [XmlElement(ElementName = "Processing")]
        public BkaProcessing Processing { get; set; }

        [XmlElement(ElementName = "Satellite_Data")]
        public BkaSatelliteData SatelliteData  { get; set; }

        [XmlElement(ElementName = "Radiometry")]
        public BkaRadiometry Radiometry  { get; set; }

        [XmlElement(ElementName = "Geo_Reference")]
        public BkaGeoReference GeoReference  { get; set; }

        [XmlElement(ElementName = "Image_Info")]
        public BkaImageInfo ImageInfo  { get; set; }
    }

    [XmlRoot(ElementName = "Production")]
    public class BkaProduction
    {
        [XmlElement(ElementName = "Copyright")]
        public string Copyright { get; set; }

        [XmlElement(ElementName = "Producer")]
        public string Producer { get; set; }

        [XmlElement(ElementName = "PackAppId")]
        public string PackAppId { get; set; }

        [XmlElement(ElementName = "JobId")]
        public string JobId { get; set; }

        [XmlElement(ElementName = "Creation_Date")]
        public string CreationDate { get; set; }
    }


    [XmlRoot(ElementName = "Processing")]
    public class BkaProcessing
    {
        [XmlElement(ElementName = "Mission")]
        public string Mission { get; set; }

        [XmlElement(ElementName = "Satellite")]
        public string Satellite { get; set; }

        [XmlElement(ElementName = "Instrument")]
        public string Instrument { get; set; }

        [XmlElement(ElementName = "Sensor_Code")]
        public string SensorCode { get; set; }

        [XmlElement(ElementName = "Band")]
        public string Band { get; set; }

        [XmlElement(ElementName = "Level")]
        public string Level { get; set; }

        [XmlElement(ElementName = "Level_Info")]
        public string LevelInfo { get; set; }

        [XmlElement(ElementName = "Level_Data")]
        public string LevelData { get; set; }

        [XmlElement(ElementName = "Radiometric")]
        public string Radiometric { get; set; }

        [XmlElement(ElementName = "Geometric")]
        public string Geometric { get; set; }

        [XmlElement(ElementName = "Percent_Cloud_Cover")]
        public double? PercentCloudCover { get; set; }

        [XmlElement(ElementName = "Country_Cover_Cod")]
        public string[] CountryCoverCod { get; set; }

        [XmlElement(ElementName = "Country_Cover")]
        public string[] CountryCover { get; set; }

        [XmlElement(ElementName = "Scenes_Count")]
        public int? ScenesCount { get; set; }

        [XmlElement(ElementName = "Scene01_File_Name")]
        public string Scene01FileName { get; set; }

        [XmlElement(ElementName = "Scene02_File_Name")]
        public string Scene02FileName { get; set; }

        [XmlElement(ElementName = "Scene03_File_Name")]
        public string Scene03FileName { get; set; }

        [XmlElement(ElementName = "Scene04_File_Name")]
        public string Scene04FileName { get; set; }

        [XmlElement(ElementName = "Legend")]
        public string Legend { get; set; }
    }


    [XmlRoot(ElementName = "Satellite_Data")]
    public class BkaSatelliteData
    {
        [XmlElement(ElementName = "Scene_Acquisition")]
        public BkaSceneAcquisition SceneAcquisition { get; set; }

        [XmlElement(ElementName = "Routes_Count")]
        public int? RoutesCount { get; set; }
    }



    [XmlRoot(ElementName = "Radiometry")]
    public class BkaRadiometry
    {
        [XmlElement(ElementName = "Correction")]
        public string Correction { get; set; }

        [XmlElement(ElementName = "Spectral")]
        public BkaSpectral Spectral { get; set; }
    }



    [XmlRoot(ElementName = "Geo_Reference")]
    public class BkaGeoReference
    {
        [XmlElement(ElementName = "Datum")]
        public string Datum { get; set; }

        [XmlElement(ElementName = "Ellipsoid")]
        public string Ellipsoid { get; set; }

        [XmlElement(ElementName = "Map_Projection")]
        public string MapProjection { get; set; }

        [XmlElement(ElementName = "Hemisphere")]
        public string Hemisphere { get; set; }

        [XmlElement(ElementName = "EPSG_code")]
        public int? EpsgCode { get; set; }

        [XmlElement(ElementName = "EPSG_tables")]
        public string EPSGTables { get; set; }

        [XmlElement(ElementName = "Elevation_Data")]
        public BkaElevationData ElevationData { get; set; }

        [XmlElement(ElementName = "Scene_Geoposition")]
        public BkaSceneGeoposition SceneGeoposition { get; set; }
    }



    [XmlRoot(ElementName = "Image_Info")]
    public class BkaImageInfo
    {
        /*
            <Image_Info>
                <Resample>
                    <Method>Bilinear Interpolation</Method>
                </Resample>
            </Image_Info>
        */


        [XmlElement(ElementName = "File_Format")]
        public string FileFormat { get; set; }

        [XmlElement(ElementName = "Compression")]
        public string Compression { get; set; }

        [XmlElement(ElementName = "Band_Composite")]
        public string BandComposite { get; set; }

        [XmlElement(ElementName = "Samples")]
        public int? Samples { get; set; }

        [XmlElement(ElementName = "Width")]
        public int? Width { get; set; }

        [XmlElement(ElementName = "Height")]
        public int? Height { get; set; }

        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "Bits_per_Pixel")]
        public int? BitsPerPixel { get; set; }

        [XmlElement(ElementName = "Data_NBits")]
        public int? DataNBits { get; set; }

        [XmlElement(ElementName = "Data_Type")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "Byte_Order")]
        public string ByteOrder { get; set; }

        [XmlElement(ElementName = "Resample")]
        public BkaImageResample Resample { get; set; }
    }



    [XmlRoot(ElementName = "Scene_Acquisition")]
    public class BkaSceneAcquisition
    {
        /*
            <Scene_Acquisition>
                <Satellite>BKA</Satellite>
                <Acquisition_Time_GMT>2015-09-24T04:43:01</Acquisition_Time_GMT>
                <Sun_Elevation>84.08</Sun_Elevation>
                <Sun_Azimuth>6.84</Sun_Azimuth>
                <Viewing_Angle>2.31</Viewing_Angle>
                <Center>
                    <Center_Lat>-6.21091851</Center_Lat>
                    <Center_Lon>106.60503472</Center_Lon>
                </Center>
            </Scene_Acquisition>
        */


        [XmlElement(ElementName = "Satellite")]
        public string SceneAcquisition { get; set; }

        [XmlElement(ElementName = "Acquisition_Time_GMT")]
        public string RoutesCount { get; set; }

        [XmlElement(ElementName = "Sun_Elevation")]
        public double? SunElevation { get; set; }

        [XmlElement(ElementName = "Sun_Azimuth")]
        public double? SunAzimuth { get; set; }

        [XmlElement(ElementName = "Viewing_Angle")]
        public double? ViewingAngle { get; set; }

        [XmlElement(ElementName = "Center")]
        public BkaCenterCoordinates Center { get; set; }
    }


    [XmlRoot(ElementName = "Center")]
    public class BkaCenterCoordinates
    {
        [XmlElement(ElementName = "Center_Lat")]
        public double? CenterLat { get; set; }

        [XmlElement(ElementName = "Center_Lon")]
        public double? CenterLon { get; set; }
    }


    [XmlRoot(ElementName = "Spectral")]
    public class BkaSpectral
    {
        [XmlElement(ElementName = "Band_PAN")]
        public BkaBandInfo BandPAN { get; set; }

        [XmlElement(ElementName = "Band_MS1")]
        public BkaBandInfo BandMS1 { get; set; }

        [XmlElement(ElementName = "Band_MS2")]
        public BkaBandInfo BandMS2 { get; set; }

        [XmlElement(ElementName = "Band_MS3")]
        public BkaBandInfo BandMS3 { get; set; }

        [XmlElement(ElementName = "Band_MS4")]
        public BkaBandInfo BandMS4 { get; set; }
    }



    [XmlRoot()]
    public class BkaBandInfo
    {
        [XmlElement(ElementName = "band_index")]
        public int BandIndex { get; set; }

        [XmlElement(ElementName = "band_code")]
        public string BandCode { get; set; }

        [XmlElement(ElementName = "band_info")]
        public string BandInfo { get; set; }
    }



    [XmlRoot(ElementName = "Elevation_Data")]
    public class BkaElevationData
    {
        [XmlElement(ElementName = "Elevation_Source")]
        public string ElevationSource { get; set; }

        [XmlElement(ElementName = "SRTM_Version")]
        public string SrtmVersion { get; set; }
    }



    [XmlRoot(ElementName = "Scene_Geoposition")]
    public class BkaSceneGeoposition
    {
        [XmlElement(ElementName = "Geodetic_Coordinates")]
        public BkaGeodeticCoordinates GeodeticCoordinates { get; set; }

        [XmlElement(ElementName = "Map_Coordinates")]
        public BkaMapCoordinates MapCoordinates { get; set; }

        [XmlElement(ElementName = "Bounds")]
        public BkaSceneBounds Bounds { get; set; }

        [XmlElement(ElementName = "Ground_Resolution")]
        public BkaSceneGroundResolution GroundResolution { get; set; }
    }


    [XmlRoot(ElementName = "Geodetic_Coordinates")]
    public class BkaGeodeticCoordinates
    {
        [XmlElement(ElementName = "Corner1_SW_Lat")]
        public double Corner1SWLat { get; set; }

        [XmlElement(ElementName = "Corner1_SW_Lon")]
        public double Corner1SWLon { get; set; }

        [XmlElement(ElementName = "Corner2_NW_Lat")]
        public double Corner1NWLat { get; set; }

        [XmlElement(ElementName = "Corner2_NW_Lon")]
        public double Corner1NWLon { get; set; }

        [XmlElement(ElementName = "Corner3_NE_Lat")]
        public double Corner1NELat { get; set; }

        [XmlElement(ElementName = "Corner3_NE_Lon")]
        public double Corner1NELon { get; set; }

        [XmlElement(ElementName = "Corner4_SE_Lat")]
        public double Corner1SELat { get; set; }

        [XmlElement(ElementName = "Corner4_SE_Lon")]
        public double Corner1SELon { get; set; }
    }


    [XmlRoot(ElementName = "Map_Coordinates")]
    public class BkaMapCoordinates
    {
        [XmlElement(ElementName = "Corner1_SW_Easting")]
        public double Corner1SWEasting { get; set; }

        [XmlElement(ElementName = "Corner1_SW_Northing")]
        public double Corner1SWNorthing { get; set; }

        [XmlElement(ElementName = "Corner2_NW_Easting")]
        public double Corner1NWEasting { get; set; }

        [XmlElement(ElementName = "Corner2_NW_Northing")]
        public double Corner1NWNorthing { get; set; }

        [XmlElement(ElementName = "Corner3_NE_Easting")]
        public double Corner1NEEasting { get; set; }

        [XmlElement(ElementName = "Corner3_NE_Northing")]
        public double Corner1NENorthing { get; set; }

        [XmlElement(ElementName = "Corner4_SE_Easting")]
        public double Corner1SEEasting { get; set; }

        [XmlElement(ElementName = "Corner4_SE_Northing")]
        public double Corner1SENorthing { get; set; }
    }


    [XmlRoot(ElementName = "Bounds")]
    public class BkaSceneBounds
    {
        [XmlElement(ElementName = "File_Name")]
        public string FileName { get; set; }
    }


    [XmlRoot(ElementName = "Ground_Resolution")]
    public class BkaSceneGroundResolution
    {
        [XmlElement(ElementName = "PixelSize_Easting")]
        public double PixelSizeEasting { get; set; }

        [XmlElement(ElementName = "PixelSize_Northing")]
        public double PixelSizeNorthing { get; set; }

        [XmlElement(ElementName = "PixelSize_units")]
        public string PixelSizeUnits { get; set; }
    }


    [XmlRoot(ElementName = "Resample")]
    public class BkaImageResample
    {
        [XmlElement(ElementName = "Method")]
        public string Method { get; set; }
   }

}