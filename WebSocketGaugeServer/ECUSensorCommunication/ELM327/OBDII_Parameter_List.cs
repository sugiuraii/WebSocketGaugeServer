﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public enum OBDIIParameterCode
    {
        Engine_Load,
        Coolant_Temperature,
        Air_Fuel_Correction_1,
        Air_Fuel_Learning_1,
        Air_Fuel_Correction_2,
        Air_Fuel_Learning_2,
        Fuel_Tank_Pressure,
        Manifold_Absolute_Pressure,
        Engine_Speed,
        Vehicle_Speed,
        Ignition_Timing,
        Intake_Air_Temperature,
        Mass_Air_Flow,
        Throttle_Opening_Angle,
        Run_time_since_engine_start,
        Distance_traveled_with_MIL_on,
        Fuel_Rail_Pressure,
        Fuel_Rail_Pressure_diesel,
        Commanded_EGR,
        EGR_Error,
        Commanded_evaporative_purge,
        Fuel_Level_Input,
        Number_of_warmups_since_codes_cleared,
        Distance_traveled_since_codes_cleared,
        Evap_System_Vapor_Pressure,
        Atmospheric_Pressure,
        Catalyst_TemperatureBank_1_Sensor_1,
        Catalyst_TemperatureBank_2_Sensor_1,
        Catalyst_TemperatureBank_1_Sensor_2,
        Catalyst_TemperatureBank_2_Sensor_2,
        Battery_Voltage,
        Absolute_load_value,
        Command_equivalence_ratio,
        Relative_throttle_position,
        Ambient_air_temperature,
        Absolute_throttle_position_B,
        Absolute_throttle_position_C,
        Accelerator_pedal_position_D,
        Accelerator_pedal_position_E,
        Accelerator_pedal_position_F,
        Commanded_throttle_actuator,
        Time_run_with_MIL_on,
        Time_since_trouble_codes_cleared,
        Ethanol_fuel_percent,

        // Added on 2018/10/07-----------------
        O2Sensor_1_Air_Fuel_Correction,
        O2Sensor_2_Air_Fuel_Correction,
        O2Sensor_3_Air_Fuel_Correction,
        O2Sensor_4_Air_Fuel_Correction,
        O2Sensor_5_Air_Fuel_Correction,
        O2Sensor_6_Air_Fuel_Correction,
        O2Sensor_7_Air_Fuel_Correction,
        O2Sensor_8_Air_Fuel_Correction,

        O2Sensor_1_Air_Fuel_Ratio,
        O2Sensor_2_Air_Fuel_Ratio,
        O2Sensor_3_Air_Fuel_Ratio,
        O2Sensor_4_Air_Fuel_Ratio,
        O2Sensor_5_Air_Fuel_Ratio,
        O2Sensor_6_Air_Fuel_Ratio,
        O2Sensor_7_Air_Fuel_Ratio,
        O2Sensor_8_Air_Fuel_Ratio,

        Evap_system_vapor_pressure,
        Fuel_rail_absolute_pressure,
        Relative_accelerator_pedal_position,
        Hybrid_battery_pack_remaining_life,
        Engine_oil_temperature,
        Fuel_injection_timing,
        Engine_fuel_rate,
        Driver_demand_engine_percent_torque,
        Actual_engine_percent_torque,
        Engine_reference_torque
    };
}
