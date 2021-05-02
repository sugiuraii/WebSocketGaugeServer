using System;
using System.Collections.Generic;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems
{
    public class ArduinoCOMWebsocketSessionParam
    {
        /// <summary>
        /// Parameter code list to communicate in websocket.
        /// </summary>
        public Dictionary<ArduinoParameterCode, bool> Sendlist;
        /// <summary>
        /// Interval to send websocket messsage.
        /// </summary>
        public int SendInterval { get; set; }
        public int SendCount { get; set; }

        public ArduinoCOMWebsocketSessionParam()
        {
            this.Sendlist = new Dictionary<ArduinoParameterCode, bool>();

            foreach (ArduinoParameterCode code in Enum.GetValues(typeof(ArduinoParameterCode)))
            {
                this.Sendlist.Add(code, false);
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }

        public void reset()
        {
            foreach (ArduinoParameterCode code in Enum.GetValues(typeof(ArduinoParameterCode)))
            {
                this.Sendlist[code] = false;
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }
    }
}