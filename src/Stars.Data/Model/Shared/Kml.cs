using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace Terradue.Stars.Data.Model.Shared
{

    [XmlRoot(ElementName="LineStyle", Namespace="")]
    public class LineStyle { 

        [XmlElement(ElementName="color", Namespace="")] 
        public string Color { get; set; } 
    }

    [XmlRoot(ElementName="PolyStyle", Namespace="")]
    public class PolyStyle { 

        [XmlElement(ElementName="fill", Namespace="")] 
        public int Fill { get; set; } 
    }

    [XmlRoot(ElementName="Style", Namespace="")]
    public class Style { 

        [XmlElement(ElementName="LineStyle", Namespace="")] 
        public LineStyle LineStyle { get; set; } 

        [XmlElement(ElementName="PolyStyle", Namespace="")] 
        public PolyStyle PolyStyle { get; set; } 

        [XmlAttribute(AttributeName="id", Namespace="")] 
        public string Id { get; set; } 

        [XmlText] 
        public string Text { get; set; } 

        [XmlElement(ElementName="IconStyle", Namespace="")] 
        public IconStyle IconStyle { get; set; } 

        [XmlElement(ElementName="BalloonStyle", Namespace="")] 
        public BalloonStyle BalloonStyle { get; set; } 
    }

    [XmlRoot(ElementName="Icon", Namespace="")]
    public class Icon { 

        [XmlElement(ElementName="href", Namespace="")] 
        public string Href { get; set; } 
    }

    [XmlRoot(ElementName="IconStyle", Namespace="")]
    public class IconStyle { 

        [XmlElement(ElementName="scale", Namespace="")] 
        public double Scale { get; set; } 

        [XmlElement(ElementName="Icon", Namespace="")] 
        public Icon Icon { get; set; } 
    }

    [XmlRoot(ElementName="BalloonStyle", Namespace="")]
    public class BalloonStyle { 

        [XmlElement(ElementName="text", Namespace="")] 
        public string Text { get; set; } 
    }

    [XmlRoot(ElementName="Pair", Namespace="")]
    public class Pair { 

        [XmlElement(ElementName="key", Namespace="")] 
        public string Key { get; set; } 

        [XmlElement(ElementName="styleUrl", Namespace="")] 
        public string StyleUrl { get; set; } 
    }

    [XmlRoot(ElementName="StyleMap", Namespace="")]
    public class StyleMap { 

        [XmlElement(ElementName="Pair", Namespace="")] 
        public List<Pair> Pair { get; set; } 

        [XmlAttribute(AttributeName="id", Namespace="")] 
        public string Id { get; set; } 

        [XmlText] 
        public string Text { get; set; } 
    }

    [XmlRoot(ElementName="LookAt", Namespace="")]
    public class LookAt { 

        [XmlElement(ElementName="longitude", Namespace="")] 
        public double Longitude { get; set; } 

        [XmlElement(ElementName="latitude", Namespace="")] 
        public double Latitude { get; set; } 

        [XmlElement(ElementName="range", Namespace="")] 
        public int Range { get; set; } 
    }

    [XmlRoot(ElementName="Point", Namespace="")]
    public class Point { 

        [XmlElement(ElementName="coordinates", Namespace="")] 
        public string Coordinates { get; set; } 
    }

    [XmlRoot(ElementName="Placemark", Namespace="")]
    public class Placemark { 

        [XmlElement(ElementName="name", Namespace="")] 
        public string Name { get; set; } 

        [XmlElement(ElementName="description", Namespace="")] 
        public string Description { get; set; } 

        [XmlElement(ElementName="LookAt", Namespace="")] 
        public LookAt LookAt { get; set; } 

        [XmlElement(ElementName="Point", Namespace="")] 
        public Point Point { get; set; } 

        [XmlElement(ElementName="styleUrl", Namespace="")] 
        public string StyleUrl { get; set; } 

        [XmlElement(ElementName="MultiGeometry", Namespace="")] 
        public MultiGeometry MultiGeometry { get; set; } 
    }

    [XmlRoot(ElementName="LinearRing", Namespace="")]
    public class LinearRing { 

        [XmlElement(ElementName="coordinates", Namespace="")] 
        public string Coordinates { get; set; } 
    }

    [XmlRoot(ElementName="outerBoundaryIs", Namespace="")]
    public class OuterBoundaryIs { 

        [XmlElement(ElementName="LinearRing", Namespace="")] 
        public LinearRing LinearRing { get; set; } 
    }

    [XmlRoot(ElementName="Polygon", Namespace="")]
    public class Polygon { 

        [XmlElement(ElementName="outerBoundaryIs", Namespace="")] 
        public OuterBoundaryIs OuterBoundaryIs { get; set; } 
    }

    [XmlRoot(ElementName="MultiGeometry", Namespace="")]
    public class MultiGeometry { 

        [XmlElement(ElementName="Polygon", Namespace="")] 
        public Polygon Polygon { get; set; } 
    }

    [XmlRoot(ElementName="LatLonQuad", Namespace="http://www.google.com/kml/ext/2.2")]
    public class LatLonQuad { 

        [XmlElement(ElementName="coordinates", Namespace="")] 
        public string Coordinates { get; set; } 
    }

    [XmlRoot(ElementName="GroundOverlay", Namespace="")]
    public class GroundOverlay { 

        [XmlElement(ElementName="name", Namespace="")] 
        public string Name { get; set; } 

        [XmlElement(ElementName="Icon", Namespace="")] 
        public Icon Icon { get; set; } 

        [XmlElement(ElementName="altitudeMode", Namespace="")] 
        public string AltitudeMode { get; set; } 

        [XmlElement(ElementName="LatLonQuad", Namespace="http://www.google.com/kml/ext/2.2")] 
        public LatLonQuad LatLonQuad { get; set; } 
    }

    [XmlRoot(ElementName="Folder", Namespace="")]
    public class Folder { 

        [XmlElement(ElementName="name", Namespace="")] 
        public string Name { get; set; } 

        [XmlElement(ElementName="Placemark", Namespace="")] 
        public List<Placemark> Placemark { get; set; } 

        [XmlElement(ElementName="GroundOverlay", Namespace="")] 
        public GroundOverlay GroundOverlay { get; set; } 
    }

    [XmlRoot(ElementName="Document", Namespace="")]
    public class Document { 

        [XmlElement(ElementName="Style", Namespace="")] 
        public List<Style> Style { get; set; } 

        [XmlElement(ElementName="StyleMap", Namespace="")] 
        public StyleMap StyleMap { get; set; } 

        [XmlElement(ElementName="Folder", Namespace="")] 
        public Folder Folder { get; set; } 
    }

    [XmlRoot(ElementName="kml", Namespace="")]
    public class Kml { 

        [XmlElement(ElementName="Document", Namespace="")] 
        public Document Document { get; set; } 

        [XmlAttribute(AttributeName="gx", Namespace="http://www.w3.org/2000/xmlns/")] 
        public string Gx { get; set; } 

        [XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")] 
        public string Xsi { get; set; } 

        [XmlAttribute(AttributeName="noNamespaceSchemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")] 
        public string NoNamespaceSchemaLocation { get; set; } 

        [XmlText] 
        public string Text { get; set; }

        [XmlElement(ElementName="GroundOverlay", Namespace="")] 
        public GroundOverlay GroundOverlay { get; set; } 

    }

}