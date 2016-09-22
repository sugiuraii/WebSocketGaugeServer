using System;
using System.Collections.Generic;

namespace DefiSSMCOM
{
    public class ArduinoContentTable : ContentTableCommon<ArduinoParameterCode, ArduinoNumericContent>
    {
        public const double ADC_STEP = 4096;
        public const double ADC_REF_VOLTAGE = 5;

        // Water/Oil temp sensor Constants
        public const double THERMISTOR_R0 = 10000; //Thermistor resistance at 25degC
        public const double THERMISTOR_B = 3389;
        // Thermistor current sense reisistance (parallel to thermistor)
        public const double THERMISTOR_SENSE_R = 5000;

        //コンストラクタ
        public ArduinoContentTable()
        {
            NumPulsePerRev = 2;
            NumPulsePerSpd = 4;
            WaterTempThermistorSerialResistance = 5000;
            OilTempThermistorSerialResistance = 5000;
        }

        public int NumPulsePerRev { get; set; }
        public int NumPulsePerSpd { get; set; }
        public double WaterTempThermistorSerialResistance { get; set; }
        public double OilTempThermistorSerialResistance { get; set; }

        protected override void set_numeric_content_table()
        {
            _numeric_content_table.Add(ArduinoParameterCode.Tacho, new ArduinoNumericContent('T', tpulse => 
            {
                if(tpulse > 0)
                    return 60/(NumPulsePerRev*tpulse * 1e-6);
                else
                    return 0;
            }, "rpm"));

            _numeric_content_table.Add(ArduinoParameterCode.Speed, new ArduinoNumericContent('S', tpulse =>
            {
                if (tpulse > 0)
                    return 3600 / (637 * tpulse * NumPulsePerRev * 1e-6);
                else
                    return 0;
            }, "km/h"));

            _numeric_content_table.Add(ArduinoParameterCode.Boost, new ArduinoNumericContent('A', adc_out => 73.47 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP - 1.88), "kPa"));
            _numeric_content_table.Add(ArduinoParameterCode.Water_Temp, new ArduinoNumericContent('B', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B/(Math.Log(R/THERMISTOR_R0)+THERMISTOR_B/298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Temp, new ArduinoNumericContent('C', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Temp2, new ArduinoNumericContent('D', adc_out =>
            {
                double R = adc_out * THERMISTOR_SENSE_R / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Pres, new ArduinoNumericContent('E', adc_out => 250 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP - 0.48), "kPa"));
            _numeric_content_table.Add(ArduinoParameterCode.Fuel_Pres, new ArduinoNumericContent('F', adc_out => 250 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP - 0.48), "kPa"));
        }
    }

    public class ArduinoNumericContent : NumericContent
    {
        private char _header_char;

        public ArduinoNumericContent(char header_char, Func <Int32, double> conversion_function, String unit)
        {
            _header_char = header_char;
            _conversion_function = conversion_function;
            _unit = unit;
        }

        public char Header_char
        {
            get
            {
                return _header_char;
            }
        }
    };

    public enum ArduinoParameterCode
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
