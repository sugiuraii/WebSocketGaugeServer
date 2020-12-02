using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat.SSM
{
    public class SSMCOMReadJSONFormat : SlowFastCOMReadJSONFormat<SSMParameterCode>
    {
        public const string ModeCode = "SSM_COM_READ";
        public SSMCOMReadJSONFormat() : base(ModeCode)
        {
        }
    }

    public class SSMSLOWREADIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "SSM_SLOWREAD_INTERVAL"; 
        public SSMSLOWREADIntervalJSONFormat(): base(ModeCode)
        {
        }
    }
}
