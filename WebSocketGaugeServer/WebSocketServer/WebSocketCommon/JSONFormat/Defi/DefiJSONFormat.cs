using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat.Defi
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
