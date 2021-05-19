using System;
using System.Collections.Generic;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class VirtualArduinoContentTable
    {
        private readonly Dictionary<ArduinoParameterCode, VirtualArduinoCOMContent> contentTable = new Dictionary<ArduinoParameterCode, VirtualArduinoCOMContent>();
        public VirtualArduinoCOMContent this[ArduinoParameterCode code] { get => contentTable[code]; }
        private void initializeContentTable()
        {
            contentTable.Add(ArduinoParameterCode.Engine_Speed, new VirtualArduinoCOMContent("rpm"));
            contentTable.Add(ArduinoParameterCode.Vehicle_Speed, new VirtualArduinoCOMContent("km/h"));
            contentTable.Add(ArduinoParameterCode.Manifold_Absolute_Pressure, new VirtualArduinoCOMContent("kPa"));
            contentTable.Add(ArduinoParameterCode.Coolant_Temperature, new VirtualArduinoCOMContent("degC"));
            contentTable.Add(ArduinoParameterCode.Oil_Temperature, new VirtualArduinoCOMContent("degC"));
            contentTable.Add(ArduinoParameterCode.Oil_Temperature2, new VirtualArduinoCOMContent("degC"));
            contentTable.Add(ArduinoParameterCode.Oil_Pressure, new VirtualArduinoCOMContent("kgf/cm2"));
            contentTable.Add(ArduinoParameterCode.Fuel_Rail_Pressure, new VirtualArduinoCOMContent("kgf/cm2"));
        }
    }
    public class VirtualArduinoCOMContent
    {
        public double Value;
        public readonly string Unit;
        public VirtualArduinoCOMContent(string unit)
        {
            Value = 0;
            Unit = unit;
        }
    }
}