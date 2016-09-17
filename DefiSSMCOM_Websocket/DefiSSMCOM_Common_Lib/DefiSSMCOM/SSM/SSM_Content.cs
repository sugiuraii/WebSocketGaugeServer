using System;
using System.Collections.Generic;

namespace DefiSSMCOM
{
    public class SSMContentTable
    {
        private Dictionary<SSMParameterCode ,SSMNumericContent> _ssm_numeric_content_table = new Dictionary<SSMParameterCode, SSMNumericContent>();
        private Dictionary<SSMSwitchCode, SSMSwitchContent> _ssm_switch_content_table = new Dictionary<SSMSwitchCode, SSMSwitchContent>();

        //コンストラクタ
        public SSMContentTable()
        {
            set_numeric_content_table();
            set_swicth_content_table();
        }

        public SSMNumericContent this[SSMParameterCode code]
        {
            get
            {
                return _ssm_numeric_content_table[code];
            }
        }

        public SSMSwitchContent this[SSMSwitchCode code]
        {
            get
            {
                return _ssm_switch_content_table[code];
            }
        }

        public void set_all_disable()
        {
            foreach (KeyValuePair<SSMParameterCode,SSMNumericContent> pair in _ssm_numeric_content_table)
            {
                pair.Value.Slow_Read_Enable = false;
                pair.Value.Fast_Read_Enable = false;
            }
        }
		public static List<SSMSwitchCode> get_Switchcodes_from_Parametercode(SSMParameterCode code)
		{
			List<SSMSwitchCode> return_code_list = new List<SSMSwitchCode> ();

			switch (code) {
			case(SSMParameterCode.Switch_P0x061):
				return_code_list.Add (SSMSwitchCode.AT_Vehicle_ID);
				return_code_list.Add (SSMSwitchCode.Test_Mode_Connector);
				return_code_list.Add (SSMSwitchCode.Read_Memory_Connector);
				break;

			case(SSMParameterCode.Switch_P0x062):
				return_code_list.Add(SSMSwitchCode.Neutral_Position_Switch);
				return_code_list.Add(SSMSwitchCode.Idle_Switch);
				return_code_list.Add(SSMSwitchCode.Intercooler_AutoWash_Switch);
				return_code_list.Add(SSMSwitchCode.Ignition_Switch);
				return_code_list.Add(SSMSwitchCode.Power_Steering_Switch);
				return_code_list.Add(SSMSwitchCode.Air_Conditioning_Switch);
				break;

			case(SSMParameterCode.Switch_P0x063):
				return_code_list.Add(SSMSwitchCode.Handle_Switch);
				return_code_list.Add(SSMSwitchCode.Starter_Switch);
				return_code_list.Add(SSMSwitchCode.Front_O2_Rich_Signal);
				return_code_list.Add(SSMSwitchCode.Rear_O2_Rich_Signal);
				return_code_list.Add(SSMSwitchCode.Front_O2_2_Rich_Signal);
				return_code_list.Add(SSMSwitchCode.Knock_Signal_1);
				return_code_list.Add(SSMSwitchCode.Knock_Signal_2);
				return_code_list.Add(SSMSwitchCode.Electrical_Load_Signal);
				break;

			case(SSMParameterCode.Switch_P0x064):
				return_code_list.Add(SSMSwitchCode.Crank_Position_Sensor);
				return_code_list.Add(SSMSwitchCode.Cam_Position_Sensor);
				return_code_list.Add(SSMSwitchCode.Defogger_Switch);
				return_code_list.Add(SSMSwitchCode.Blower_Switch);
				return_code_list.Add(SSMSwitchCode.Interior_Light_Switch);
				return_code_list.Add(SSMSwitchCode.Wiper_Switch);
				return_code_list.Add(SSMSwitchCode.AirCon_Lock_Signal);
				return_code_list.Add(SSMSwitchCode.AirCon_Mid_Pressure_Switch);
				break;

			case(SSMParameterCode.Switch_P0x065):
				return_code_list.Add(SSMSwitchCode.AirCon_Compressor_Signal);
				return_code_list.Add(SSMSwitchCode.Radiator_Fan_Relay_3);
				return_code_list.Add(SSMSwitchCode.Radiator_Fan_Relay_1);
				return_code_list.Add(SSMSwitchCode.Radiator_Fan_Relay_2);
				return_code_list.Add(SSMSwitchCode.Fuel_Pump_Relay);
				return_code_list.Add(SSMSwitchCode.Intercooler_AutoWash_Relay);
				return_code_list.Add(SSMSwitchCode.CPC_Solenoid_Valve);
				return_code_list.Add(SSMSwitchCode.BlowBy_Leak_Connector);
				break;

			case(SSMParameterCode.Switch_P0x066):
				return_code_list.Add(SSMSwitchCode.PCV_Solenoid_Valve);
				return_code_list.Add(SSMSwitchCode.TGV_Output);
				return_code_list.Add(SSMSwitchCode.TGV_Drive);
				return_code_list.Add(SSMSwitchCode.Variable_Intake_Air_Solenoid);
				return_code_list.Add(SSMSwitchCode.Pressure_Sources_Change);
				return_code_list.Add(SSMSwitchCode.Vent_Solenoid_Valve);
				return_code_list.Add(SSMSwitchCode.P_S_Solenoid_Valve);
				return_code_list.Add(SSMSwitchCode.Assist_Air_Solenoid_Valve);
				break;

			case(SSMParameterCode.Switch_P0x067):
				return_code_list.Add(SSMSwitchCode.Tank_Sensor_Control_Valve);
				return_code_list.Add(SSMSwitchCode.Relief_Valve_Solenoid_1);
				return_code_list.Add(SSMSwitchCode.Relief_Valve_Solenoid_2);
				return_code_list.Add(SSMSwitchCode.TCS_Relief_Valve_Solenoid);
				return_code_list.Add(SSMSwitchCode.Ex_Gas_Positive_Pressure);
				return_code_list.Add(SSMSwitchCode.Ex_Gas_Negative_Pressure);
				return_code_list.Add(SSMSwitchCode.Intake_Air_Solenoid);
				return_code_list.Add(SSMSwitchCode.Muffler_Control);
				break;

			case(SSMParameterCode.Switch_P0x068):
				return_code_list.Add(SSMSwitchCode.Retard_Signal_from_AT);
				return_code_list.Add(SSMSwitchCode.Fuel_Cut_Signal_from_AT);
				return_code_list.Add(SSMSwitchCode.Ban_of_Torque_Down);
				return_code_list.Add(SSMSwitchCode.Request_Torque_Down_VDC);
				break;

			case(SSMParameterCode.Switch_P0x069):
				return_code_list.Add(SSMSwitchCode.Torque_Control_Signal_1);
				return_code_list.Add(SSMSwitchCode.Torque_Control_Signal_2);
				return_code_list.Add(SSMSwitchCode.Torque_Permission_Signal);
				return_code_list.Add(SSMSwitchCode.EAM_Signal);
				return_code_list.Add(SSMSwitchCode.AT_coop_lock_up_signal);
				return_code_list.Add(SSMSwitchCode.AT_coop_lean_burn_signal);
				return_code_list.Add(SSMSwitchCode.AT_coop_rich_spike_signal);
				return_code_list.Add(SSMSwitchCode.AET_Signal);
				break;

			case(SSMParameterCode.Switch_P0x120):
				return_code_list.Add(SSMSwitchCode.ETC_Motor_Relay);
				break;

			case(SSMParameterCode.Switch_P0x121):
				return_code_list.Add(SSMSwitchCode.Clutch_Switch);
				return_code_list.Add(SSMSwitchCode.Stop_Light_Switch);
				return_code_list.Add(SSMSwitchCode.Set_Coast_Switch);
				return_code_list.Add(SSMSwitchCode.Rsume_Accelerate_Switch);
				return_code_list.Add(SSMSwitchCode.Brake_Switch);
				return_code_list.Add(SSMSwitchCode.Accelerator_Switch);
				break;

			default:
				throw new ArgumentException ("Code " + code.ToString () + " doesn't contain Switch code.");
			}

			return return_code_list;
		}

