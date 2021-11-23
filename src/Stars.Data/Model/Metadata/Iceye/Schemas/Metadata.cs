namespace Terradue.Stars.Data.Model.Metadata.Iceye.Schemas
{


    /// <remarks/>

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class coefficient
    {

        private string numberField;

        private string valueField;

        /// <remarks/>

        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>

        public string value
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

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Metadata
    {

        private string spec_versionField;

        private string product_fileField;

        private string product_nameField;

        private string product_typeField;

        private string product_levelField;

        private string satellite_nameField;

        private string acquisition_modeField;

        private string look_sideField;

        private double satellite_look_angleField;

        private string processing_timeField;

        private string processor_versionField;

        private string acquisition_start_utcField;

        private string acquisition_end_utcField;

        private string zerodoppler_start_utcField;

        private string zerodoppler_end_utcField;

        private int number_of_azimuth_samplesField;

        private int number_of_range_samplesField;

        private string orbit_repeat_cycleField;

        private int orbit_relative_numberField;

        private int orbit_absolute_numberField;

        private string orbit_directionField;

        private string sample_precisionField;

        private string polarizationField;

        private string azimuth_looksField;

        private string range_looksField;

        private string range_look_bandwidthField;

        private string range_look_overlapField;

        private string azimuth_look_bandwidthField;

        private string azimuth_look_overlapField;

        private string range_spacingField;

        private string azimuth_spacingField;

        private string acquisition_prfField;

        private string processing_prfField;

        private string carrier_frequencyField;

        private string first_pixel_timeField;

        private string slant_range_to_first_pixelField;

        private string azimuth_time_intervalField;

        private string range_sampling_rateField;

        private string chirp_bandwidthField;

        private string chirp_durationField;

        private string total_processed_bandwidth_azimuthField;

        private string window_function_rangeField;

        private string window_function_azimuthField;

        private string range_spread_comp_flagField;

        private string ant_elev_corr_flagField;

        private string number_of_dc_estimationsField;

        private string dc_estimate_poly_orderField;

        private string doppler_rate_poly_orderField;

        private string grsr_poly_orderField;

        private string incidence_angle_poly_orderField;

        private string gcp_terrain_modelField;

        private string geo_ref_systemField;

        private string avg_scene_heightField;

        private string mean_orbit_altitudeField;

        private string mean_earth_radiusField;

        private string coord_first_nearField;

        private string coord_first_farField;

        private string coord_last_nearField;

        private string coord_last_farField;

        private string coord_centerField;

        private string headingField;

        private string incidence_nearField;

        private double incidence_centerField;

        private string incidence_farField;

        private string calibration_factorField;

        private string orbit_processing_levelField;

        private string tropo_range_delayField;

        private string data_orientationField;

        // private MetadataOrbit_State_Vectors[] orbit_State_VectorsField;

        // private MetadataDoppler_Centroid_CoefficientsDc_coefficients_list[][] doppler_Centroid_CoefficientsField;

        // private MetadataDoppler_Rate[] doppler_RateField;

        // private MetadataGRSR_Coefficients[] gRSR_CoefficientsField;

        // private MetadataIncidence_Angle_Coefficients[] incidence_Angle_CoefficientsField;

        // private MetadataRPC[] rPCField;

        /// <remarks/>

        public string spec_version
        {
            get
            {
                return this.spec_versionField;
            }
            set
            {
                this.spec_versionField = value;
            }
        }

        /// <remarks/>

        public string product_file
        {
            get
            {
                return this.product_fileField;
            }
            set
            {
                this.product_fileField = value;
            }
        }

        /// <remarks/>

        public string product_name
        {
            get
            {
                return this.product_nameField;
            }
            set
            {
                this.product_nameField = value;
            }
        }

        /// <remarks/>

        public string product_type
        {
            get
            {
                return this.product_typeField;
            }
            set
            {
                this.product_typeField = value;
            }
        }

        /// <remarks/>

        public string product_level
        {
            get
            {
                return this.product_levelField;
            }
            set
            {
                this.product_levelField = value;
            }
        }

        /// <remarks/>

        public string satellite_name
        {
            get
            {
                return this.satellite_nameField;
            }
            set
            {
                this.satellite_nameField = value;
            }
        }

        /// <remarks/>

        public string acquisition_mode
        {
            get
            {
                return this.acquisition_modeField;
            }
            set
            {
                this.acquisition_modeField = value;
            }
        }

        /// <remarks/>

        public string look_side
        {
            get
            {
                return this.look_sideField;
            }
            set
            {
                this.look_sideField = value;
            }
        }

        /// <remarks/>

        public double satellite_look_angle
        {
            get
            {
                return this.satellite_look_angleField;
            }
            set
            {
                this.satellite_look_angleField = value;
            }
        }

        /// <remarks/>

        public string processing_time
        {
            get
            {
                return this.processing_timeField;
            }
            set
            {
                this.processing_timeField = value;
            }
        }

        /// <remarks/>

        public string processor_version
        {
            get
            {
                return this.processor_versionField;
            }
            set
            {
                this.processor_versionField = value;
            }
        }

        /// <remarks/>

        public string acquisition_start_utc
        {
            get
            {
                return this.acquisition_start_utcField;
            }
            set
            {
                this.acquisition_start_utcField = value;
            }
        }

        /// <remarks/>

        public string acquisition_end_utc
        {
            get
            {
                return this.acquisition_end_utcField;
            }
            set
            {
                this.acquisition_end_utcField = value;
            }
        }

        /// <remarks/>

        public string zerodoppler_start_utc
        {
            get
            {
                return this.zerodoppler_start_utcField;
            }
            set
            {
                this.zerodoppler_start_utcField = value;
            }
        }

        /// <remarks/>

        public string zerodoppler_end_utc
        {
            get
            {
                return this.zerodoppler_end_utcField;
            }
            set
            {
                this.zerodoppler_end_utcField = value;
            }
        }

        /// <remarks/>

        public int number_of_azimuth_samples
        {
            get
            {
                return this.number_of_azimuth_samplesField;
            }
            set
            {
                this.number_of_azimuth_samplesField = value;
            }
        }

        /// <remarks/>

        public int number_of_range_samples
        {
            get
            {
                return this.number_of_range_samplesField;
            }
            set
            {
                this.number_of_range_samplesField = value;
            }
        }

        /// <remarks/>

        public string orbit_repeat_cycle
        {
            get
            {
                return this.orbit_repeat_cycleField;
            }
            set
            {
                this.orbit_repeat_cycleField = value;
            }
        }

        /// <remarks/>

        public int orbit_relative_number
        {
            get
            {
                return this.orbit_relative_numberField;
            }
            set
            {
                this.orbit_relative_numberField = value;
            }
        }

        /// <remarks/>

        public int orbit_absolute_number
        {
            get
            {
                return this.orbit_absolute_numberField;
            }
            set
            {
                this.orbit_absolute_numberField = value;
            }
        }

        /// <remarks/>

        public string orbit_direction
        {
            get
            {
                return this.orbit_directionField;
            }
            set
            {
                this.orbit_directionField = value;
            }
        }

        /// <remarks/>

        public string sample_precision
        {
            get
            {
                return this.sample_precisionField;
            }
            set
            {
                this.sample_precisionField = value;
            }
        }

        /// <remarks/>

        public string polarization
        {
            get
            {
                return this.polarizationField;
            }
            set
            {
                this.polarizationField = value;
            }
        }

        /// <remarks/>

        public string azimuth_looks
        {
            get
            {
                return this.azimuth_looksField;
            }
            set
            {
                this.azimuth_looksField = value;
            }
        }

        /// <remarks/>

        public string range_looks
        {
            get
            {
                return this.range_looksField;
            }
            set
            {
                this.range_looksField = value;
            }
        }

        /// <remarks/>

        public string range_look_bandwidth
        {
            get
            {
                return this.range_look_bandwidthField;
            }
            set
            {
                this.range_look_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string range_look_overlap
        {
            get
            {
                return this.range_look_overlapField;
            }
            set
            {
                this.range_look_overlapField = value;
            }
        }

        /// <remarks/>

        public string azimuth_look_bandwidth
        {
            get
            {
                return this.azimuth_look_bandwidthField;
            }
            set
            {
                this.azimuth_look_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string azimuth_look_overlap
        {
            get
            {
                return this.azimuth_look_overlapField;
            }
            set
            {
                this.azimuth_look_overlapField = value;
            }
        }

        /// <remarks/>

        public string range_spacing
        {
            get
            {
                return this.range_spacingField;
            }
            set
            {
                this.range_spacingField = value;
            }
        }

        /// <remarks/>

        public string azimuth_spacing
        {
            get
            {
                return this.azimuth_spacingField;
            }
            set
            {
                this.azimuth_spacingField = value;
            }
        }

        /// <remarks/>

        public string acquisition_prf
        {
            get
            {
                return this.acquisition_prfField;
            }
            set
            {
                this.acquisition_prfField = value;
            }
        }

        /// <remarks/>

        public string processing_prf
        {
            get
            {
                return this.processing_prfField;
            }
            set
            {
                this.processing_prfField = value;
            }
        }

        /// <remarks/>

        public string carrier_frequency
        {
            get
            {
                return this.carrier_frequencyField;
            }
            set
            {
                this.carrier_frequencyField = value;
            }
        }

        /// <remarks/>

        public string first_pixel_time
        {
            get
            {
                return this.first_pixel_timeField;
            }
            set
            {
                this.first_pixel_timeField = value;
            }
        }

        /// <remarks/>

        public string slant_range_to_first_pixel
        {
            get
            {
                return this.slant_range_to_first_pixelField;
            }
            set
            {
                this.slant_range_to_first_pixelField = value;
            }
        }

        /// <remarks/>

        public string azimuth_time_interval
        {
            get
            {
                return this.azimuth_time_intervalField;
            }
            set
            {
                this.azimuth_time_intervalField = value;
            }
        }

        /// <remarks/>

        public string range_sampling_rate
        {
            get
            {
                return this.range_sampling_rateField;
            }
            set
            {
                this.range_sampling_rateField = value;
            }
        }

        /// <remarks/>

        public string chirp_bandwidth
        {
            get
            {
                return this.chirp_bandwidthField;
            }
            set
            {
                this.chirp_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string chirp_duration
        {
            get
            {
                return this.chirp_durationField;
            }
            set
            {
                this.chirp_durationField = value;
            }
        }

        /// <remarks/>

        public string total_processed_bandwidth_azimuth
        {
            get
            {
                return this.total_processed_bandwidth_azimuthField;
            }
            set
            {
                this.total_processed_bandwidth_azimuthField = value;
            }
        }

        /// <remarks/>

        public string window_function_range
        {
            get
            {
                return this.window_function_rangeField;
            }
            set
            {
                this.window_function_rangeField = value;
            }
        }

        /// <remarks/>

        public string window_function_azimuth
        {
            get
            {
                return this.window_function_azimuthField;
            }
            set
            {
                this.window_function_azimuthField = value;
            }
        }

        /// <remarks/>

        public string range_spread_comp_flag
        {
            get
            {
                return this.range_spread_comp_flagField;
            }
            set
            {
                this.range_spread_comp_flagField = value;
            }
        }

        /// <remarks/>

        public string ant_elev_corr_flag
        {
            get
            {
                return this.ant_elev_corr_flagField;
            }
            set
            {
                this.ant_elev_corr_flagField = value;
            }
        }

        /// <remarks/>

        public string number_of_dc_estimations
        {
            get
            {
                return this.number_of_dc_estimationsField;
            }
            set
            {
                this.number_of_dc_estimationsField = value;
            }
        }

        /// <remarks/>

        public string dc_estimate_poly_order
        {
            get
            {
                return this.dc_estimate_poly_orderField;
            }
            set
            {
                this.dc_estimate_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string doppler_rate_poly_order
        {
            get
            {
                return this.doppler_rate_poly_orderField;
            }
            set
            {
                this.doppler_rate_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string grsr_poly_order
        {
            get
            {
                return this.grsr_poly_orderField;
            }
            set
            {
                this.grsr_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string incidence_angle_poly_order
        {
            get
            {
                return this.incidence_angle_poly_orderField;
            }
            set
            {
                this.incidence_angle_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string gcp_terrain_model
        {
            get
            {
                return this.gcp_terrain_modelField;
            }
            set
            {
                this.gcp_terrain_modelField = value;
            }
        }

        /// <remarks/>

        public string geo_ref_system
        {
            get
            {
                return this.geo_ref_systemField;
            }
            set
            {
                this.geo_ref_systemField = value;
            }
        }

        /// <remarks/>

        public string avg_scene_height
        {
            get
            {
                return this.avg_scene_heightField;
            }
            set
            {
                this.avg_scene_heightField = value;
            }
        }

        /// <remarks/>

        public string mean_orbit_altitude
        {
            get
            {
                return this.mean_orbit_altitudeField;
            }
            set
            {
                this.mean_orbit_altitudeField = value;
            }
        }

        /// <remarks/>

        public string mean_earth_radius
        {
            get
            {
                return this.mean_earth_radiusField;
            }
            set
            {
                this.mean_earth_radiusField = value;
            }
        }

        /// <remarks/>

        public string coord_first_near
        {
            get
            {
                return this.coord_first_nearField;
            }
            set
            {
                this.coord_first_nearField = value;
            }
        }

        /// <remarks/>

        public string coord_first_far
        {
            get
            {
                return this.coord_first_farField;
            }
            set
            {
                this.coord_first_farField = value;
            }
        }

        /// <remarks/>

        public string coord_last_near
        {
            get
            {
                return this.coord_last_nearField;
            }
            set
            {
                this.coord_last_nearField = value;
            }
        }

        /// <remarks/>

        public string coord_last_far
        {
            get
            {
                return this.coord_last_farField;
            }
            set
            {
                this.coord_last_farField = value;
            }
        }

        /// <remarks/>

        public string coord_center
        {
            get
            {
                return this.coord_centerField;
            }
            set
            {
                this.coord_centerField = value;
            }
        }

        /// <remarks/>

        public string heading
        {
            get
            {
                return this.headingField;
            }
            set
            {
                this.headingField = value;
            }
        }

        /// <remarks/>

        public string incidence_near
        {
            get
            {
                return this.incidence_nearField;
            }
            set
            {
                this.incidence_nearField = value;
            }
        }

        /// <remarks/>

        public double incidence_center
        {
            get
            {
                return this.incidence_centerField;
            }
            set
            {
                this.incidence_centerField = value;
            }
        }

        /// <remarks/>

        public string incidence_far
        {
            get
            {
                return this.incidence_farField;
            }
            set
            {
                this.incidence_farField = value;
            }
        }

        /// <remarks/>

        public string calibration_factor
        {
            get
            {
                return this.calibration_factorField;
            }
            set
            {
                this.calibration_factorField = value;
            }
        }

        /// <remarks/>

        public string orbit_processing_level
        {
            get
            {
                return this.orbit_processing_levelField;
            }
            set
            {
                this.orbit_processing_levelField = value;
            }
        }

        /// <remarks/>

        public string tropo_range_delay
        {
            get
            {
                return this.tropo_range_delayField;
            }
            set
            {
                this.tropo_range_delayField = value;
            }
        }

        /// <remarks/>

        public string data_orientation
        {
            get
            {
                return this.data_orientationField;
            }
            set
            {
                this.data_orientationField = value;
            }
        }
    }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("Orbit_State_Vectors", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataOrbit_State_Vectors[] Orbit_State_Vectors
    //     {
    //         get
    //         {
    //             return this.orbit_State_VectorsField;
    //         }
    //         set
    //         {
    //             this.orbit_State_VectorsField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     [System.Xml.Serialization.XmlArrayItemAttribute("dc_coefficients_list", typeof(MetadataDoppler_Centroid_CoefficientsDc_coefficients_list), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    //     public MetadataDoppler_Centroid_CoefficientsDc_coefficients_list[][] Doppler_Centroid_Coefficients
    //     {
    //         get
    //         {
    //             return this.doppler_Centroid_CoefficientsField;
    //         }
    //         set
    //         {
    //             this.doppler_Centroid_CoefficientsField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("Doppler_Rate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataDoppler_Rate[] Doppler_Rate
    //     {
    //         get
    //         {
    //             return this.doppler_RateField;
    //         }
    //         set
    //         {
    //             this.doppler_RateField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("GRSR_Coefficients", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataGRSR_Coefficients[] GRSR_Coefficients
    //     {
    //         get
    //         {
    //             return this.gRSR_CoefficientsField;
    //         }
    //         set
    //         {
    //             this.gRSR_CoefficientsField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("Incidence_Angle_Coefficients", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataIncidence_Angle_Coefficients[] Incidence_Angle_Coefficients
    //     {
    //         get
    //         {
    //             return this.incidence_Angle_CoefficientsField;
    //         }
    //         set
    //         {
    //             this.incidence_Angle_CoefficientsField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("RPC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataRPC[] RPC
    //     {
    //         get
    //         {
    //             return this.rPCField;
    //         }
    //         set
    //         {
    //             this.rPCField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataOrbit_State_Vectors
    // {

    //     private string countField;

    //     private MetadataOrbit_State_VectorsOrbit_vector[] orbit_vectorField;

    //     /// <remarks/>

    //     public string count
    //     {
    //         get
    //         {
    //             return this.countField;
    //         }
    //         set
    //         {
    //             this.countField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("orbit_vector", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //     public MetadataOrbit_State_VectorsOrbit_vector[] orbit_vector
    //     {
    //         get
    //         {
    //             return this.orbit_vectorField;
    //         }
    //         set
    //         {
    //             this.orbit_vectorField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataOrbit_State_VectorsOrbit_vector
    // {

    //     private string numberField;

    //     private string timeField;

    //     private string posXField;

    //     private string posYField;

    //     private string posZField;

    //     private string velXField;

    //     private string velYField;

    //     private string velZField;

    //     /// <remarks/>

    //     public string number
    //     {
    //         get
    //         {
    //             return this.numberField;
    //         }
    //         set
    //         {
    //             this.numberField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string time
    //     {
    //         get
    //         {
    //             return this.timeField;
    //         }
    //         set
    //         {
    //             this.timeField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string posX
    //     {
    //         get
    //         {
    //             return this.posXField;
    //         }
    //         set
    //         {
    //             this.posXField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string posY
    //     {
    //         get
    //         {
    //             return this.posYField;
    //         }
    //         set
    //         {
    //             this.posYField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string posZ
    //     {
    //         get
    //         {
    //             return this.posZField;
    //         }
    //         set
    //         {
    //             this.posZField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string velX
    //     {
    //         get
    //         {
    //             return this.velXField;
    //         }
    //         set
    //         {
    //             this.velXField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string velY
    //     {
    //         get
    //         {
    //             return this.velYField;
    //         }
    //         set
    //         {
    //             this.velYField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string velZ
    //     {
    //         get
    //         {
    //             return this.velZField;
    //         }
    //         set
    //         {
    //             this.velZField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataDoppler_Centroid_CoefficientsDc_coefficients_list
    // {

    //     private string numberField;

    //     private string zero_doppler_timeField;

    //     private string reference_pixel_timeField;

    //     private coefficient[] coefficientField;

    //     /// <remarks/>

    //     public string number
    //     {
    //         get
    //         {
    //             return this.numberField;
    //         }
    //         set
    //         {
    //             this.numberField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string zero_doppler_time
    //     {
    //         get
    //         {
    //             return this.zero_doppler_timeField;
    //         }
    //         set
    //         {
    //             this.zero_doppler_timeField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string reference_pixel_time
    //     {
    //         get
    //         {
    //             return this.reference_pixel_timeField;
    //         }
    //         set
    //         {
    //             this.reference_pixel_timeField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("coefficient")]
    //     public coefficient[] coefficient
    //     {
    //         get
    //         {
    //             return this.coefficientField;
    //         }
    //         set
    //         {
    //             this.coefficientField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataDoppler_Rate
    // {

    //     private string reference_pixel_timeField;

    //     private coefficient[] coefficientField;

    //     /// <remarks/>

    //     public string reference_pixel_time
    //     {
    //         get
    //         {
    //             return this.reference_pixel_timeField;
    //         }
    //         set
    //         {
    //             this.reference_pixel_timeField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("coefficient")]
    //     public coefficient[] coefficient
    //     {
    //         get
    //         {
    //             return this.coefficientField;
    //         }
    //         set
    //         {
    //             this.coefficientField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataGRSR_Coefficients
    // {

    //     private string zero_doppler_timeField;

    //     private string ground_range_originField;

    //     private coefficient[] coefficientField;

    //     /// <remarks/>

    //     public string zero_doppler_time
    //     {
    //         get
    //         {
    //             return this.zero_doppler_timeField;
    //         }
    //         set
    //         {
    //             this.zero_doppler_timeField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string ground_range_origin
    //     {
    //         get
    //         {
    //             return this.ground_range_originField;
    //         }
    //         set
    //         {
    //             this.ground_range_originField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("coefficient")]
    //     public coefficient[] coefficient
    //     {
    //         get
    //         {
    //             return this.coefficientField;
    //         }
    //         set
    //         {
    //             this.coefficientField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataIncidence_Angle_Coefficients
    // {

    //     private string zero_doppler_timeField;

    //     private string ground_range_originField;

    //     private coefficient[] coefficientField;

    //     /// <remarks/>

    //     public string zero_doppler_time
    //     {
    //         get
    //         {
    //             return this.zero_doppler_timeField;
    //         }
    //         set
    //         {
    //             this.zero_doppler_timeField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string ground_range_origin
    //     {
    //         get
    //         {
    //             return this.ground_range_originField;
    //         }
    //         set
    //         {
    //             this.ground_range_originField = value;
    //         }
    //     }

    //     /// <remarks/>
    //     [System.Xml.Serialization.XmlElementAttribute("coefficient")]
    //     public coefficient[] coefficient
    //     {
    //         get
    //         {
    //             return this.coefficientField;
    //         }
    //         set
    //         {
    //             this.coefficientField = value;
    //         }
    //     }
    // }

    // /// <remarks/>

    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    // public partial class MetadataRPC
    // {

    //     private string lINE_OFFField;

    //     private string sAMP_OFFField;

    //     private string lAT_OFFField;

    //     private string lONG_OFFField;

    //     private string hEIGHT_OFFField;

    //     private string lINE_SCALEField;

    //     private string sAMP_SCALEField;

    //     private string lAT_SCALEField;

    //     private string lONG_SCALEField;

    //     private string hEIGHT_SCALEField;

    //     private string lINE_NUM_COEFFField;

    //     private string lINE_DEN_COEFFField;

    //     private string sAMP_NUM_COEFFField;

    //     private string sAMP_DEN_COEFFField;

    //     /// <remarks/>

    //     public string LINE_OFF
    //     {
    //         get
    //         {
    //             return this.lINE_OFFField;
    //         }
    //         set
    //         {
    //             this.lINE_OFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string SAMP_OFF
    //     {
    //         get
    //         {
    //             return this.sAMP_OFFField;
    //         }
    //         set
    //         {
    //             this.sAMP_OFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LAT_OFF
    //     {
    //         get
    //         {
    //             return this.lAT_OFFField;
    //         }
    //         set
    //         {
    //             this.lAT_OFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LONG_OFF
    //     {
    //         get
    //         {
    //             return this.lONG_OFFField;
    //         }
    //         set
    //         {
    //             this.lONG_OFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string HEIGHT_OFF
    //     {
    //         get
    //         {
    //             return this.hEIGHT_OFFField;
    //         }
    //         set
    //         {
    //             this.hEIGHT_OFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LINE_SCALE
    //     {
    //         get
    //         {
    //             return this.lINE_SCALEField;
    //         }
    //         set
    //         {
    //             this.lINE_SCALEField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string SAMP_SCALE
    //     {
    //         get
    //         {
    //             return this.sAMP_SCALEField;
    //         }
    //         set
    //         {
    //             this.sAMP_SCALEField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LAT_SCALE
    //     {
    //         get
    //         {
    //             return this.lAT_SCALEField;
    //         }
    //         set
    //         {
    //             this.lAT_SCALEField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LONG_SCALE
    //     {
    //         get
    //         {
    //             return this.lONG_SCALEField;
    //         }
    //         set
    //         {
    //             this.lONG_SCALEField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string HEIGHT_SCALE
    //     {
    //         get
    //         {
    //             return this.hEIGHT_SCALEField;
    //         }
    //         set
    //         {
    //             this.hEIGHT_SCALEField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LINE_NUM_COEFF
    //     {
    //         get
    //         {
    //             return this.lINE_NUM_COEFFField;
    //         }
    //         set
    //         {
    //             this.lINE_NUM_COEFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string LINE_DEN_COEFF
    //     {
    //         get
    //         {
    //             return this.lINE_DEN_COEFFField;
    //         }
    //         set
    //         {
    //             this.lINE_DEN_COEFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string SAMP_NUM_COEFF
    //     {
    //         get
    //         {
    //             return this.sAMP_NUM_COEFFField;
    //         }
    //         set
    //         {
    //             this.sAMP_NUM_COEFFField = value;
    //         }
    //     }

    //     /// <remarks/>

    //     public string SAMP_DEN_COEFF
    //     {
    //         get
    //         {
    //             return this.sAMP_DEN_COEFFField;
    //         }
    //         set
    //         {
    //             this.sAMP_DEN_COEFFField = value;
    //         }
    //     }
    // }

}
