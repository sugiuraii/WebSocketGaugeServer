using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace DefiSSMCOM
{
    namespace SSM
    {
        public class SSM_Content_Table
        {
            private Dictionary<SSM_Parameter_Code ,SSM_Numeric_Content> _ssm_numeric_content_table = new Dictionary<SSM_Parameter_Code, SSM_Numeric_Content>();
            private Dictionary<SSM_Switch_Code, SSM_Switch_Content> _ssm_switch_content_table = new Dictionary<SSM_Switch_Code, SSM_Switch_Content>();

            //コンストラクタ
            public SSM_Content_Table()
            {
                set_numeric_content_table();
                set_swicth_content_table();
            }

            //デストラクタ
            ~SSM_Content_Table()
            {
            }

            public SSM_Numeric_Content this[SSM_Parameter_Code code]
            {
                get
                {
                    return _ssm_numeric_content_table[code];
                }
            }

            public SSM_Switch_Content this[SSM_Switch_Code code]
            {
                get
                {
                    return _ssm_switch_content_table[code];
                }
            }

            public void set_all_disable()
            {
                foreach (KeyValuePair<SSM_Parameter_Code,SSM_Numeric_Content> pair in _ssm_numeric_content_table)
                {
                    pair.Value.Slow_Read_Enable = false;
                    pair.Value.Fast_Read_Enable = false;
                }
            }
			public static List<SSM_Switch_Code> get_Switchcodes_from_Parametercode(SSM_Parameter_Code code)
			{
				List<SSM_Switch_Code> return_code_list = new List<SSM_Switch_Code> ();

				switch (code) {
				case(SSM_Parameter_Code.Switch_P0x061):
					return_code_list.Add (SSM_Switch_Code.AT_Vehicle_ID);
					return_code_list.Add (SSM_Switch_Code.Test_Mode_Connector);
					return_code_list.Add (SSM_Switch_Code.Read_Memory_Connector);
					break;

				case(SSM_Parameter_Code.Switch_P0x062):
					return_code_list.Add(SSM_Switch_Code.Neutral_Position_Switch);
					return_code_list.Add(SSM_Switch_Code.Idle_Switch);
					return_code_list.Add(SSM_Switch_Code.Intercooler_AutoWash_Switch);
					return_code_list.Add(SSM_Switch_Code.Ignition_Switch);
					return_code_list.Add(SSM_Switch_Code.Power_Steering_Switch);
					return_code_list.Add(SSM_Switch_Code.Air_Conditioning_Switch);
					break;

				case(SSM_Parameter_Code.Switch_P0x063):
					return_code_list.Add(SSM_Switch_Code.Handle_Switch);
					return_code_list.Add(SSM_Switch_Code.Starter_Switch);
					return_code_list.Add(SSM_Switch_Code.Front_O2_Rich_Signal);
					return_code_list.Add(SSM_Switch_Code.Rear_O2_Rich_Signal);
					return_code_list.Add(SSM_Switch_Code.Front_O2_2_Rich_Signal);
					return_code_list.Add(SSM_Switch_Code.Knock_Signal_1);
					return_code_list.Add(SSM_Switch_Code.Knock_Signal_2);
					return_code_list.Add(SSM_Switch_Code.Electrical_Load_Signal);
					break;

				case(SSM_Parameter_Code.Switch_P0x064):
					return_code_list.Add(SSM_Switch_Code.Crank_Position_Sensor);
					return_code_list.Add(SSM_Switch_Code.Cam_Position_Sensor);
					return_code_list.Add(SSM_Switch_Code.Defogger_Switch);
					return_code_list.Add(SSM_Switch_Code.Blower_Switch);
					return_code_list.Add(SSM_Switch_Code.Interior_Light_Switch);
					return_code_list.Add(SSM_Switch_Code.Wiper_Switch);
					return_code_list.Add(SSM_Switch_Code.AirCon_Lock_Signal);
					return_code_list.Add(SSM_Switch_Code.AirCon_Mid_Pressure_Switch);
					break;

				case(SSM_Parameter_Code.Switch_P0x065):
					return_code_list.Add(SSM_Switch_Code.AirCon_Compressor_Signal);
					return_code_list.Add(SSM_Switch_Code.Radiator_Fan_Relay_3);
					return_code_list.Add(SSM_Switch_Code.Radiator_Fan_Relay_1);
					return_code_list.Add(SSM_Switch_Code.Radiator_Fan_Relay_2);
					return_code_list.Add(SSM_Switch_Code.Fuel_Pump_Relay);
					return_code_list.Add(SSM_Switch_Code.Intercooler_AutoWash_Relay);
					return_code_list.Add(SSM_Switch_Code.CPC_Solenoid_Valve);
					return_code_list.Add(SSM_Switch_Code.BlowBy_Leak_Connector);
					break;

				case(SSM_Parameter_Code.Switch_P0x066):
					return_code_list.Add(SSM_Switch_Code.PCV_Solenoid_Valve);
					return_code_list.Add(SSM_Switch_Code.TGV_Output);
					return_code_list.Add(SSM_Switch_Code.TGV_Drive);
					return_code_list.Add(SSM_Switch_Code.Variable_Intake_Air_Solenoid);
					return_code_list.Add(SSM_Switch_Code.Pressure_Sources_Change);
					return_code_list.Add(SSM_Switch_Code.Vent_Solenoid_Valve);
					return_code_list.Add(SSM_Switch_Code.P_S_Solenoid_Valve);
					return_code_list.Add(SSM_Switch_Code.Assist_Air_Solenoid_Valve);
					break;

				case(SSM_Parameter_Code.Switch_P0x067):
					return_code_list.Add(SSM_Switch_Code.Tank_Sensor_Control_Valve);
					return_code_list.Add(SSM_Switch_Code.Relief_Valve_Solenoid_1);
					return_code_list.Add(SSM_Switch_Code.Relief_Valve_Solenoid_2);
					return_code_list.Add(SSM_Switch_Code.TCS_Relief_Valve_Solenoid);
					return_code_list.Add(SSM_Switch_Code.Ex_Gas_Positive_Pressure);
					return_code_list.Add(SSM_Switch_Code.Ex_Gas_Negative_Pressure);
					return_code_list.Add(SSM_Switch_Code.Intake_Air_Solenoid);
					return_code_list.Add(SSM_Switch_Code.Muffler_Control);
					break;

				case(SSM_Parameter_Code.Switch_P0x068):
					return_code_list.Add(SSM_Switch_Code.Retard_Signal_from_AT);
					return_code_list.Add(SSM_Switch_Code.Fuel_Cut_Signal_from_AT);
					return_code_list.Add(SSM_Switch_Code.Ban_of_Torque_Down);
					return_code_list.Add(SSM_Switch_Code.Request_Torque_Down_VDC);
					break;

				case(SSM_Parameter_Code.Switch_P0x069):
					return_code_list.Add(SSM_Switch_Code.Torque_Control_Signal_1);
					return_code_list.Add(SSM_Switch_Code.Torque_Control_Signal_2);
					return_code_list.Add(SSM_Switch_Code.Torque_Permission_Signal);
					return_code_list.Add(SSM_Switch_Code.EAM_Signal);
					return_code_list.Add(SSM_Switch_Code.AT_coop_lock_up_signal);
					return_code_list.Add(SSM_Switch_Code.AT_coop_lean_burn_signal);
					return_code_list.Add(SSM_Switch_Code.AT_coop_rich_spike_signal);
					return_code_list.Add(SSM_Switch_Code.AET_Signal);
					break;

				case(SSM_Parameter_Code.Switch_P0x120):
					return_code_list.Add(SSM_Switch_Code.ETC_Motor_Relay);
					break;

				case(SSM_Parameter_Code.Switch_P0x121):
					return_code_list.Add(SSM_Switch_Code.Clutch_Switch);
					return_code_list.Add(SSM_Switch_Code.Stop_Light_Switch);
					return_code_list.Add(SSM_Switch_Code.Set_Coast_Switch);
					return_code_list.Add(SSM_Switch_Code.Rsume_Accelerate_Switch);
					return_code_list.Add(SSM_Switch_Code.Brake_Switch);
					return_code_list.Add(SSM_Switch_Code.Accelerator_Switch);
					break;

				default:
					throw new ArgumentException ("Code " + code.ToString () + " doesn't contain Switch code.");
				}

				return return_code_list;
			}

			public static SSM_Parameter_Code get_master_ParameterCode_from_SwitchCode(SSM_Switch_Code code)
			{
				switch (code) {
				case SSM_Switch_Code.AT_Vehicle_ID:
				case SSM_Switch_Code.Test_Mode_Connector:
				case SSM_Switch_Code.Read_Memory_Connector:
					return SSM_Parameter_Code.Switch_P0x061;

				case SSM_Switch_Code.Neutral_Position_Switch:
				case SSM_Switch_Code.Idle_Switch:
				case SSM_Switch_Code.Intercooler_AutoWash_Switch:
				case SSM_Switch_Code.Ignition_Switch:
				case SSM_Switch_Code.Power_Steering_Switch:
				case SSM_Switch_Code.Air_Conditioning_Switch:
					return SSM_Parameter_Code.Switch_P0x062;

				case SSM_Switch_Code.Handle_Switch:
				case SSM_Switch_Code.Starter_Switch:
				case SSM_Switch_Code.Front_O2_Rich_Signal:
				case SSM_Switch_Code.Rear_O2_Rich_Signal:
				case SSM_Switch_Code.Front_O2_2_Rich_Signal:
				case SSM_Switch_Code.Knock_Signal_1:
				case SSM_Switch_Code.Knock_Signal_2:
				case SSM_Switch_Code.Electrical_Load_Signal:
					return SSM_Parameter_Code.Switch_P0x063;

				case SSM_Switch_Code.Crank_Position_Sensor:
				case SSM_Switch_Code.Cam_Position_Sensor:
				case SSM_Switch_Code.Defogger_Switch:
				case SSM_Switch_Code.Blower_Switch:
				case SSM_Switch_Code.Interior_Light_Switch:
				case SSM_Switch_Code.Wiper_Switch:
				case SSM_Switch_Code.AirCon_Lock_Signal:
				case SSM_Switch_Code.AirCon_Mid_Pressure_Switch:
					return SSM_Parameter_Code.Switch_P0x064;

				case SSM_Switch_Code.AirCon_Compressor_Signal:
				case SSM_Switch_Code.Radiator_Fan_Relay_3:
				case SSM_Switch_Code.Radiator_Fan_Relay_1:
				case SSM_Switch_Code.Radiator_Fan_Relay_2:
				case SSM_Switch_Code.Fuel_Pump_Relay:
				case SSM_Switch_Code.Intercooler_AutoWash_Relay:
				case SSM_Switch_Code.CPC_Solenoid_Valve:
				case SSM_Switch_Code.BlowBy_Leak_Connector:
					return SSM_Parameter_Code.Switch_P0x065;

				case SSM_Switch_Code.PCV_Solenoid_Valve:
				case SSM_Switch_Code.TGV_Output:
				case SSM_Switch_Code.TGV_Drive:
				case SSM_Switch_Code.Variable_Intake_Air_Solenoid:
				case SSM_Switch_Code.Pressure_Sources_Change:
				case SSM_Switch_Code.Vent_Solenoid_Valve:
				case SSM_Switch_Code.P_S_Solenoid_Valve:
				case SSM_Switch_Code.Assist_Air_Solenoid_Valve:
					return SSM_Parameter_Code.Switch_P0x066;

				case SSM_Switch_Code.Tank_Sensor_Control_Valve:
				case SSM_Switch_Code.Relief_Valve_Solenoid_1:
				case SSM_Switch_Code.Relief_Valve_Solenoid_2:
				case SSM_Switch_Code.TCS_Relief_Valve_Solenoid:
				case SSM_Switch_Code.Ex_Gas_Positive_Pressure:
				case SSM_Switch_Code.Ex_Gas_Negative_Pressure:
				case SSM_Switch_Code.Intake_Air_Solenoid:
				case SSM_Switch_Code.Muffler_Control:
					return SSM_Parameter_Code.Switch_P0x067;

				case SSM_Switch_Code.Retard_Signal_from_AT:
				case SSM_Switch_Code.Fuel_Cut_Signal_from_AT:
				case SSM_Switch_Code.Ban_of_Torque_Down:
				case SSM_Switch_Code.Request_Torque_Down_VDC:
					return SSM_Parameter_Code.Switch_P0x068;

				case SSM_Switch_Code.Torque_Control_Signal_1:
				case SSM_Switch_Code.Torque_Control_Signal_2:
				case SSM_Switch_Code.Torque_Permission_Signal:
				case SSM_Switch_Code.EAM_Signal:
				case SSM_Switch_Code.AT_coop_lock_up_signal:
				case SSM_Switch_Code.AT_coop_lean_burn_signal:
				case SSM_Switch_Code.AT_coop_rich_spike_signal:
				case SSM_Switch_Code.AET_Signal:
					return SSM_Parameter_Code.Switch_P0x069;

				case SSM_Switch_Code.ETC_Motor_Relay:
					return SSM_Parameter_Code.Switch_P0x120;

				case SSM_Switch_Code.Clutch_Switch:
				case SSM_Switch_Code.Stop_Light_Switch:
				case SSM_Switch_Code.Set_Coast_Switch:
				case SSM_Switch_Code.Rsume_Accelerate_Switch:
				case SSM_Switch_Code.Brake_Switch:
				case SSM_Switch_Code.Accelerator_Switch:
					return SSM_Parameter_Code.Switch_P0x121;
				
				default:
					throw new ArgumentOutOfRangeException("Switch code is out of range");
				}
			}

            private void set_numeric_content_table()
            {
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Engine_Load, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x07 }, 100.0 / 255.0, 0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Coolant_Temperature, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x08 }, 1.0, -40.0, "C"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Correction_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x09 }, 1 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Learning_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x0A }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Correction_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x0B }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Learning_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x0C }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Manifold_Absolute_Pressure, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x0D }, 37.0 / 255.0, 0.0, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Engine_Speed, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x0E, 0x00, 0x00, 0x0F }, 1.0 / 4.0, 0.0, "rpm"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Vehicle_Speed, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x10 }, 1.0, 0.0, "km/h"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Ignition_Timing, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x11 }, 1.0 / 2.0, -128.0 / 2.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_Air_Temperature, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x12 }, 1.0, -40.0, "C"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Mass_Air_Flow, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x13, 0x00, 0x00, 0x14 }, 1.0 / 100.0, 0, "g/s"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Throttle_Opening_Angle, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x15 }, 100.0 / 255.0, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Front_O2_Sensor_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x16, 0x00, 0x00, 0x17 }, 0.005, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Rear_O2_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x18, 0x00, 0x00, 0x19 }, 0.005, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Front_O2_Sensor_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x1A, 0x00, 0x00, 0x1B }, 0.005, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Battery_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x1C }, 0.08, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Flow_Sensor_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x1D }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Throttle_Sensor_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x1E }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Differential_Pressure_Sensor_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x1F }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Injection_1_Pulse_Width, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x20 }, 0.256, 0.0, "ms"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Injection_2_Pulse_Width, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x21 }, 0.256, 0.0, "ms"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Knock_Correction, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x22 }, 1.0 / 2.0, -128.0 / 2.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Atmospheric_Pressure, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x23 }, 37.0 / 255.0, 0.0, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Manifold_Relative_Pressure, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x24 }, 37.0 / 255.0, -37.0 * 128.0 / 255.0, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Pressure_Differential_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x25 }, 37.0 / 255.0, -37.0 * 128.0 / 255.0, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Tank_Pressure, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x26 }, 0.0035, -128.0 * 0.0035, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.CO_Adjustment, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x27 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Learned_Ignition_Timing, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x28 }, 1.0 / 2.0, -128.0 / 2.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Accelerator_Opening_Angle, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x29 }, 1.0 / 2.56, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Temperature, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x2A }, 1.0, -40.0, "C"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Front_O2_Heater_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x2B }, 10.04 / 256.0, 0.0, "A"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Rear_O2_Heater_Current, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x2C }, 10.04 / 256.0, 0.0, "A"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Front_O2_Heater_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x2D }, 10.04 / 256.0, 0.0, "A"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Level, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x2E }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Primary_Wastegate_Duty_Cycle, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x30 }, 100.0 / 255.0, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Secondary_Wastegate_Duty_Cycle, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x31 }, 100.0 / 255.0, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.CPC_Valve_Duty_Ratio, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x32 }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Tumble_Valve_Position_Sensor_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x33 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Tumble_Valve_Position_Sensor_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x34 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Idle_Speed_Control_Valve_Duty_Ratio, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x35 }, 1.0 / 2.0, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Lean_Correction, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x36 }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Heater_Duty, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x37 }, 1.0 / 2.55, 0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Idle_Speed_Control_Valve_Step, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x38 }, 1.0, 0.0, "steps"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Number_of_Ex_Gas_Recirc_Steps, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x39 }, 1.0, 0.0, "steps"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Alternator_Duty, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3A }, 1.0, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Pump_Duty, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3B }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_VVT_Advance_Angle_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3C }, 1.0, -50.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_VVT_Advance_Angle_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3D }, 1.0, -50.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_OCV_Duty_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3E }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_OCV_Duty_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x3F }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_OCV_Current_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x40 }, 32.0, 0.0, "mA"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Intake_OCV_Current_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x41 }, 32.0, 0.0, "mA"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_1_Current, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x42 }, 0.125, -128 * 0.125, "mA"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_2_Current, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x43 }, 0.125, -128 * 0.125, "mA"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_1_Resistance, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x44 }, 1.0, 0.0, "ohm"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_2_Resistance, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x45 }, 1.0, 0.0, "ohm"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x46 }, 1.0 / 128.0, 0.0, "Lambda"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Sensor_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x47 }, 1.0 / 128.0, 0.0, "Lambda"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Gear_Position, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x4A }, 1.0, 1.0, "position"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.A_F_Sensor_1_Heater_Current, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x53 }, 1.0 / 10.0, 0.0, "A"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.A_F_Sensor_2_Heater_Current, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x54 }, 1.0 / 10.0, 0.0, "A"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Roughness_Monitor_Cylinder_1, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xCE }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Roughness_Monitor_Cylinder_2, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xCF }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Correction_3, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD0 }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Learning_3, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD1 }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Rear_O2_Heater_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD2 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Air_Fuel_Adjustment_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD3 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Roughness_Monitor_Cylinder_3, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD8 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Roughness_Monitor_Cylinder_4, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xD9 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Throttle_Motor_Duty, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xFA }, 1.0 / 1.28, -128.0 / 1.28, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Throttle_Motor_Voltage, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0xFB }, 0.08, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Sub_Throttle_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x00 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Main_Throttle_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x01 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Sub_Accelerator_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x02 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Main_Accelerator_Sensor, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x03 }, 0.02, 0.0, "V"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Brake_Booster_Pressure, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x04 }, 37.0 / 255.0, 0.0, "psig"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Fuel_Pressure_High, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x05 }, 0.04, 0.0, "MPa"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_Gas_Temperature, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x06 }, 5.0, 5.0 * 40.0, "C"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Cold_Start_Injector, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x08 }, 0.256, 0.0, "ms"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.SCV_Step, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x09 }, 1.0, 0.0, "step"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Memorised_Cruise_Speed, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x0A }, 1.0, 0.0, "km/h"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_VVT_Advance_Angle_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x18 }, 1.0, -50.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_VVT_Advance_Angle_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x19 }, 1.0, -50.0, "deg"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_OCV_Duty_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x1A }, 1.0 / 2.55, 0.0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_OCV_Duty_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x1B }, 1.0 / 2.55, 0, "%"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_OCV_Current_Right, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x1C }, 32.0, 0.0, "mA"));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Exhaust_OCV_Current_Left, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x1D }, 32.0, 0.0, "mA"));


                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x061, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x61 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x062, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x62 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x063, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x63 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x064, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x64 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x065, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x65 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x066, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x66 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x067, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x67 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x068, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x68 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x069, new SSM_Numeric_Content(new byte[] { 0x00, 0x00, 0x69 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x120, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x20 }, 1.0, 0.0, ""));
                _ssm_numeric_content_table.Add(SSM_Parameter_Code.Switch_P0x121, new SSM_Numeric_Content(new byte[] { 0x00, 0x01, 0x21 }, 1.0, 0.0, ""));
            }

            private void set_swicth_content_table()
            {
                SSM_Numeric_Content master_content;

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x061];
                _ssm_switch_content_table.Add(SSM_Switch_Code.AT_Vehicle_ID,new SSM_Switch_Content(master_content,6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Test_Mode_Connector, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Read_Memory_Connector, new SSM_Switch_Content(master_content, 4));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x062];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Neutral_Position_Switch, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Idle_Switch, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Intercooler_AutoWash_Switch, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Ignition_Switch, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Power_Steering_Switch, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Air_Conditioning_Switch, new SSM_Switch_Content(master_content, 1));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x063];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Handle_Switch,            new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Starter_Switch,           new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Front_O2_Rich_Signal,     new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Rear_O2_Rich_Signal,      new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Front_O2_2_Rich_Signal,   new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Knock_Signal_1,           new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Knock_Signal_2,           new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Electrical_Load_Signal,   new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x064];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Crank_Position_Sensor, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Cam_Position_Sensor, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Defogger_Switch, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Blower_Switch, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Interior_Light_Switch, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Wiper_Switch, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AirCon_Lock_Signal, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AirCon_Mid_Pressure_Switch, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x065];
                _ssm_switch_content_table.Add(SSM_Switch_Code.AirCon_Compressor_Signal, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Radiator_Fan_Relay_3, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Radiator_Fan_Relay_1, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Radiator_Fan_Relay_2, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Fuel_Pump_Relay, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Intercooler_AutoWash_Relay, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.CPC_Solenoid_Valve, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.BlowBy_Leak_Connector, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x066];
                _ssm_switch_content_table.Add(SSM_Switch_Code.PCV_Solenoid_Valve, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.TGV_Output, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.TGV_Drive, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Variable_Intake_Air_Solenoid, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Pressure_Sources_Change, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Vent_Solenoid_Valve, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.P_S_Solenoid_Valve, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Assist_Air_Solenoid_Valve, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x067];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Tank_Sensor_Control_Valve, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Relief_Valve_Solenoid_1, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Relief_Valve_Solenoid_2, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.TCS_Relief_Valve_Solenoid, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Ex_Gas_Positive_Pressure, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Ex_Gas_Negative_Pressure, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Intake_Air_Solenoid, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Muffler_Control, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x068];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Retard_Signal_from_AT, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Fuel_Cut_Signal_from_AT, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Ban_of_Torque_Down, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Request_Torque_Down_VDC, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x069];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Torque_Control_Signal_1, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Torque_Control_Signal_2, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Torque_Permission_Signal, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.EAM_Signal, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AT_coop_lock_up_signal, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AT_coop_lean_burn_signal, new SSM_Switch_Content(master_content, 2));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AT_coop_rich_spike_signal, new SSM_Switch_Content(master_content, 1));
                _ssm_switch_content_table.Add(SSM_Switch_Code.AET_Signal, new SSM_Switch_Content(master_content, 0));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x120];
                _ssm_switch_content_table.Add(SSM_Switch_Code.ETC_Motor_Relay, new SSM_Switch_Content(master_content, 6));

                master_content = _ssm_numeric_content_table[SSM_Parameter_Code.Switch_P0x121];
                _ssm_switch_content_table.Add(SSM_Switch_Code.Clutch_Switch, new SSM_Switch_Content(master_content, 7));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Stop_Light_Switch, new SSM_Switch_Content(master_content, 6));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Set_Coast_Switch, new SSM_Switch_Content(master_content, 5));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Rsume_Accelerate_Switch, new SSM_Switch_Content(master_content, 4));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Brake_Switch, new SSM_Switch_Content(master_content, 3));
                _ssm_switch_content_table.Add(SSM_Switch_Code.Accelerator_Switch, new SSM_Switch_Content(master_content, 1));
            }


        }

        public class SSM_Numeric_Content
        {
            private byte[] _read_address;
            private double _conversion_coefficient;
            private double _conversion_offset;
            private Int32 _raw_value;
            private String _unit;
            private bool _slow_read_enable;
            private bool _fast_read_enable;

            public SSM_Numeric_Content( byte[] address, double conversion_coefficient, double conversion_offset, String unit)
            {
                _read_address = address;
                _conversion_coefficient = conversion_coefficient;
                _conversion_offset = conversion_offset;
                _unit = unit;

                //デフォルトでは無効。明示的にEnableされない限り通信をしない設定とする。
                _slow_read_enable = false;
                _fast_read_enable = false;
            }

            ~SSM_Numeric_Content()
            {
            }

            public double Value
            {
                get
                {
                    return _conversion_coefficient * _raw_value + _conversion_offset;
                }
            }

            public Int32 Address_Length
            {
                get
                {
                    return _read_address.Length;
                }
            }

            public byte[] Read_Address
            {
                get
                {
                    return _read_address;
                }
            }

            public Int32 Raw_Value
            {
                get
                {
                    return _raw_value;
                }
                set
                {
                    _raw_value = value;
                }
            }

            public double Conversion_Coefficient
            {
                get
                {
                    return _conversion_coefficient;
                }
            }

            public double Conversion_Offset
            {
                get
                {
                    return _conversion_offset;
                }
            }
            public String Unit
            {
                get
                {
                    return _unit;
                }
            }

            public bool Slow_Read_Enable
            {
                get
                {
                    return _slow_read_enable;
                }
                set
                {
                    _slow_read_enable = value;
                }
            }
            public bool Fast_Read_Enable
            {
                get
                {
                    return _fast_read_enable;
                }
                set
                {
                    _fast_read_enable = value;
                }
            }

        }

        public class SSM_Switch_Content
        {
            private SSM_Numeric_Content _master_content;
            private int _bit_index;

            public SSM_Switch_Content( SSM_Numeric_Content master_content, int bit_index)
            {
                _master_content = master_content;
                _bit_index = bit_index;
            }

            public bool Value
            {
                get
                {
                    return (_master_content.Raw_Value & 0x01<<_bit_index)>>_bit_index == 1; 
                }
            }
        }
    }
}
