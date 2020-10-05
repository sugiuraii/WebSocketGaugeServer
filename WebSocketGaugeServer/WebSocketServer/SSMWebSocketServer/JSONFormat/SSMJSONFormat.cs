using System;
using DefiSSMCOM.SSM;

namespace DefiSSMCOM.WebSocket.JSON
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
