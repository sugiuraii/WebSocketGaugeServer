using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM.OBDII
{
    public class OBDIINumericContent : NumericContent
    {
        private byte _pid;
        private int _returnByteLength;

        public OBDIINumericContent(byte pid, int returnByteLength, Func<double, double> conversion_function, String unit)
        {
            _pid = pid;
            _returnByteLength = returnByteLength;
            _conversion_function = conversion_function;
            _unit = unit;

            //デフォルトでは無効。明示的にEnableされない限り通信をしない設定とする。
            SlowReadEnable = false;
            FastReadEnable = false;
        }

        public byte PID { get { return _pid; } }
        public int ReturnByteLength { get { return _returnByteLength; } } 

        public bool SlowReadEnable { get; set; }
        public bool FastReadEnable { get; set; }

    }

    public class OBDIIContentTable : ContentTableCommon<OBDIIParameterCode, OBDIINumericContent>
    {
        public OBDIIContentTable()
        {
        }

        public void setAllDisable()
        {
            foreach (KeyValuePair<OBDIIParameterCode, OBDIINumericContent> pair in _numeric_content_table)
            {
                pair.Value.SlowReadEnable = false;
                pair.Value.FastReadEnable = false;
            }
        }

        protected override void setNumericContentTable()
        {
            _numeric_content_table.Add(OBDIIParameterCode.Engine_Load , new OBDIINumericContent(0x04 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Coolant_Temperature , new OBDIINumericContent(0x05 , 1 , A=> A-40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Air_Fuel_Correction_1 , new OBDIINumericContent(0x06 , 1 , A=> (A-128) * 100/128 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Air_Fuel_Learning_1 , new OBDIINumericContent(0x07 , 1 , A=> (A-128) * 100/128 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Air_Fuel_Correction_2 , new OBDIINumericContent(0x08 , 1 , A=> (A-128) * 100/128 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Air_Fuel_Learning_2 , new OBDIINumericContent(0x09 , 1 , A=> (A-128) * 100/128 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Fuel_Tank_Pressure , new OBDIINumericContent(0x0A , 1 , A=> A*3 ,"kPa"));
            _numeric_content_table.Add(OBDIIParameterCode.Manifold_Absolute_Pressure , new OBDIINumericContent(0x0B , 1 , A=> A ,"kPa"));
            _numeric_content_table.Add(OBDIIParameterCode.Engine_Speed , new OBDIINumericContent(0x0C , 2 , A=> A/4 ,"rpm"));
            _numeric_content_table.Add(OBDIIParameterCode.Vehicle_Speed , new OBDIINumericContent(0x0D , 1 , A=> A ,"km/h"));
            _numeric_content_table.Add(OBDIIParameterCode.Ignition_Timing, new OBDIINumericContent(0x0E , 1 , A=> A/2 - 64 ,"deg"));
            _numeric_content_table.Add(OBDIIParameterCode.Intake_Air_Temperature , new OBDIINumericContent(0x0F , 1 , A=> A-40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Mass_Air_Flow , new OBDIINumericContent(0x10 , 2 , A=> A / 100 ,"g/s"));
            _numeric_content_table.Add(OBDIIParameterCode.Throttle_Opening_Angle , new OBDIINumericContent(0x11 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Run_time_since_engine_start , new OBDIINumericContent(0x1F , 2 , A=> A ,"seconds"));
            _numeric_content_table.Add(OBDIIParameterCode.Distance_traveled_with_MIL_on , new OBDIINumericContent(0x21 , 2 , A=> A ,"km"));
            _numeric_content_table.Add(OBDIIParameterCode.Fuel_Rail_Pressure , new OBDIINumericContent(0x22 , 2 , A=> (A * 10) / 128 ,"kPa"));
            _numeric_content_table.Add(OBDIIParameterCode.Fuel_Rail_Pressure_diesel , new OBDIINumericContent(0x23 , 2 , A=> A * 10 ,"kPa"));
            _numeric_content_table.Add(OBDIIParameterCode.Commanded_EGR , new OBDIINumericContent(0x2C , 1 , A=> 100*A/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.EGR_Error , new OBDIINumericContent(0x2D , 1 , A=> (A-128) * 100/128 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Commanded_evaporative_purge , new OBDIINumericContent(0x2E , 1 , A=> 100*A/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Fuel_Level_Input , new OBDIINumericContent(0x2F , 1 , A=> 100*A/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Number_of_warmups_since_codes_cleared , new OBDIINumericContent(0x30 , 1 , A=> A ,"N/A"));
            _numeric_content_table.Add(OBDIIParameterCode.Distance_traveled_since_codes_cleared , new OBDIINumericContent(0x31 , 2 , A=> A ,"km"));
            _numeric_content_table.Add(OBDIIParameterCode.Evap_System_Vapor_Pressure , new OBDIINumericContent(0x32 , 2 , A=> A/4 ,"Pa"));
            _numeric_content_table.Add(OBDIIParameterCode.Atmospheric_Pressure , new OBDIINumericContent(0x33 , 1 , A=> A ,"kPa "));
            _numeric_content_table.Add(OBDIIParameterCode.Catalyst_TemperatureBank_1_Sensor_1 , new OBDIINumericContent(0x3C , 2 , A=> A/10 - 40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Catalyst_TemperatureBank_2_Sensor_1 , new OBDIINumericContent(0x3D , 2 , A=> A/10 - 40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Catalyst_TemperatureBank_1_Sensor_2 , new OBDIINumericContent(0x3E , 2 , A=> A/10 - 40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Catalyst_TemperatureBank_2_Sensor_2 , new OBDIINumericContent(0x3F , 2 , A=> A/10 - 40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Battery_Voltage , new OBDIINumericContent(0x42 , 2 , A=> A/1000 ,"V"));
            _numeric_content_table.Add(OBDIIParameterCode.Absolute_load_value , new OBDIINumericContent(0x43 , 2 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Command_equivalence_ratio , new OBDIINumericContent(0x44 , 2 , A=> A/32768 ,"N/A"));
            _numeric_content_table.Add(OBDIIParameterCode.Relative_throttle_position , new OBDIINumericContent(0x45 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Ambient_air_temperature , new OBDIINumericContent(0x46 , 1 , A=> A-40 ,"degC"));
            _numeric_content_table.Add(OBDIIParameterCode.Absolute_throttle_position_B , new OBDIINumericContent(0x47 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Absolute_throttle_position_C , new OBDIINumericContent(0x48 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Accelerator_pedal_position_D , new OBDIINumericContent(0x49 , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Accelerator_pedal_position_E , new OBDIINumericContent(0x4A , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Accelerator_pedal_position_F , new OBDIINumericContent(0x4B , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Commanded_throttle_actuator , new OBDIINumericContent(0x4C , 1 , A=> A*100/255 ,"%"));
            _numeric_content_table.Add(OBDIIParameterCode.Time_run_with_MIL_on , new OBDIINumericContent(0x4D , 2 , A=> A ,"minutes"));
            _numeric_content_table.Add(OBDIIParameterCode.Time_since_trouble_codes_cleared , new OBDIINumericContent(0x4E , 2 , A=> A ,"minutes"));
            _numeric_content_table.Add(OBDIIParameterCode.Ethanol_fuel_percent , new OBDIINumericContent(0x52 , 1 , A=> A*100/255 ,"%"));
        }
    }
}
