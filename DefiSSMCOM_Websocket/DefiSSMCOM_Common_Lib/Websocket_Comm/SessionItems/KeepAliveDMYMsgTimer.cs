using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SuperWebSocket;
using System.Net;

namespace DefiSSMCOM.WebSocket
{
    public class KeepAliveDMYMsgTimer
    {
        /// <summary>
        /// Interval of sending keep-alive dummy message (to prevent wifi sleep mode) to clients (in millisecond).
        /// </summary>
        private readonly int KeepAliveInterval;

        /// <summary>
        /// Dummy message string.
        /// </summary>
        private const string DUMMY_MESSAGE = "DMY";

        private readonly WebSocketSession webSocketSession;

        private readonly Timer keepAliveTimer;

        /// <summary>
        /// Constructor of KeepAliveMsgTimer
        /// </summary>
        /// <param name="session">Target WebSocket session.</param>
        /// <param name="interval">Keep alive interval in milisecond.</param>
        public KeepAliveDMYMsgTimer(WebSocketSession session, int interval)
        {
            this.webSocketSession = session;
            this.KeepAliveInterval = interval;

            this.keepAliveTimer = new Timer((object obj) => 
            {
                KeepAliveDMYMsgTimer timerObj = (KeepAliveDMYMsgTimer)obj;
                WebSocketSession targetSession = timerObj.webSocketSession;

                targetSession.Send(DUMMY_MESSAGE);

            },this,Timeout.Infinite, KeepAliveInterval);
        }

        /// <summary>
        /// Start sending dummy message periodically.
        /// </summary>
        public void Start()
        {
            this.keepAliveTimer.Change(0, KeepAliveInterval);
        }

        /// <summary>
        /// Stop sending dummy message periodically.
        /// </summary>
        public void Stop()
        {
            this.keepAliveTimer.Change(Timeout.Infinite, KeepAliveInterval);
        }
    }
}
