﻿using System;
using System.Collections.Generic;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems
{
    public class ELM327WebsocketSessionParam
    {
        public Dictionary<OBDIIParameterCode, bool> SlowSendlist, FastSendlist;

        public ELM327WebsocketSessionParam()
        {
            this.SlowSendlist = new Dictionary<OBDIIParameterCode, bool>();
            this.FastSendlist = new Dictionary<OBDIIParameterCode, bool>();

            foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
        }

        public void reset()
        {
            foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
        }
    }
}