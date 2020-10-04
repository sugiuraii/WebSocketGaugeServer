using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.ELM327WebSocketServer.JSONFormat
{
    public class ELM327COMReadJSONFormat : SlowFastCOMReadJSONFormat<OBDIIParameterCode>
    {
        public const string ModeCode = "ELM327_COM_READ";
        public ELM327COMReadJSONFormat() : base(ModeCode)
        {
        }
    }

    public class ELM327SLOWREADIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "ELM327_SLOWREAD_INTERVAL";
        public ELM327SLOWREADIntervalJSONFormat() : base(ModeCode)
        {
        }
    }
}
