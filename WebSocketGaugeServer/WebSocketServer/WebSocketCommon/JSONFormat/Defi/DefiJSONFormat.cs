using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.DefiWebSocketServer.JSONFormat
{
    public class DefiWSSendJSONFormat : WSSendJSONFormat<DefiParameterCode>
    {
        public const string ModeCode = "DEFI_WS_SEND";
        public DefiWSSendJSONFormat() : base (ModeCode)
        {
        }
    }

    public class DefiWSIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "DEFI_WS_INTERVAL";
        public DefiWSIntervalJSONFormat() : base(ModeCode)
        {
        }
    }
}
