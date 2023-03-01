using System;
using System.Collections.Generic;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class ELM327OutMessageParseResult
    {
        public byte ModeCode {get; private set; }
        public Dictionary<OBDIIParameterCode, string> ValueStrMap{get; private set; }

        public ELM327OutMessageParseResult(byte ModeCode, Dictionary<OBDIIParameterCode, string> ValueStrMap)
        {
            this.ModeCode = ModeCode;
            this.ValueStrMap = ValueStrMap;
        }
    }
}
