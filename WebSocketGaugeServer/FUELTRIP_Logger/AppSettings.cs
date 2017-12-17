using System;
using System.Collections.Generic;
using DefiSSMCOM.Defi;
using DefiSSMCOM.SSM;
using DefiSSMCOM.Arduino;
using DefiSSMCOM.OBDII;

namespace FUELTRIP_Logger
{
    /// <summary>
    /// Application setting class.
    /// </summary>
    public class AppSettings
    {
        public string defiserver_url;
        public string ssmserver_url;
        public string arduinoserver_url;
        public string elm327server_url;
        public int websocket_port;
        public int keepalive_interval;
        public calculation Calculation;

        public class calculation
        {
            public FuelCalculationMethod FuelCalculationMethod;
            public DataSource DataSource;
            public FuelTripCalculatorOption CalculationOption;
        }

        public class DataSource
        {
            public WebSocketType VehicleSpeedSource;
            public WebSocketType RPMSource;
            public WebSocketType InjectionPWSource;
            public WebSocketType MassAirFlowSource;
            public WebSocketType AFRatioSource;
            public WebSocketType FuelRateSource;
        }

        /// <summary>
        /// Create the list of parameter code from data source and calculation method setting.
        /// </summary>
        /// <returns>The list of required parameter code.</returns>
        public RequiredParameterCode getRequiredParameterCodes()
        {
            RequiredParameterCode requiredCodes = new RequiredParameterCode();
            FuelCalculationMethod calcMethod = this.Calculation.FuelCalculationMethod;
            DataSource dataSource = this.Calculation.DataSource;
            switch (calcMethod)
            {
                case FuelCalculationMethod.FUEL_RATE:
                    {
                        switch (dataSource.FuelRateSource)
                        {
                            case WebSocketType.ELM327:
                                requiredCodes.ELM327OBDCodes.Add(OBDIIParameterCode.Engine_fuel_rate);
                                break;
                            default:
                                throw new ArgumentException("Engine Fuel rate is supported only on ELM327.");
                        }
                        break;
                    }
                case FuelCalculationMethod.RPM_INJECTION_PW:
                    {
                        switch (dataSource.InjectionPWSource)
                        {
                            case WebSocketType.SSM:
                                requiredCodes.SSMCodes.Add(SSMParameterCode.Fuel_Injection_1_Pulse_Width);
                                break;
                            default:
                                throw new ArgumentException("Fuel injection pulse width is supported only on SSM");
                        }
                        switch (dataSource.RPMSource)
                        {
                            case WebSocketType.DEFI:
                                requiredCodes.DefiCodes.Add(DefiParameterCode.Engine_Speed);
                                break;
                            case WebSocketType.SSM:
                                requiredCodes.SSMCodes.Add(SSMParameterCode.Engine_Speed);
                                break;
                            case WebSocketType.ARDUINO:
                                requiredCodes.ArduinoCodes.Add(ArduinoParameterCode.Engine_Speed);
                                break;
                            case WebSocketType.ELM327:
                                requiredCodes.ELM327OBDCodes.Add(OBDIIParameterCode.Engine_Speed);
                                break;
                        }
                        break;
                    }
                case FuelCalculationMethod.MASS_AIR_FLOW_AF:
                    {
                        switch (dataSource.MassAirFlowSource)
                        {
                            case WebSocketType.SSM:
                                requiredCodes.SSMCodes.Add(SSMParameterCode.Mass_Air_Flow);
                                break;
                            case WebSocketType.ELM327:
                                requiredCodes.ELM327OBDCodes.Add(OBDIIParameterCode.Mass_Air_Flow);
                                break;
                            default:
                                throw new ArgumentException("Mass air flow is supported only on SSM or ELM327.");
                        }
                        switch (dataSource.AFRatioSource)
                        {
                            case WebSocketType.SSM:
                                requiredCodes.SSMCodes.Add(SSMParameterCode.Air_Fuel_Sensor_1);
                                break;
                            case WebSocketType.ELM327:
                                requiredCodes.ELM327OBDCodes.Add(OBDIIParameterCode.Command_equivalence_ratio);
                                break;
                            default:
                                throw new ArgumentException("A/F ratio is supported only on SSM or ELM327.");
                        }
                        break;
                    }
                case FuelCalculationMethod.MASS_AIR_FLOW:
                    {
                        switch (dataSource.MassAirFlowSource)
                        {
                            case WebSocketType.SSM:
                                requiredCodes.SSMCodes.Add(SSMParameterCode.Mass_Air_Flow);
                                break;
                            case WebSocketType.ELM327:
                                requiredCodes.ELM327OBDCodes.Add(OBDIIParameterCode.Mass_Air_Flow);
                                break;
                            default:
                                throw new ArgumentException("Mass air flow is supported only on SSM or ELM327.");
                        }
                        break;
                    }
            }
            return requiredCodes;
        }

        public void ValidateSettings()
        {
            DataSource datasource = this.Calculation.DataSource;
            switch (datasource.VehicleSpeedSource)
            {
                case WebSocketType.DEFI:
                    throw new ArgumentException("VehicleSpeed is not supported by " + datasource.VehicleSpeedSource.ToString());
            }

            switch (datasource.InjectionPWSource)
            {
                case WebSocketType.DEFI:
                case WebSocketType.ARDUINO:
                case WebSocketType.ELM327:
                    throw new ArgumentException("InjectionPW is not supported by " + datasource.InjectionPWSource.ToString());
            }

            switch (datasource.MassAirFlowSource)
            {
                case WebSocketType.DEFI:
                case WebSocketType.ARDUINO:
                    throw new ArgumentException("MassAirFlow is not supported by " + datasource.MassAirFlowSource.ToString());
            }

            switch (datasource.AFRatioSource)
            {
                case WebSocketType.DEFI:
                case WebSocketType.ARDUINO:
                    throw new ArgumentException("AFRatio is not supported by " + datasource.AFRatioSource.ToString());
            }

            switch (datasource.FuelRateSource)
            {
                case WebSocketType.DEFI:
                case WebSocketType.SSM:
                case WebSocketType.ARDUINO:
                    throw new ArgumentException("FuelRate is not supported by " + datasource.FuelRateSource.ToString());
            }
        }
    }

    /// <summary>
    /// Class to store required parameter code.
    /// </summary>
    public class RequiredParameterCode
    {
        public List<DefiParameterCode> DefiCodes = new List<DefiParameterCode>();
        public List<SSMParameterCode> SSMCodes = new List<SSMParameterCode>();
        public List<ArduinoParameterCode> ArduinoCodes = new List<ArduinoParameterCode>();
        public List<OBDIIParameterCode> ELM327OBDCodes = new List<OBDIIParameterCode>();
    }

    /// <summary>
    /// Type of websocket server.
    /// </summary>
    public enum WebSocketType
    {
        DEFI,
        SSM,
        ARDUINO,
        ELM327
    }

}
