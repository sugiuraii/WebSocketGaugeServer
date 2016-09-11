using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DefiSSMCOM
{
    public class Arduino_Content_Table
    {
        private Dictionary<Arduino_Parameter_Code, Arduino_Numeric_Content> _arduino_numeric_content_table = new Dictionary<Arduino_Parameter_Code, Arduino_Numeric_Content>();
        public const int ADC_STEP = 4096;
        public const double ADC_REF_VOLTAGE = 5;

        // Water/Oil temp sensor Constants
        public const double THERMISTOR_R0 = 10000; //Thermistor resistance at 25degC
        public const double THERMISTOR_B = 3389;
        // Thermistor current sense reisistance (parallel to thermistor)
        public const double THERMISTOR_SENSE_R = 5000;

        //コンストラクタ
        public Arduino_Content_Table()
        {
            set_numeric_content_table();
            NumPulsePerRev = 2;
            NumPulsePerSpd = 4;
            WaterTempThermistorSerialResistance = 5000;
            OilTempThermistorSerialResistance = 5000;
        }

        public Arduino_Numeric_Content this[Arduino_Parameter_Code code]
        {
            get
            {
                return _arduino_numeric_content_table[code];
            }
        }

        public int NumPulsePerRev { get; set; }
        public int NumPulsePerSpd { get; set; }
        public double WaterTempThermistorSerialResistance { get; set; }
        public double OilTempThermistorSerialResistance { get; set; }

        private void set_numeric_content_table()
        {
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Tacho, new Arduino_Numeric_Content('T', tpulse => 
            {
                if(tpulse > 0)
                    return 60/(NumPulsePerRev*tpulse);
                else
                    return 0;
            }, "rpm"));

            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Speed, new Arduino_Numeric_Content('S', tpulse =>
            {
                if (tpulse > 0)
                    return 3600 / (637 * tpulse * NumPulsePerRev);
                else
                    return 0;
            }, "km/h"));

            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Boost, new Arduino_Numeric_Content('A', adc_out => 73.47*(adc_out/ADC_STEP*ADC_REF_VOLTAGE - 1.88), "kPa"));
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Water_Temp, new Arduino_Numeric_Content('B', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B/(Math.Log(R/THERMISTOR_R0)+THERMISTOR_B/298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Oil_Temp, new Arduino_Numeric_Content('C', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Oil_Temp2, new Arduino_Numeric_Content('D', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Oil_Pres, new Arduino_Numeric_Content('E', adc_out => 250 * (adc_out / ADC_STEP * ADC_REF_VOLTAGE - 0.48), "kPa"));
            _arduino_numeric_content_table.Add(Arduino_Parameter_Code.Fuel_Pres, new Arduino_Numeric_Content('F', adc_out => 250 * (adc_out / ADC_STEP * ADC_REF_VOLTAGE - 0.48), "kPa"));
        }
    }

    public class Arduino_Numeric_Content
    {
        private char _header_char;
        private Func<int, double> _conversion_function;
        private int _raw_value;
        private String _unit;

        public Arduino_Numeric_Content(char header_char, Func <int, double> conversion_function, String unit)
        {
            _header_char = header_char;
            _conversion_function = conversion_function;
            _unit = unit;
        }

        public double Value
        {
            get
            {
                return _conversion_function(_raw_value);
            }
        }

        public char Header_char
        {
            get
            {
                return _header_char;
            }
        }

        public int Raw_Value
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

        public Func<int, double> Conversion_Fuction
        {
            get
            {
                return _conversion_function;
            }
        }

        public String Unit
        {
            get
            {
                return _unit;
            }
        }

    };



    public enum Arduino_Parameter_Code
    {
        Tacho,
        Speed,

        Boost,
        Water_Temp,
        Oil_Temp,
        Oil_Temp2,
        Oil_Pres,
        Fuel_Pres,
    };
}