		public static SSMParameterCode get_master_ParameterCode_from_SwitchCode(SSMSwitchCode code)
		{
			switch (code) {
			case SSMSwitchCode.AT_Vehicle_ID:
			case SSMSwitchCode.Test_Mode_Connector:
			case SSMSwitchCode.Read_Memory_Connector:
				return SSMParameterCode.Switch_P0x061;

			case SSMSwitchCode.Neutral_Position_Switch:
			case SSMSwitchCode.Idle_Switch:
			case SSMSwitchCode.Intercooler_AutoWash_Switch:
			case SSMSwitchCode.Ignition_Switch:
			case SSMSwitchCode.Power_Steering_Switch:
			case SSMSwitchCode.Air_Conditioning_Switch:
				return SSMParameterCode.Switch_P0x062;

			case SSMSwitchCode.Handle_Switch:
			case SSMSwitchCode.Starter_Switch:
			case SSMSwitchCode.Front_O2_Rich_Signal:
			case SSMSwitchCode.Rear_O2_Rich_Signal:
			case SSMSwitchCode.Front_O2_2_Rich_Signal:
			case SSMSwitchCode.Knock_Signal_1:
			case SSMSwitchCode.Knock_Signal_2:
			case SSMSwitchCode.Electrical_Load_Signal:
				return SSMParameterCode.Switch_P0x063;

			case SSMSwitchCode.Crank_Position_Sensor:
			case SSMSwitchCode.Cam_Position_Sensor:
			case SSMSwitchCode.Defogger_Switch:
			case SSMSwitchCode.Blower_Switch:
			case SSMSwitchCode.Interior_Light_Switch:
			case SSMSwitchCode.Wiper_Switch:
			case SSMSwitchCode.AirCon_Lock_Signal:
			case SSMSwitchCode.AirCon_Mid_Pressure_Switch:
				return SSMParameterCode.Switch_P0x064;

			case SSMSwitchCode.AirCon_Compressor_Signal:
			case SSMSwitchCode.Radiator_Fan_Relay_3:
			case SSMSwitchCode.Radiator_Fan_Relay_1:
			case SSMSwitchCode.Radiator_Fan_Relay_2:
			case SSMSwitchCode.Fuel_Pump_Relay:
			case SSMSwitchCode.Intercooler_AutoWash_Relay:
			case SSMSwitchCode.CPC_Solenoid_Valve:
			case SSMSwitchCode.BlowBy_Leak_Connector:
				return SSMParameterCode.Switch_P0x065;

			case SSMSwitchCode.PCV_Solenoid_Valve:
			case SSMSwitchCode.TGV_Output:
			case SSMSwitchCode.TGV_Drive:
			case SSMSwitchCode.Variable_Intake_Air_Solenoid:
			case SSMSwitchCode.Pressure_Sources_Change:
			case SSMSwitchCode.Vent_Solenoid_Valve:
			case SSMSwitchCode.P_S_Solenoid_Valve:
			case SSMSwitchCode.Assist_Air_Solenoid_Valve:
				return SSMParameterCode.Switch_P0x066;

			case SSMSwitchCode.Tank_Sensor_Control_Valve:
			case SSMSwitchCode.Relief_Valve_Solenoid_1:
			case SSMSwitchCode.Relief_Valve_Solenoid_2:
			case SSMSwitchCode.TCS_Relief_Valve_Solenoid:
			case SSMSwitchCode.Ex_Gas_Positive_Pressure:
			case SSMSwitchCode.Ex_Gas_Negative_Pressure:
			case SSMSwitchCode.Intake_Air_Solenoid:
			case SSMSwitchCode.Muffler_Control:
				return SSMParameterCode.Switch_P0x067;

			case SSMSwitchCode.Retard_Signal_from_AT:
			case SSMSwitchCode.Fuel_Cut_Signal_from_AT:
			case SSMSwitchCode.Ban_of_Torque_Down:
			case SSMSwitchCode.Request_Torque_Down_VDC:
				return SSMParameterCode.Switch_P0x068;

			case SSMSwitchCode.Torque_Control_Signal_1:
			case SSMSwitchCode.Torque_Control_Signal_2:
			case SSMSwitchCode.Torque_Permission_Signal:
			case SSMSwitchCode.EAM_Signal:
			case SSMSwitchCode.AT_coop_lock_up_signal:
			case SSMSwitchCode.AT_coop_lean_burn_signal:
			case SSMSwitchCode.AT_coop_rich_spike_signal:
			case SSMSwitchCode.AET_Signal:
				return SSMParameterCode.Switch_P0x069;

			case SSMSwitchCode.ETC_Motor_Relay:
				return SSMParameterCode.Switch_P0x120;

			case SSMSwitchCode.Clutch_Switch:
			case SSMSwitchCode.Stop_Light_Switch:
			case SSMSwitchCode.Set_Coast_Switch:
			case SSMSwitchCode.Rsume_Accelerate_Switch:
			case SSMSwitchCode.Brake_Switch:
			case SSMSwitchCode.Accelerator_Switch:
				return SSMParameterCode.Switch_P0x121;
				
			default:
				throw new ArgumentOutOfRangeException("Switch code is out of range");
			}
		}

