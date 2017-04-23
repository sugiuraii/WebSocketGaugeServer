using System;
using DefiSSMCOM.Defi;

namespace DefiSSMCOM.WebSocket.JSON
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
