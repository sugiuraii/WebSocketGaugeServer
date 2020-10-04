using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class ArduinoContentTable : ContentTableCommon<ArduinoParameterCode, ArduinoNumericContent>
    {
        public const double ADC_STEP = 4096;
        public const double ADC_REF_VOLTAGE = 5;

        // Water/Oil temp sensor Constants
        // This parameter are derived from Autogauge water/oil temp sensor (9BTP000 3747-SENSOR)
        public const double THERMISTOR_R0 = 1306; //Thermistor resistance at 25degC
        public const double THERMISTOR_B = 3529;
        // Thermistor current sense reisistance (inserted in series with the thermistor)
        public const double THERMISTOR_SENSE_R = 100;

        //Constructor
        public ArduinoContentTable()
        {
            NumPulsePerRev = 2;
            NumPulsePerSpd = 4;
            WaterTempThermistorSerialResistance = THERMISTOR_SENSE_R;
            OilTempThermistorSerialResistance = THERMISTOR_SENSE_R;
        }

        public int NumPulsePerRev { get; set; }
        public int NumPulsePerSpd { get; set; }
        public double WaterTempThermistorSerialResistance { get; set; }
        public double OilTempThermistorSerialResistance { get; set; }

        protected override void setNumericContentTable()
        {
            _numeric_content_table.Add(ArduinoParameterCode.Engine_Speed, new ArduinoNumericContent('T', tpulse => 
            {
                if(tpulse > 0)
                    return 60/(NumPulsePerRev*tpulse * 1e-6);
                else
                    return 0;
            }, "rpm"));

            _numeric_content_table.Add(ArduinoParameterCode.Vehicle_Speed, new ArduinoNumericContent('S', tpulse =>
            {
                if (tpulse > 0)
                    return 3600 / (637 * tpulse * NumPulsePerSpd * 1e-6);
                else
                    return 0;
            }, "km/h"));

            //Boost conversion for autogauge boost sensor.
            _numeric_content_table.Add(ArduinoParameterCode.Manifold_Absolute_Pressure, new ArduinoNumericContent('A', adc_out => 100 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP), "kPa"));
            // (To use boost sensor of defi instead of autogauge, please use following conversion function)
            // _numeric_content_table.Add(ArduinoParameterCode.Manifold_Absolute_Pressure, new ArduinoNumericContent('A', adc_out => 73.47 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP), "kPa"));
            
            _numeric_content_table.Add(ArduinoParameterCode.Coolant_Temperature, new ArduinoNumericContent('B', adc_out =>
            {
                double R = adc_out * WaterTempThermistorSerialResistance / (ADC_STEP - adc_out);
                double T = THERMISTOR_B/(Math.Log(R/THERMISTOR_R0)+THERMISTOR_B/298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Temperature, new ArduinoNumericContent('C', adc_out =>
            {
                double R = adc_out * OilTempThermistorSerialResistance / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Temperature2, new ArduinoNumericContent('D', adc_out =>
            {
                double R = adc_out * OilTempThermistorSerialResistance / (ADC_STEP - adc_out);
                double T = THERMISTOR_B / (Math.Log(R / THERMISTOR_R0) + THERMISTOR_B / 298.15);
                double Tdeg = T - 273.15;
                return Tdeg;
            }, "degC"));
            _numeric_content_table.Add(ArduinoParameterCode.Oil_Pressure, new ArduinoNumericContent('E', adc_out => 250 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP - 0.48) * 0.0101972, "kgf/cm2"));
            _numeric_content_table.Add(ArduinoParameterCode.Fuel_Rail_Pressure, new ArduinoNumericContent('F', adc_out => 250 * (adc_out * ADC_REF_VOLTAGE / ADC_STEP - 0.48) * 0.0101972, "kgf/cm2"));
        }
    }

    public class ArduinoNumericContent : NumericContent
    {
        private char _header_char;

        public ArduinoNumericContent(char header_char, Func <double, double> conversion_function, String unit)
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
        Engine_Speed,
        Vehicle_Speed,

        Manifold_Absolute_Pressure,
        Coolant_Temperature,
        Oil_Temperature,
        Oil_Temperature2,
        Oil_Pressure,
        Fuel_Rail_Pressure
    };
}