        private void set_numeric_content_table()
        {
            _ssm_numeric_content_table.Add(SSMParameterCode.Engine_Load, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x07 }, x => 100.0 / 255.0 * x + 0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Coolant_Temperature, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x08 }, x => 1.0 * x - 40.0, "C"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Correction_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x09 }, x => 1 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Learning_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x0A }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Correction_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x0B }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Learning_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x0C }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Manifold_Absolute_Pressure, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x0D }, x => 37.0 / 255.0 * x + 0.0, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Engine_Speed, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x0E, 0x00, 0x00, 0x0F }, x => 1.0 / 4.0 * x + 0.0, "rpm"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Vehicle_Speed, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x10 }, x => 1.0 * x + 0.0, "km/h"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Ignition_Timing, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x11 }, x => 1.0 / 2.0 * x - 128.0 / 2.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_Air_Temperature, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x12 }, x => 1.0 * x - 40.0, "C"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Mass_Air_Flow, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x13, 0x00, 0x00, 0x14 }, x => 1.0 / 100.0 * x + 0, "g/s"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Throttle_Opening_Angle, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x15 }, x => 100.0 / 255.0 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Front_O2_Sensor_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x16, 0x00, 0x00, 0x17 }, x => 0.005 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Rear_O2_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x18, 0x00, 0x00, 0x19 }, x => 0.005 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Front_O2_Sensor_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x1A, 0x00, 0x00, 0x1B }, x => 0.005 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Battery_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x1C }, x => 0.08 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Flow_Sensor_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x1D }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Throttle_Sensor_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x1E }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Differential_Pressure_Sensor_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x1F }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Injection_1_Pulse_Width, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x20 }, x => 0.256 * x + 0.0, "ms"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Injection_2_Pulse_Width, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x21 }, x => 0.256 * x + 0.0, "ms"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Knock_Correction, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x22 }, x => 1.0 / 2.0 * x - 128.0 / 2.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Atmospheric_Pressure, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x23 }, x => 37.0 / 255.0 * x + 0.0, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Manifold_Relative_Pressure, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x24 }, x => 37.0 / 255.0 * x - 37.0 * 128.0 / 255.0, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Pressure_Differential_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x25 }, x => 37.0 / 255.0 * x - 37.0 * 128.0 / 255.0, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Tank_Pressure, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x26 }, x => 0.0035 * x - 128.0 * 0.0035, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.CO_Adjustment, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x27 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Learned_Ignition_Timing, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x28 }, x => 1.0 / 2.0 * x - 128.0 / 2.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Accelerator_Opening_Angle, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x29 }, x => 1.0 / 2.56 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Temperature, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x2A }, x => 1.0 * x - 40.0, "C"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Front_O2_Heater_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x2B }, x => 10.04 / 256.0 * x + 0.0, "A"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Rear_O2_Heater_Current, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x2C }, x => 10.04 / 256.0 * x + 0.0, "A"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Front_O2_Heater_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x2D }, x => 10.04 / 256.0 * x + 0.0, "A"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Level, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x2E }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Primary_Wastegate_Duty_Cycle, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x30 }, x => 100.0 / 255.0 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Secondary_Wastegate_Duty_Cycle, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x31 }, x => 100.0 / 255.0 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.CPC_Valve_Duty_Ratio, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x32 }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Tumble_Valve_Position_Sensor_Right, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x33 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Tumble_Valve_Position_Sensor_Left, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x34 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Idle_Speed_Control_Valve_Duty_Ratio, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x35 }, x => 1.0 / 2.0 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Lean_Correction, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x36 }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Heater_Duty, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x37 }, x => 1.0 / 2.55 * x + 0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Idle_Speed_Control_Valve_Step, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x38 }, x => 1.0 * x + 0.0, "steps"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Number_of_Ex_Gas_Recirc_Steps, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x39 }, x => 1.0 * x + 0.0, "steps"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Alternator_Duty, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3A }, x => 1.0 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Pump_Duty, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3B }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_VVT_Advance_Angle_Right, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3C }, x => 1.0 * x - 50.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_VVT_Advance_Angle_Left, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3D }, x => 1.0 * x - 50.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_OCV_Duty_Right, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3E }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_OCV_Duty_Left, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x3F }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_OCV_Current_Right, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x40 }, x => 32.0 * x + 0.0, "mA"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Intake_OCV_Current_Left, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x41 }, x => 32.0 * x + 0.0, "mA"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_1_Current, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x42 }, x => 0.125 * x - 128 * 0.125, "mA"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_2_Current, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x43 }, x => 0.125 * x - 128 * 0.125, "mA"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_1_Resistance, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x44 }, x => 1.0 * x + 0.0, "ohm"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_2_Resistance, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x45 }, x => 1.0 * x + 0.0, "ohm"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x46 }, x => 1.0 / 128.0 * x + 0.0, "Lambda"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Sensor_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x47 }, x => 1.0 / 128.0 * x + 0.0, "Lambda"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Gear_Position, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x4A }, x => 1.0 * x + 1.0, "position"));
            _ssm_numeric_content_table.Add(SSMParameterCode.A_F_Sensor_1_Heater_Current, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x53 }, x => 1.0 / 10.0 * x + 0.0, "A"));
            _ssm_numeric_content_table.Add(SSMParameterCode.A_F_Sensor_2_Heater_Current, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x54 }, x => 1.0 / 10.0 * x + 0.0, "A"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Roughness_Monitor_Cylinder_1, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xCE }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Roughness_Monitor_Cylinder_2, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xCF }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Correction_3, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD0 }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Learning_3, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD1 }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Rear_O2_Heater_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD2 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Air_Fuel_Adjustment_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD3 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Roughness_Monitor_Cylinder_3, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD8 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Roughness_Monitor_Cylinder_4, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xD9 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Throttle_Motor_Duty, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xFA }, x => 1.0 / 1.28 * x - 128.0 / 1.28, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Throttle_Motor_Voltage, new SSMNumericContent(new byte[] { 0x00, 0x00, 0xFB }, x => 0.08 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Sub_Throttle_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x00 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Main_Throttle_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x01 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Sub_Accelerator_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x02 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Main_Accelerator_Sensor, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x03 }, x => 0.02 * x + 0.0, "V"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Brake_Booster_Pressure, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x04 }, x => 37.0 / 255.0 * x + 0.0, "psig"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Fuel_Pressure_High, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x05 }, x => 0.04 * x + 0.0, "MPa"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_Gas_Temperature, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x06 }, x => 5.0 * x + 5.0 * 40.0, "C"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Cold_Start_Injector, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x08 }, x => 0.256 * x + 0.0, "ms"));
            _ssm_numeric_content_table.Add(SSMParameterCode.SCV_Step, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x09 }, x => 1.0 * x + 0.0, "step"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Memorised_Cruise_Speed, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x0A }, x => 1.0 * x + 0.0, "km/h"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_VVT_Advance_Angle_Right, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x18 }, x => 1.0 * x - 50.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_VVT_Advance_Angle_Left, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x19 }, x => 1.0 * x - 50.0, "deg"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_OCV_Duty_Right, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x1A }, x => 1.0 / 2.55 * x + 0.0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_OCV_Duty_Left, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x1B }, x => 1.0 / 2.55 * x + 0, "%"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_OCV_Current_Right, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x1C }, x => 32.0 * x + 0.0, "mA"));
            _ssm_numeric_content_table.Add(SSMParameterCode.Exhaust_OCV_Current_Left, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x1D }, x => 32.0 * x + 0.0, "mA"));


            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x061, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x61 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x062, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x62 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x063, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x63 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x064, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x64 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x065, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x65 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x066, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x66 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x067, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x67 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x068, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x68 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x069, new SSMNumericContent(new byte[] { 0x00, 0x00, 0x69 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x120, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x20 }, x => 1.0 * x + 0.0, ""));
            _ssm_numeric_content_table.Add(SSMParameterCode.Switch_P0x121, new SSMNumericContent(new byte[] { 0x00, 0x01, 0x21 }, x => 1.0 * x + 0.0, ""));
        }

        private void set_swicth_content_table()
        {
            SSMNumericContent master_content;

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x061];
            _ssm_switch_content_table.Add(SSMSwitchCode.AT_Vehicle_ID,new SSMSwitchContent(master_content,6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Test_Mode_Connector, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Read_Memory_Connector, new SSMSwitchContent(master_content, 4));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x062];
            _ssm_switch_content_table.Add(SSMSwitchCode.Neutral_Position_Switch, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Idle_Switch, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Intercooler_AutoWash_Switch, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Ignition_Switch, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Power_Steering_Switch, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.Air_Conditioning_Switch, new SSMSwitchContent(master_content, 1));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x063];
            _ssm_switch_content_table.Add(SSMSwitchCode.Handle_Switch,            new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Starter_Switch,           new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Front_O2_Rich_Signal,     new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Rear_O2_Rich_Signal,      new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Front_O2_2_Rich_Signal,   new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Knock_Signal_1,           new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.Knock_Signal_2,           new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.Electrical_Load_Signal,   new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x064];
            _ssm_switch_content_table.Add(SSMSwitchCode.Crank_Position_Sensor, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Cam_Position_Sensor, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Defogger_Switch, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Blower_Switch, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Interior_Light_Switch, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Wiper_Switch, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.AirCon_Lock_Signal, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.AirCon_Mid_Pressure_Switch, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x065];
            _ssm_switch_content_table.Add(SSMSwitchCode.AirCon_Compressor_Signal, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Radiator_Fan_Relay_3, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Radiator_Fan_Relay_1, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Radiator_Fan_Relay_2, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Fuel_Pump_Relay, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Intercooler_AutoWash_Relay, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.CPC_Solenoid_Valve, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.BlowBy_Leak_Connector, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x066];
            _ssm_switch_content_table.Add(SSMSwitchCode.PCV_Solenoid_Valve, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.TGV_Output, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.TGV_Drive, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Variable_Intake_Air_Solenoid, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Pressure_Sources_Change, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Vent_Solenoid_Valve, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.P_S_Solenoid_Valve, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.Assist_Air_Solenoid_Valve, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x067];
            _ssm_switch_content_table.Add(SSMSwitchCode.Tank_Sensor_Control_Valve, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Relief_Valve_Solenoid_1, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Relief_Valve_Solenoid_2, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.TCS_Relief_Valve_Solenoid, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Ex_Gas_Positive_Pressure, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Ex_Gas_Negative_Pressure, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.Intake_Air_Solenoid, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.Muffler_Control, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x068];
            _ssm_switch_content_table.Add(SSMSwitchCode.Retard_Signal_from_AT, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Fuel_Cut_Signal_from_AT, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.Ban_of_Torque_Down, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.Request_Torque_Down_VDC, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x069];
            _ssm_switch_content_table.Add(SSMSwitchCode.Torque_Control_Signal_1, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Torque_Control_Signal_2, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Torque_Permission_Signal, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.EAM_Signal, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.AT_coop_lock_up_signal, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.AT_coop_lean_burn_signal, new SSMSwitchContent(master_content, 2));
            _ssm_switch_content_table.Add(SSMSwitchCode.AT_coop_rich_spike_signal, new SSMSwitchContent(master_content, 1));
            _ssm_switch_content_table.Add(SSMSwitchCode.AET_Signal, new SSMSwitchContent(master_content, 0));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x120];
            _ssm_switch_content_table.Add(SSMSwitchCode.ETC_Motor_Relay, new SSMSwitchContent(master_content, 6));

            master_content = _ssm_numeric_content_table[SSMParameterCode.Switch_P0x121];
            _ssm_switch_content_table.Add(SSMSwitchCode.Clutch_Switch, new SSMSwitchContent(master_content, 7));
            _ssm_switch_content_table.Add(SSMSwitchCode.Stop_Light_Switch, new SSMSwitchContent(master_content, 6));
            _ssm_switch_content_table.Add(SSMSwitchCode.Set_Coast_Switch, new SSMSwitchContent(master_content, 5));
            _ssm_switch_content_table.Add(SSMSwitchCode.Rsume_Accelerate_Switch, new SSMSwitchContent(master_content, 4));
            _ssm_switch_content_table.Add(SSMSwitchCode.Brake_Switch, new SSMSwitchContent(master_content, 3));
            _ssm_switch_content_table.Add(SSMSwitchCode.Accelerator_Switch, new SSMSwitchContent(master_content, 1));
        }


    }

    public class SSMNumericContent : NumericContent
    {
        private byte[] _read_address;
        private bool _slow_read_enable;
        private bool _fast_read_enable;

        public SSMNumericContent(byte[] address, Func<Int32, double> conversion_function, String unit)
        {
            _read_address = address;
            _conversion_function = conversion_function;
            _unit = unit;

            //デフォルトでは無効。明示的にEnableされない限り通信をしない設定とする。
            _slow_read_enable = false;
            _fast_read_enable = false;
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

    public class SSMSwitchContent
    {
        private SSMNumericContent _master_content;
        private int _bit_index;

        public SSMSwitchContent( SSMNumericContent master_content, int bit_index)
        {
            _master_content = master_content;
            _bit_index = bit_index;
        }

        public bool Value
        {
            get
            {
                return (_master_content.RawValue & 0x01<<_bit_index)>>_bit_index == 1; 
            }
        }
    }
}
