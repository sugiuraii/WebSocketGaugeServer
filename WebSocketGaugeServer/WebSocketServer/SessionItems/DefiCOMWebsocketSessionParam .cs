using System;
using System.Collections.Generic;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems
{
    public class DefiCOMWebsocketSessionParam
    {
        /// <summary>
        /// Parameter code list to communicate in websocket.
        /// </summary>
        public Dictionary<DefiParameterCode, bool> Sendlist;
        /// <summary>
        /// Interval to send websocket messsage.
        /// </summary>
        public int SendInterval { get; set; }
        public int SendCount { get; set; }

        public DefiCOMWebsocketSessionParam()
        {
            this.Sendlist = new Dictionary<DefiParameterCode, bool>();

            foreach (DefiParameterCode code in Enum.GetValues(typeof(DefiParameterCode)))
            {
                this.Sendlist.Add(code, false);
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }

        public void reset()
        {
            foreach (DefiParameterCode code in Enum.GetValues(typeof(DefiParameterCode)))
            {
                this.Sendlist[code] = false;
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }
    }
}