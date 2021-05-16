using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public interface IArduinoCOM
    {
        event EventHandler ArduinoPacketReceived;
        double get_value(ArduinoParameterCode code);
        UInt32 get_raw_value(ArduinoParameterCode code);
        string get_unit(ArduinoParameterCode code);
        void BackgroundCommunicateStart();
        void BackGroundCommunicateStop();
    }
}