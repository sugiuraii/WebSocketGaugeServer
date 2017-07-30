using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SuperWebSocket;
using System.Net.NetworkInformation;
using System.Net;

namespace DefiSSMCOM.WebSocket
{
    public class KeepAliveDMYMsgTimer
    {
        /// <summary>
        /// Interval of sending keep-alive dummy message (to prevent wifi sleep mode) to clients (in millisecond).
        /// </summary>
        public static const int KEEPALIVE_MESSAGE_INTERVAL = 90;

        /// <summary>
        /// Dummy message string.
        /// </summary>
        public static const string DUMMY_MESSAGE = "DMY";

        private readonly WebSocketSession webSocketSession;
        private readonly Ping ping = new Ping();

        private readonly Timer keepAliveTimer;

        public KeepAliveDMYMsgTimer(WebSocketSession session)
        {
            this.webSocketSession = session;

            this.keepAliveTimer = new Timer((object obj) => 
            {
                KeepAliveDMYMsgTimer timerObj = (KeepAliveDMYMsgTimer)obj;
                WebSocketSession targetSession = timerObj.webSocketSession;

                targetSession.Send(DUMMY_MESSAGE);

            },this,Timeout.Infinite, KEEPALIVE_MESSAGE_INTERVAL);
        }

        /// <summary>
        /// Start sending dummy message periodically.
        /// </summary>
        public void Start()
        {
            this.keepAliveTimer.Change(0, KEEPALIVE_MESSAGE_INTERVAL);
        }

        /// <summary>
        /// Stop sending dummy message periodically.
        /// </summary>
        public void Stop()
        {
            this.keepAliveTimer.Change(Timeout.Infinite, KEEPALIVE_MESSAGE_INTERVAL);
        }
    }
}
