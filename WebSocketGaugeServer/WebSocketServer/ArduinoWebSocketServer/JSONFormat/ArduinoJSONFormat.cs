using System;
using DefiSSMCOM.Arduino;

namespace DefiSSMCOM.WebSocket.JSON
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
