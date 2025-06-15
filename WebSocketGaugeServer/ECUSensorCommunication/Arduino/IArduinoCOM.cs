using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public interface IArduinoCOM : IBackgroundCommunicate
    {
        event EventHandler ArduinoPacketReceived;
        double GetValue(ArduinoParameterCode code);
        // Not used -> Delete
        // UInt32 get_raw_value(ArduinoParameterCode code); 
        string GetUnit(ArduinoParameterCode code);
    }
}