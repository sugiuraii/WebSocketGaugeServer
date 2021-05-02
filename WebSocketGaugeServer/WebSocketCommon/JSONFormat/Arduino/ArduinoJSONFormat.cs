using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;

namespace SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat.Arduino
{
    public class ArduinoWSSendJSONFormat : WSSendJSONFormat<ArduinoParameterCode>
    {
        public const string ModeCode = "ARDUINO_WS_SEND";
        public ArduinoWSSendJSONFormat() : base (ModeCode)
        {
        }
    }

    public class ArduinoWSIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "ARDUINO_WS_INTERVAL";
        public ArduinoWSIntervalJSONFormat() : base (ModeCode)
        {
        }
    }
}
