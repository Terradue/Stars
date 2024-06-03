// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Metadata.cs

namespace Terradue.Stars.Data.Model.Metadata.Iceye.Schemas
{


    /// <remarks/>

    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class coefficient
    {

        private string numberField;

        private string valueField;

        /// <remarks/>

        public string number
        {
            get
            {
                return numberField;
            }
            set
            {
                numberField = value;
            }
        }

        /// <remarks/>

        public string value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>

    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
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
                return spec_versionField;
            }
            set
            {
                spec_versionField = value;
            }
        }

        /// <remarks/>

        public string product_file
        {
            get
            {
                return product_fileField;
            }
            set
            {
                product_fileField = value;
            }
        }

        /// <remarks/>

        public string product_name
        {
            get
            {
                return product_nameField;
            }
            set
            {
                product_nameField = value;
            }
        }

        /// <remarks/>

        public string product_type
        {
            get
            {
                return product_typeField;
            }
            set
            {
                product_typeField = value;
            }
        }

        /// <remarks/>

        public string product_level
        {
            get
            {
                return product_levelField;
            }
            set
            {
                product_levelField = value;
            }
        }

        /// <remarks/>

        public string satellite_name
        {
            get
            {
                return satellite_nameField;
            }
            set
            {
                satellite_nameField = value;
            }
        }

        /// <remarks/>

        public string acquisition_mode
        {
            get
            {
                return acquisition_modeField;
            }
            set
            {
                acquisition_modeField = value;
            }
        }

        /// <remarks/>

        public string look_side
        {
            get
            {
                return look_sideField;
            }
            set
            {
                look_sideField = value;
            }
        }

        /// <remarks/>

        public double satellite_look_angle
        {
            get
            {
                return satellite_look_angleField;
            }
            set
            {
                satellite_look_angleField = value;
            }
        }

        /// <remarks/>

        public string processing_time
        {
            get
            {
                return processing_timeField;
            }
            set
            {
                processing_timeField = value;
            }
        }

        /// <remarks/>

        public string processor_version
        {
            get
            {
                return processor_versionField;
            }
            set
            {
                processor_versionField = value;
            }
        }

        /// <remarks/>

        public string acquisition_start_utc
        {
            get
            {
                return acquisition_start_utcField;
            }
            set
            {
                acquisition_start_utcField = value;
            }
        }

        /// <remarks/>

        public string acquisition_end_utc
        {
            get
            {
                return acquisition_end_utcField;
            }
            set
            {
                acquisition_end_utcField = value;
            }
        }

        /// <remarks/>

        public string zerodoppler_start_utc
        {
            get
            {
                return zerodoppler_start_utcField;
            }
            set
            {
                zerodoppler_start_utcField = value;
            }
        }

        /// <remarks/>

        public string zerodoppler_end_utc
        {
            get
            {
                return zerodoppler_end_utcField;
            }
            set
            {
                zerodoppler_end_utcField = value;
            }
        }

        /// <remarks/>

        public int number_of_azimuth_samples
        {
            get
            {
                return number_of_azimuth_samplesField;
            }
            set
            {
                number_of_azimuth_samplesField = value;
            }
        }

        /// <remarks/>

        public int number_of_range_samples
        {
            get
            {
                return number_of_range_samplesField;
            }
            set
            {
                number_of_range_samplesField = value;
            }
        }

        /// <remarks/>

        public string orbit_repeat_cycle
        {
            get
            {
                return orbit_repeat_cycleField;
            }
            set
            {
                orbit_repeat_cycleField = value;
            }
        }

        /// <remarks/>

        public int orbit_relative_number
        {
            get
            {
                return orbit_relative_numberField;
            }
            set
            {
                orbit_relative_numberField = value;
            }
        }

        /// <remarks/>

        public int orbit_absolute_number
        {
            get
            {
                return orbit_absolute_numberField;
            }
            set
            {
                orbit_absolute_numberField = value;
            }
        }

        /// <remarks/>

        public string orbit_direction
        {
            get
            {
                return orbit_directionField;
            }
            set
            {
                orbit_directionField = value;
            }
        }

        /// <remarks/>

        public string sample_precision
        {
            get
            {
                return sample_precisionField;
            }
            set
            {
                sample_precisionField = value;
            }
        }

        /// <remarks/>

        public string polarization
        {
            get
            {
                return polarizationField;
            }
            set
            {
                polarizationField = value;
            }
        }

        /// <remarks/>

        public string azimuth_looks
        {
            get
            {
                return azimuth_looksField;
            }
            set
            {
                azimuth_looksField = value;
            }
        }

        /// <remarks/>

        public string range_looks
        {
            get
            {
                return range_looksField;
            }
            set
            {
                range_looksField = value;
            }
        }

        /// <remarks/>

        public string range_look_bandwidth
        {
            get
            {
                return range_look_bandwidthField;
            }
            set
            {
                range_look_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string range_look_overlap
        {
            get
            {
                return range_look_overlapField;
            }
            set
            {
                range_look_overlapField = value;
            }
        }

        /// <remarks/>

        public string azimuth_look_bandwidth
        {
            get
            {
                return azimuth_look_bandwidthField;
            }
            set
            {
                azimuth_look_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string azimuth_look_overlap
        {
            get
            {
                return azimuth_look_overlapField;
            }
            set
            {
                azimuth_look_overlapField = value;
            }
        }

        /// <remarks/>

        public string range_spacing
        {
            get
            {
                return range_spacingField;
            }
            set
            {
                range_spacingField = value;
            }
        }

        /// <remarks/>

        public string azimuth_spacing
        {
            get
            {
                return azimuth_spacingField;
            }
            set
            {
                azimuth_spacingField = value;
            }
        }

        /// <remarks/>

        public string acquisition_prf
        {
            get
            {
                return acquisition_prfField;
            }
            set
            {
                acquisition_prfField = value;
            }
        }

        /// <remarks/>

        public string processing_prf
        {
            get
            {
                return processing_prfField;
            }
            set
            {
                processing_prfField = value;
            }
        }

        /// <remarks/>

        public string carrier_frequency
        {
            get
            {
                return carrier_frequencyField;
            }
            set
            {
                carrier_frequencyField = value;
            }
        }

        /// <remarks/>

        public string first_pixel_time
        {
            get
            {
                return first_pixel_timeField;
            }
            set
            {
                first_pixel_timeField = value;
            }
        }

        /// <remarks/>

        public string slant_range_to_first_pixel
        {
            get
            {
                return slant_range_to_first_pixelField;
            }
            set
            {
                slant_range_to_first_pixelField = value;
            }
        }

        /// <remarks/>

        public string azimuth_time_interval
        {
            get
            {
                return azimuth_time_intervalField;
            }
            set
            {
                azimuth_time_intervalField = value;
            }
        }

        /// <remarks/>

        public string range_sampling_rate
        {
            get
            {
                return range_sampling_rateField;
            }
            set
            {
                range_sampling_rateField = value;
            }
        }

        /// <remarks/>

        public string chirp_bandwidth
        {
            get
            {
                return chirp_bandwidthField;
            }
            set
            {
                chirp_bandwidthField = value;
            }
        }

        /// <remarks/>

        public string chirp_duration
        {
            get
            {
                return chirp_durationField;
            }
            set
            {
                chirp_durationField = value;
            }
        }

        /// <remarks/>

        public string total_processed_bandwidth_azimuth
        {
            get
            {
                return total_processed_bandwidth_azimuthField;
            }
            set
            {
                total_processed_bandwidth_azimuthField = value;
            }
        }

        /// <remarks/>

        public string window_function_range
        {
            get
            {
                return window_function_rangeField;
            }
            set
            {
                window_function_rangeField = value;
            }
        }

        /// <remarks/>

        public string window_function_azimuth
        {
            get
            {
                return window_function_azimuthField;
            }
            set
            {
                window_function_azimuthField = value;
            }
        }

        /// <remarks/>

        public string range_spread_comp_flag
        {
            get
            {
                return range_spread_comp_flagField;
            }
            set
            {
                range_spread_comp_flagField = value;
            }
        }

        /// <remarks/>

        public string ant_elev_corr_flag
        {
            get
            {
                return ant_elev_corr_flagField;
            }
            set
            {
                ant_elev_corr_flagField = value;
            }
        }

        /// <remarks/>

        public string number_of_dc_estimations
        {
            get
            {
                return number_of_dc_estimationsField;
            }
            set
            {
                number_of_dc_estimationsField = value;
            }
        }

        /// <remarks/>

        public string dc_estimate_poly_order
        {
            get
            {
                return dc_estimate_poly_orderField;
            }
            set
            {
                dc_estimate_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string doppler_rate_poly_order
        {
            get
            {
                return doppler_rate_poly_orderField;
            }
            set
            {
                doppler_rate_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string grsr_poly_order
        {
            get
            {
                return grsr_poly_orderField;
            }
            set
            {
                grsr_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string incidence_angle_poly_order
        {
            get
            {
                return incidence_angle_poly_orderField;
            }
            set
            {
                incidence_angle_poly_orderField = value;
            }
        }

        /// <remarks/>

        public string gcp_terrain_model
        {
            get
            {
                return gcp_terrain_modelField;
            }
            set
            {
                gcp_terrain_modelField = value;
            }
        }

        /// <remarks/>

        public string geo_ref_system
        {
            get
            {
                return geo_ref_systemField;
            }
            set
            {
                geo_ref_systemField = value;
            }
        }

        /// <remarks/>

        public string avg_scene_height
        {
            get
            {
                return avg_scene_heightField;
            }
            set
            {
                avg_scene_heightField = value;
            }
        }

        /// <remarks/>

        public string mean_orbit_altitude
        {
            get
            {
                return mean_orbit_altitudeField;
            }
            set
            {
                mean_orbit_altitudeField = value;
            }
        }

        /// <remarks/>

        public string mean_earth_radius
        {
            get
            {
                return mean_earth_radiusField;
            }
            set
            {
                mean_earth_radiusField = value;
            }
        }

        /// <remarks/>

        public string coord_first_near
        {
            get
            {
                return coord_first_nearField;
            }
            set
            {
                coord_first_nearField = value;
            }
        }

        /// <remarks/>

        public string coord_first_far
        {
            get
            {
                return coord_first_farField;
            }
            set
            {
                coord_first_farField = value;
            }
        }

        /// <remarks/>

        public string coord_last_near
        {
            get
            {
                return coord_last_nearField;
            }
            set
            {
                coord_last_nearField = value;
            }
        }

        /// <remarks/>

        public string coord_last_far
        {
            get
            {
                return coord_last_farField;
            }
            set
            {
                coord_last_farField = value;
            }
        }

        /// <remarks/>

        public string coord_center
        {
            get
            {
                return coord_centerField;
            }
            set
            {
                coord_centerField = value;
            }
        }

        /// <remarks/>

        public string heading
        {
            get
            {
                return headingField;
            }
            set
            {
                headingField = value;
            }
        }

        /// <remarks/>

        public string incidence_near
        {
            get
            {
                return incidence_nearField;
            }
            set
            {
                incidence_nearField = value;
            }
        }

        /// <remarks/>

        public double incidence_center
        {
            get
            {
                return incidence_centerField;
            }
            set
            {
                incidence_centerField = value;
            }
        }

        /// <remarks/>

        public string incidence_far
        {
            get
            {
                return incidence_farField;
            }
            set
            {
                incidence_farField = value;
            }
        }

        /// <remarks/>

        public string calibration_factor
        {
            get
            {
                return calibration_factorField;
            }
            set
            {
                calibration_factorField = value;
            }
        }

        /// <remarks/>

        public string orbit_processing_level
        {
            get
            {
                return orbit_processing_levelField;
            }
            set
            {
                orbit_processing_levelField = value;
            }
        }

        /// <remarks/>

        public string tropo_range_delay
        {
            get
            {
                return tropo_range_delayField;
            }
            set
            {
                tropo_range_delayField = value;
            }
        }

        /// <remarks/>

        public string data_orientation
        {
            get
            {
                return data_orientationField;
            }
            set
            {
                data_orientationField = value;
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
