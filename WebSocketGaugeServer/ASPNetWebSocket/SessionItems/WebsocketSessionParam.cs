using System;
using System.Collections.Generic;
using DefiSSMCOM.Defi;

namespace DefiSSMCOM.WebSocket
{
    /// <summary>
    /// Class to store websocket session parameters.
    /// </summary>
    public abstract class WebsocketSessionParam
    {
        public abstract void reset();
    }

    /// <summary>
    /// Class to store simple (do not contains slow/fast read mode) weboscket session parameter.
    /// </summary>
    /// <typeparam name="parameterCodeType">parameter code type (Defi or Arduino)</typeparam>
    public class SimpleWebsocketSessionParam<parameterCodeType> : WebsocketSessionParam where parameterCodeType : struct
    {
        /// <summary>
        /// Parameter code list to communicate in websocket.
        /// </summary>
        public Dictionary<parameterCodeType, bool> Sendlist;
        /// <summary>
        /// Interval to send websocket messsage.
        /// </summary>
        public int SendInterval { get; set; }
        public int SendCount { get; set; }

        public SimpleWebsocketSessionParam()
        {
            this.Sendlist = new Dictionary<parameterCodeType, bool>();

            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.Sendlist.Add(code, false);
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }

        public override void reset()
        {
            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.Sendlist[code] = false;
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }
    }

    public class SlowFastWebsocketSessionParam<parameterCodeType> : WebsocketSessionParam where parameterCodeType : struct
    {
        public Dictionary<parameterCodeType, bool> SlowSendlist, FastSendlist;

        public SlowFastWebsocketSessionParam()
        {
            this.SlowSendlist = new Dictionary<parameterCodeType, bool>();
            this.FastSendlist = new Dictionary<parameterCodeType, bool>();

            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
        }

        public override void reset()
        {
            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
        }
    }
}