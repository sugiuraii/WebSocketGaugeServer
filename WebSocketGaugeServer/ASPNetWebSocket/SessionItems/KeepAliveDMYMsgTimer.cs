using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;

namespace DefiSSMCOM.WebSocket
{
    using WebSocket = System.Net.WebSockets.WebSocket;
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

        private readonly WebSocket ws;

        private readonly Timer keepAliveTimer;

        public KeepAliveDMYMsgTimer(WebSocket ws, int interval)
        {
            this.ws = ws;
            this.KeepAliveInterval = interval;

            this.keepAliveTimer = new Timer(async (obj) => 
            {
                KeepAliveDMYMsgTimer timerObj = (KeepAliveDMYMsgTimer)obj;
                var ws = timerObj.ws;
                
                await WebsocketSendText(ws, DUMMY_MESSAGE);

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

        private async Task WebsocketSendText(WebSocket ws, string text)
        {
            byte[] outbuf = Encoding.UTF8.GetBytes(text);
            await ws.SendAsync(new ArraySegment<byte>(outbuf), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
