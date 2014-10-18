using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DefiSSMCOM
{
    namespace Communication
    {
        namespace SSM
        {
            public enum SSM_Parameter_Code
            {
                // SSMのパラメーターリスト
                Engine_Load,
                Coolant_Temperature,
                Air_Fuel_Correction_1,
                Air_Fuel_Learning_1,
                Air_Fuel_Correction_2,
                Air_Fuel_Learning_2,
                Manifold_Absolute_Pressure,
                Engine_Speed,
                Vehicle_Speed,
                Ignition_Timing,
                Intake_Air_Temperature,
                Mass_Air_Flow,
                Throttle_Opening_Angle,
                Front_O2_Sensor_1,
                Rear_O2_Sensor,
                Front_O2_Sensor_2,
                Battery_Voltage,
                Air_Flow_Sensor_Voltage,
                Throttle_Sensor_Voltage,
                Differential_Pressure_Sensor_Voltage,
                Fuel_Injection_1_Pulse_Width,
                Fuel_Injection_2_Pulse_Width,
                Knock_Correction,
                Atmospheric_Pressure,
                Manifold_Relative_Pressure,
                Pressure_Differential_Sensor,
                Fuel_Tank_Pressure,
                CO_Adjustment,
                Learned_Ignition_Timing,
                Accelerator_Opening_Angle,
                Fuel_Temperature,
                Front_O2_Heater_1,
                Rear_O2_Heater_Current,
                Front_O2_Heater_2,
                Fuel_Level,
                Primary_Wastegate_Duty_Cycle,
                Secondary_Wastegate_Duty_Cycle,
                CPC_Valve_Duty_Ratio,
                Tumble_Valve_Position_Sensor_Right,
                Tumble_Valve_Position_Sensor_Left,
                Idle_Speed_Control_Valve_Duty_Ratio,
                Air_Fuel_Lean_Correction,
                Air_Fuel_Heater_Duty,
                Idle_Speed_Control_Valve_Step,
                Number_of_Ex_Gas_Recirc_Steps,
                Alternator_Duty,
                Fuel_Pump_Duty,
                Intake_VVT_Advance_Angle_Right,
                Intake_VVT_Advance_Angle_Left,
                Intake_OCV_Duty_Right,
                Intake_OCV_Duty_Left,
                Intake_OCV_Current_Right,
                Intake_OCV_Current_Left,
                Air_Fuel_Sensor_1_Current,
                Air_Fuel_Sensor_2_Current,
                Air_Fuel_Sensor_1_Resistance,
                Air_Fuel_Sensor_2_Resistance,
                Air_Fuel_Sensor_1,
                Air_Fuel_Sensor_2,
                Gear_Position,
                A_F_Sensor_1_Heater_Current,
                A_F_Sensor_2_Heater_Current,
                Roughness_Monitor_Cylinder_1,
                Roughness_Monitor_Cylinder_2,
                Air_Fuel_Correction_3,
                Air_Fuel_Learning_3,
                Rear_O2_Heater_Voltage,
                Air_Fuel_Adjustment_Voltage,
                Roughness_Monitor_Cylinder_3,
                Roughness_Monitor_Cylinder_4,
                Throttle_Motor_Duty,
                Throttle_Motor_Voltage,
                Sub_Throttle_Sensor,
                Main_Throttle_Sensor,
                Sub_Accelerator_Sensor,
                Main_Accelerator_Sensor,
                Brake_Booster_Pressure,
                Fuel_Pressure_High,
                Exhaust_Gas_Temperature,
                Cold_Start_Injector,
                SCV_Step,
                Memorised_Cruise_Speed,
                Exhaust_VVT_Advance_Angle_Right,
                Exhaust_VVT_Advance_Angle_Left,
                Exhaust_OCV_Duty_Right,
                Exhaust_OCV_Duty_Left,
                Exhaust_OCV_Current_Right,
                Exhaust_OCV_Current_Left,

                // For Swicth Contents
                Switch_P0x061,
                Switch_P0x062,
                Switch_P0x063,
                Switch_P0x064,
                Switch_P0x065,
                Switch_P0x066,
                Switch_P0x067,
                Switch_P0x068,
                Switch_P0x069,
                Switch_P0x120,
                Switch_P0x121
            };
            public enum SSM_Switch_Code
            {
                // Switch_P0x061
                AT_Vehicle_ID,
                Test_Mode_Connector,
                Read_Memory_Connector,

                // Switch_P0x062
                Neutral_Position_Switch,
                Idle_Switch,
                Intercooler_AutoWash_Switch,
                Ignition_Switch,
                Power_Steering_Switch,
                Air_Conditioning_Switch,

                // Switch_P0x063
                Handle_Switch,
                Starter_Switch,
                Front_O2_Rich_Signal,
                Rear_O2_Rich_Signal,
                Front_O2_2_Rich_Signal,
                Knock_Signal_1,
                Knock_Signal_2,
                Electrical_Load_Signal,

                // Switch_P0x064
                Crank_Position_Sensor,
                Cam_Position_Sensor,
                Defogger_Switch,
                Blower_Switch,
                Interior_Light_Switch,
                Wiper_Switch,
                AirCon_Lock_Signal,
                AirCon_Mid_Pressure_Switch,

                // Switch_P0x065
                AirCon_Compressor_Signal,
                Radiator_Fan_Relay_3,
                Radiator_Fan_Relay_1,
                Radiator_Fan_Relay_2,
                Fuel_Pump_Relay,
                Intercooler_AutoWash_Relay,
                CPC_Solenoid_Valve,
                BlowBy_Leak_Connector,

                // Switch_P0x066
                PCV_Solenoid_Valve,
                TGV_Output,
                TGV_Drive,
                Variable_Intake_Air_Solenoid,
                Pressure_Sources_Change,
                Vent_Solenoid_Valve,
                P_S_Solenoid_Valve,
                Assist_Air_Solenoid_Valve,

                // Switch_P0x067
                Tank_Sensor_Control_Valve,
                Relief_Valve_Solenoid_1,
                Relief_Valve_Solenoid_2,
                TCS_Relief_Valve_Solenoid,
                Ex_Gas_Positive_Pressure,
                Ex_Gas_Negative_Pressure,
                Intake_Air_Solenoid,
                Muffler_Control,

                // Switch_P0x068
                Retard_Signal_from_AT,
                Fuel_Cut_Signal_from_AT,
                Ban_of_Torque_Down,
                Request_Torque_Down_VDC,

                // Switch_P0x069
                Torque_Control_Signal_1,
                Torque_Control_Signal_2,
                Torque_Permission_Signal,
                EAM_Signal,
                AT_coop_lock_up_signal,
                AT_coop_lean_burn_signal,
                AT_coop_rich_spike_signal,
                AET_Signal,

                // Switch_P0x120
                ETC_Motor_Relay,

                // Switch_P0x121
                Clutch_Switch,
                Stop_Light_Switch,
                Set_Coast_Switch,
                Rsume_Accelerate_Switch,
                Brake_Switch,
                Accelerator_Switch
            }
        }
    }
}

