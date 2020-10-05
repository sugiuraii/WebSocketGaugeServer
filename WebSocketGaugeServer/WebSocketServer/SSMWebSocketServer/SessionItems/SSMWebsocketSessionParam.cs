using System;
using System.Collections.Generic;
using DefiSSMCOM.SSM;

namespace DefiSSMCOM.WebSocket
{
    public class SSMCOMWebsocketSessionParam
    {
        public Dictionary<SSMParameterCode, bool> SlowSendlist, FastSendlist;

        public SSMCOMWebsocketSessionParam()
        {
            this.SlowSendlist = new Dictionary<SSMParameterCode, bool>();
            this.FastSendlist = new Dictionary<SSMParameterCode, bool>();

            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
        }

        public void reset()
        {
            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
        }
    }
}