using System;
using System.Collections.Generic;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class PIDToOBDIIParameterCodeReverseMapBuilder
    {
        public Dictionary<byte, OBDIIParameterCode> create()
        {
            var contentTable = new OBDIIContentTable();
            var revDict = new Dictionary<byte, OBDIIParameterCode>();
            foreach(OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
            {
                var pid = contentTable[code].PID;
                revDict.Add(pid, code);
            }

            return revDict;
        }
    }
}