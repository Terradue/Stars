// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Metadata.cs

namespace Terradue.Stars.Data.Model.Metadata.Vrss.Schemas
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName = "productMeta", IsNullable = false)]
    public partial class Metadata   // productMeta
    {

        private string organizationNameField;

        private string rawdataFileNameField;

        private string l0DataFileNameField;

        private string imageNameField;

        private string browseNameField;

        private string thumbNameField;

        private string productDateField;

        private string productIdField;

        private string satelliteIdField;

        private string sensorIdField;

        private string recStationIdField;

        private string sceneIdField;

        private string orbitIdField;

        private string rawdataIDField;

        private string l0DataIDField;

        private string sceneSerialNoField;

        private string dataUpperLeftLatField;

        private string dataUpperLeftLongField;

        private string dataUpperRightLatField;

        private string dataUpperRightLongField;

        private string dataLowerLeftLatField;

        private string dataLowerLeftLongField;

        private string dataLowerRightLatField;

        private string dataLowerRightLongField;

        private string productUpperLeftLatField;

        private string productUpperLeftLongField;

        private string productUpperRightLatField;

        private string productUpperRightLongField;

        private string productLowerLeftLatField;

        private string productLowerLeftLongField;

        private string productLowerRightLatField;

        private string productLowerRightLongField;

        private string dataUpperLeftXField;

        private string dataUpperLeftYField;

        private string dataUpperRightXField;

        private string dataUpperRightYField;

        private string dataLowerLeftXField;

        private string dataLowerLeftYField;

        private string dataLowerRightXField;

        private string dataLowerRightYField;

        private string productUpperLeftXField;

        private string productUpperLeftYField;

        private string productUpperRightXField;

        private string productUpperRightYField;

        private string productLowerLeftXField;

        private string productLowerLeftYField;

        private string productLowerRightXField;

        private string productLowerRightYField;

        private string dataSizeField;

        private string sceneCenterLatField;

        private string sceneCenterLongField;

        private string sceneStartLineField;

        private string sceneStopLineField;

        private string scene_imagingStartTimeField;

        private string scene_imagingStopTimeField;

        private string scenePathField;

        private string sceneRowField;

        private string scenePathBiasField;

        private string sceneRowBiasField;

        private string sceneCountField;

        private string sceneShiftField;

        private string directionField;

        private string lostFrameNumField;

        private string lineTimeDataStatusField;

        private string mtfcProField;

        private string resampleTechniqueField;

        private string productLevelField;

        private string deadPixelField;

        private string earthModelField;

        private string mapProjectionField;

        private string zone_NumberField;

        private string productTypeField;

        private string bandsField;

        private string gainField;

        private string intergralLevelField;

        private string instrumentModeField;

        private string sensorGSDField;

        private string mirrorOffNadirField;

        private string satellite_AltitudeField;

        private string satZenithAngleField;

        private string satAzimuthAngleField;

        private string satOffNadirField;

        private string satPathField;

        private string satRowField;

        private string ephemerisDataField;

        private string attitudeDataField;

        private string attDataStatusField;

        private string gpsDataStatusField;

        private string dataFormatDesField;

        private string linkageField;

        private productMetaQuality[] qualityField;

        private productMetaSunElevation[] sunElevationField;

        private productMetaSunAzimuth[] sunAzimuthField;

        private productMetaCloudCoverage[] cloudCoverageField;

        private productMetaSolarIrradiance[] solarIrradianceField;

        private productMetaAb_calibra_param[] ab_calibra_paramField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string OrganizationName
        {
            get
            {
                return this.organizationNameField;
            }
            set
            {
                this.organizationNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RawdataFileName
        {
            get
            {
                return this.rawdataFileNameField;
            }
            set
            {
                this.rawdataFileNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string L0DataFileName
        {
            get
            {
                return this.l0DataFileNameField;
            }
            set
            {
                this.l0DataFileNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string imageName
        {
            get
            {
                return this.imageNameField;
            }
            set
            {
                this.imageNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string browseName
        {
            get
            {
                return this.browseNameField;
            }
            set
            {
                this.browseNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string thumbName
        {
            get
            {
                return this.thumbNameField;
            }
            set
            {
                this.thumbNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productDate
        {
            get
            {
                return this.productDateField;
            }
            set
            {
                this.productDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productId
        {
            get
            {
                return this.productIdField;
            }
            set
            {
                this.productIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satelliteId
        {
            get
            {
                return this.satelliteIdField;
            }
            set
            {
                this.satelliteIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sensorId
        {
            get
            {
                return this.sensorIdField;
            }
            set
            {
                this.sensorIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string recStationId
        {
            get
            {
                return this.recStationIdField;
            }
            set
            {
                this.recStationIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneId
        {
            get
            {
                return this.sceneIdField;
            }
            set
            {
                this.sceneIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string orbitId
        {
            get
            {
                return this.orbitIdField;
            }
            set
            {
                this.orbitIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RawdataID
        {
            get
            {
                return this.rawdataIDField;
            }
            set
            {
                this.rawdataIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string L0DataID
        {
            get
            {
                return this.l0DataIDField;
            }
            set
            {
                this.l0DataIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneSerialNo
        {
            get
            {
                return this.sceneSerialNoField;
            }
            set
            {
                this.sceneSerialNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperLeftLat
        {
            get
            {
                return this.dataUpperLeftLatField;
            }
            set
            {
                this.dataUpperLeftLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperLeftLong
        {
            get
            {
                return this.dataUpperLeftLongField;
            }
            set
            {
                this.dataUpperLeftLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperRightLat
        {
            get
            {
                return this.dataUpperRightLatField;
            }
            set
            {
                this.dataUpperRightLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperRightLong
        {
            get
            {
                return this.dataUpperRightLongField;
            }
            set
            {
                this.dataUpperRightLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerLeftLat
        {
            get
            {
                return this.dataLowerLeftLatField;
            }
            set
            {
                this.dataLowerLeftLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerLeftLong
        {
            get
            {
                return this.dataLowerLeftLongField;
            }
            set
            {
                this.dataLowerLeftLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerRightLat
        {
            get
            {
                return this.dataLowerRightLatField;
            }
            set
            {
                this.dataLowerRightLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerRightLong
        {
            get
            {
                return this.dataLowerRightLongField;
            }
            set
            {
                this.dataLowerRightLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperLeftLat
        {
            get
            {
                return this.productUpperLeftLatField;
            }
            set
            {
                this.productUpperLeftLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperLeftLong
        {
            get
            {
                return this.productUpperLeftLongField;
            }
            set
            {
                this.productUpperLeftLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperRightLat
        {
            get
            {
                return this.productUpperRightLatField;
            }
            set
            {
                this.productUpperRightLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperRightLong
        {
            get
            {
                return this.productUpperRightLongField;
            }
            set
            {
                this.productUpperRightLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerLeftLat
        {
            get
            {
                return this.productLowerLeftLatField;
            }
            set
            {
                this.productLowerLeftLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerLeftLong
        {
            get
            {
                return this.productLowerLeftLongField;
            }
            set
            {
                this.productLowerLeftLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerRightLat
        {
            get
            {
                return this.productLowerRightLatField;
            }
            set
            {
                this.productLowerRightLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerRightLong
        {
            get
            {
                return this.productLowerRightLongField;
            }
            set
            {
                this.productLowerRightLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperLeftX
        {
            get
            {
                return this.dataUpperLeftXField;
            }
            set
            {
                this.dataUpperLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperLeftY
        {
            get
            {
                return this.dataUpperLeftYField;
            }
            set
            {
                this.dataUpperLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperRightX
        {
            get
            {
                return this.dataUpperRightXField;
            }
            set
            {
                this.dataUpperRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataUpperRightY
        {
            get
            {
                return this.dataUpperRightYField;
            }
            set
            {
                this.dataUpperRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerLeftX
        {
            get
            {
                return this.dataLowerLeftXField;
            }
            set
            {
                this.dataLowerLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerLeftY
        {
            get
            {
                return this.dataLowerLeftYField;
            }
            set
            {
                this.dataLowerLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerRightX
        {
            get
            {
                return this.dataLowerRightXField;
            }
            set
            {
                this.dataLowerRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataLowerRightY
        {
            get
            {
                return this.dataLowerRightYField;
            }
            set
            {
                this.dataLowerRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperLeftX
        {
            get
            {
                return this.productUpperLeftXField;
            }
            set
            {
                this.productUpperLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperLeftY
        {
            get
            {
                return this.productUpperLeftYField;
            }
            set
            {
                this.productUpperLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperRightX
        {
            get
            {
                return this.productUpperRightXField;
            }
            set
            {
                this.productUpperRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productUpperRightY
        {
            get
            {
                return this.productUpperRightYField;
            }
            set
            {
                this.productUpperRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerLeftX
        {
            get
            {
                return this.productLowerLeftXField;
            }
            set
            {
                this.productLowerLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerLeftY
        {
            get
            {
                return this.productLowerLeftYField;
            }
            set
            {
                this.productLowerLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerRightX
        {
            get
            {
                return this.productLowerRightXField;
            }
            set
            {
                this.productLowerRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLowerRightY
        {
            get
            {
                return this.productLowerRightYField;
            }
            set
            {
                this.productLowerRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataSize
        {
            get
            {
                return this.dataSizeField;
            }
            set
            {
                this.dataSizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneCenterLat
        {
            get
            {
                return this.sceneCenterLatField;
            }
            set
            {
                this.sceneCenterLatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneCenterLong
        {
            get
            {
                return this.sceneCenterLongField;
            }
            set
            {
                this.sceneCenterLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneStartLine
        {
            get
            {
                return this.sceneStartLineField;
            }
            set
            {
                this.sceneStartLineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneStopLine
        {
            get
            {
                return this.sceneStopLineField;
            }
            set
            {
                this.sceneStopLineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Scene_imagingStartTime
        {
            get
            {
                return this.scene_imagingStartTimeField;
            }
            set
            {
                this.scene_imagingStartTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Scene_imagingStopTime
        {
            get
            {
                return this.scene_imagingStopTimeField;
            }
            set
            {
                this.scene_imagingStopTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string scenePath
        {
            get
            {
                return this.scenePathField;
            }
            set
            {
                this.scenePathField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneRow
        {
            get
            {
                return this.sceneRowField;
            }
            set
            {
                this.sceneRowField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string scenePathBias
        {
            get
            {
                return this.scenePathBiasField;
            }
            set
            {
                this.scenePathBiasField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneRowBias
        {
            get
            {
                return this.sceneRowBiasField;
            }
            set
            {
                this.sceneRowBiasField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneCount
        {
            get
            {
                return this.sceneCountField;
            }
            set
            {
                this.sceneCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sceneShift
        {
            get
            {
                return this.sceneShiftField;
            }
            set
            {
                this.sceneShiftField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string direction
        {
            get
            {
                return this.directionField;
            }
            set
            {
                this.directionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string lostFrameNum
        {
            get
            {
                return this.lostFrameNumField;
            }
            set
            {
                this.lostFrameNumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string lineTimeDataStatus
        {
            get
            {
                return this.lineTimeDataStatusField;
            }
            set
            {
                this.lineTimeDataStatusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string mtfcPro
        {
            get
            {
                return this.mtfcProField;
            }
            set
            {
                this.mtfcProField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string resampleTechnique
        {
            get
            {
                return this.resampleTechniqueField;
            }
            set
            {
                this.resampleTechniqueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productLevel
        {
            get
            {
                return this.productLevelField;
            }
            set
            {
                this.productLevelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DeadPixel
        {
            get
            {
                return this.deadPixelField;
            }
            set
            {
                this.deadPixelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string earthModel
        {
            get
            {
                return this.earthModelField;
            }
            set
            {
                this.earthModelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string mapProjection
        {
            get
            {
                return this.mapProjectionField;
            }
            set
            {
                this.mapProjectionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Zone_Number
        {
            get
            {
                return this.zone_NumberField;
            }
            set
            {
                this.zone_NumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string productType
        {
            get
            {
                return this.productTypeField;
            }
            set
            {
                this.productTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bands
        {
            get
            {
                return this.bandsField;
            }
            set
            {
                this.bandsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string gain
        {
            get
            {
                return this.gainField;
            }
            set
            {
                this.gainField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string intergralLevel
        {
            get
            {
                return this.intergralLevelField;
            }
            set
            {
                this.intergralLevelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string instrumentMode
        {
            get
            {
                return this.instrumentModeField;
            }
            set
            {
                this.instrumentModeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sensorGSD
        {
            get
            {
                return this.sensorGSDField;
            }
            set
            {
                this.sensorGSDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string mirrorOffNadir
        {
            get
            {
                return this.mirrorOffNadirField;
            }
            set
            {
                this.mirrorOffNadirField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Satellite_Altitude
        {
            get
            {
                return this.satellite_AltitudeField;
            }
            set
            {
                this.satellite_AltitudeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satZenithAngle
        {
            get
            {
                return this.satZenithAngleField;
            }
            set
            {
                this.satZenithAngleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satAzimuthAngle
        {
            get
            {
                return this.satAzimuthAngleField;
            }
            set
            {
                this.satAzimuthAngleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satOffNadir
        {
            get
            {
                return this.satOffNadirField;
            }
            set
            {
                this.satOffNadirField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satPath
        {
            get
            {
                return this.satPathField;
            }
            set
            {
                this.satPathField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string satRow
        {
            get
            {
                return this.satRowField;
            }
            set
            {
                this.satRowField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ephemerisData
        {
            get
            {
                return this.ephemerisDataField;
            }
            set
            {
                this.ephemerisDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string attitudeData
        {
            get
            {
                return this.attitudeDataField;
            }
            set
            {
                this.attitudeDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string attDataStatus
        {
            get
            {
                return this.attDataStatusField;
            }
            set
            {
                this.attDataStatusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string gpsDataStatus
        {
            get
            {
                return this.gpsDataStatusField;
            }
            set
            {
                this.gpsDataStatusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataFormatDes
        {
            get
            {
                return this.dataFormatDesField;
            }
            set
            {
                this.dataFormatDesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Linkage
        {
            get
            {
                return this.linkageField;
            }
            set
            {
                this.linkageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("quality", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public productMetaQuality[] quality
        {
            get
            {
                return this.qualityField;
            }
            set
            {
                this.qualityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("sunElevation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public productMetaSunElevation[] sunElevation
        {
            get
            {
                return this.sunElevationField;
            }
            set
            {
                this.sunElevationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("sunAzimuth", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public productMetaSunAzimuth[] sunAzimuth
        {
            get
            {
                return this.sunAzimuthField;
            }
            set
            {
                this.sunAzimuthField = value;
            }
        }

        /// <remarks/>
        /// [System.Xml.Serialization.XmlElementAttribute("cloudCoverage", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public productMetaCloudCoverage[] cloudCoverage
        {
            get
            {
                return this.cloudCoverageField;
            }
            set
            {
                this.cloudCoverageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SolarIrradiance", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public productMetaSolarIrradiance[] SolarIrradiance
        {
            get
            {
                return this.solarIrradianceField;
            }
            set
            {
                this.solarIrradianceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ab_calibra_param", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public productMetaAb_calibra_param[] ab_calibra_param
        {
            get
            {
                return this.ab_calibra_paramField;
            }
            set
            {
                this.ab_calibra_paramField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaQuality
    {

        private string rangeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Range
        {
            get
            {
                return this.rangeField;
            }
            set
            {
                this.rangeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaSunElevation
    {

        private string unitsField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaSunAzimuth
    {

        private string unitsField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaCloudCoverage
    {

        private string unitsField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaSolarIrradiance
    {

        private string bandField;

        private string unitsField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Band
        {
            get
            {
                return this.bandField;
            }
            set
            {
                this.bandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class productMetaAb_calibra_param
    {

        private string kField;

        private string bField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string K
        {
            get
            {
                return this.kField;
            }
            set
            {
                this.kField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string B
        {
            get
            {
                return this.bField;
            }
            set
            {
                this.bField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class NewDataSet
    {

        private Metadata[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("productMeta")]
        public Metadata[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }
}
