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
        private Dictionary<byte,OBDIIParameterCode> pidToParameterCodeTable = new Dictionary<byte,OBDIIParameterCode>();
        
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

        private void addContentTable(OBDIIParameterCode code, byte pid, int returnByteLength, Func<double, double> conversionFunction, String unit)
        {
            _numeric_content_table.Add(code, new OBDIINumericContent(pid, returnByteLength, conversionFunction, unit));
            pidToParameterCodeTable.Add(pid, code);
        }

        public OBDIIParameterCode getParameterCodeFromPID(byte pid)
        {
            return pidToParameterCodeTable[pid];
        }

        protected override void setNumericContentTable()
        {
            addContentTable(OBDIIParameterCode.Engine_Load , 0x04 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Coolant_Temperature , 0x05 , 1 , A=> A-40 ,"degC");
            addContentTable(OBDIIParameterCode.Air_Fuel_Correction_1 , 0x06 , 1 , A=> (A-128) * 100/128 ,"%");
            addContentTable(OBDIIParameterCode.Air_Fuel_Learning_1 , 0x07 , 1 , A=> (A-128) * 100/128 ,"%");
            addContentTable(OBDIIParameterCode.Air_Fuel_Correction_2 , 0x08 , 1 , A=> (A-128) * 100/128 ,"%");
            addContentTable(OBDIIParameterCode.Air_Fuel_Learning_2 , 0x09 , 1 , A=> (A-128) * 100/128 ,"%");
            addContentTable(OBDIIParameterCode.Fuel_Tank_Pressure , 0x0A , 1 , A=> A*3 ,"kPa");
            addContentTable(OBDIIParameterCode.Manifold_Absolute_Pressure , 0x0B , 1 , A=> A ,"kPa");
            addContentTable(OBDIIParameterCode.Engine_Speed , 0x0C , 2 , A=> A/4 ,"rpm");
            addContentTable(OBDIIParameterCode.Vehicle_Speed , 0x0D , 1 , A=> A ,"km/h");
            addContentTable(OBDIIParameterCode.Ignition_Timing, 0x0E , 1 , A=> A/2 - 64 ,"deg");
            addContentTable(OBDIIParameterCode.Intake_Air_Temperature , 0x0F , 1 , A=> A-40 ,"degC");
            addContentTable(OBDIIParameterCode.Mass_Air_Flow , 0x10 , 2 , A=> A / 100 ,"g/s");
            addContentTable(OBDIIParameterCode.Throttle_Opening_Angle , 0x11 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Run_time_since_engine_start , 0x1F , 2 , A=> A ,"seconds");
            addContentTable(OBDIIParameterCode.Distance_traveled_with_MIL_on , 0x21 , 2 , A=> A ,"km");
            addContentTable(OBDIIParameterCode.Fuel_Rail_Pressure , 0x22 , 2 , A=> (A * 10) / 128 ,"kPa");
            addContentTable(OBDIIParameterCode.Fuel_Rail_Pressure_diesel , 0x23 , 2 , A=> A * 10 ,"kPa");
            addContentTable(OBDIIParameterCode.Commanded_EGR , 0x2C , 1 , A=> 100*A/255 ,"%");
            addContentTable(OBDIIParameterCode.EGR_Error , 0x2D , 1 , A=> (A-128) * 100/128 ,"%");
            addContentTable(OBDIIParameterCode.Commanded_evaporative_purge , 0x2E , 1 , A=> 100*A/255 ,"%");
            addContentTable(OBDIIParameterCode.Fuel_Level_Input , 0x2F , 1 , A=> 100*A/255 ,"%");
            addContentTable(OBDIIParameterCode.Number_of_warmups_since_codes_cleared , 0x30 , 1 , A=> A ,"N/A");
            addContentTable(OBDIIParameterCode.Distance_traveled_since_codes_cleared , 0x31 , 2 , A=> A ,"km");
            addContentTable(OBDIIParameterCode.Evap_System_Vapor_Pressure , 0x32 , 2 , A=> A/4 ,"Pa");
            addContentTable(OBDIIParameterCode.Atmospheric_Pressure , 0x33 , 1 , A=> A ,"kPa ");
            addContentTable(OBDIIParameterCode.Catalyst_TemperatureBank_1_Sensor_1 , 0x3C , 2 , A=> A/10 - 40 ,"degC");
            addContentTable(OBDIIParameterCode.Catalyst_TemperatureBank_2_Sensor_1 , 0x3D , 2 , A=> A/10 - 40 ,"degC");
            addContentTable(OBDIIParameterCode.Catalyst_TemperatureBank_1_Sensor_2 , 0x3E , 2 , A=> A/10 - 40 ,"degC");
            addContentTable(OBDIIParameterCode.Catalyst_TemperatureBank_2_Sensor_2 , 0x3F , 2 , A=> A/10 - 40 ,"degC");
            addContentTable(OBDIIParameterCode.Battery_Voltage , 0x42 , 2 , A=> A/1000 ,"V");
            addContentTable(OBDIIParameterCode.Absolute_load_value , 0x43 , 2 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Command_equivalence_ratio , 0x44 , 2 , A=> A/32768 ,"N/A");
            addContentTable(OBDIIParameterCode.Relative_throttle_position , 0x45 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Ambient_air_temperature , 0x46 , 1 , A=> A-40 ,"degC");
            addContentTable(OBDIIParameterCode.Absolute_throttle_position_B , 0x47 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Absolute_throttle_position_C , 0x48 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Accelerator_pedal_position_D , 0x49 , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Accelerator_pedal_position_E , 0x4A , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Accelerator_pedal_position_F , 0x4B , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Commanded_throttle_actuator , 0x4C , 1 , A=> A*100/255 ,"%");
            addContentTable(OBDIIParameterCode.Time_run_with_MIL_on , 0x4D , 2 , A=> A ,"minutes");
            addContentTable(OBDIIParameterCode.Time_since_trouble_codes_cleared , 0x4E , 2 , A=> A ,"minutes");
            addContentTable(OBDIIParameterCode.Ethanol_fuel_percent , 0x52 , 1 , A=> A*100/255 ,"%");
        }
    }
}
