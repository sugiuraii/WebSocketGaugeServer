using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM.OBDII
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

        // Temporary added on 2017/12/14
        Engine_fuel_rate,
        Engine_oil_temperature,
        Hybrid_battery_pack_remaining_life
    };
}
