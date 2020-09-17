using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using DefiSSMCOM.OBDII;
using DefiSSMCOM.WebSocket;
using DefiSSMCOM.WebSocket.JSON;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using log4net;

namespace ASPNetWebSocket.Service
{
    public class ELM327COMService : IDisposable
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        private readonly ELM327COM elm327COM;
        private readonly Timer update_obdflag_timer;
        private readonly Dictionary<Guid, (WebSocket WebSocket, ELM327WebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, ELM327WebsocketSessionParam SessionParam)>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSocketDictionary.Add(sessionGuid, (websocket, new ELM327WebsocketSessionParam()));
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSocketDictionary.Remove(sessionGuid);
        }

        public ELM327WebsocketSessionParam GetSessionParam(Guid guid) 
        {
            return this.WebSocketDictionary[guid].SessionParam;
        }

        public ELM327COM ELM327COM { get { return elm327COM; } }
        public ELM327COMService(string comportName)
        {
            this.elm327COM = new ELM327COM();
            this.elm327COM.PortName = comportName;

            this.update_obdflag_timer = new Timer(new TimerCallback(updateOBDReadflag), null, 0, Timeout.Infinite);
            update_obdflag_timer.Change(0, 2000);
            // Register websocket broad cast
            this.elm327COM.ELM327DataReceived += async (sender, args) =>
            {
                try
                {
                    foreach (var session in WebSocketDictionary)
                    {
                        var guid = session.Key;
                        var websocket = session.Value.WebSocket;
                        var sessionparam = session.Value.SessionParam;

                        var msg_data = new ValueJSONFormat();        
                        foreach (var code in args.Received_Parameter_Code) 
                        {
                            if (sessionparam.FastSendlist[code] || sessionparam.SlowSendlist[code])
                            {
                                msg_data.val.Add(code.ToString(), elm327COM.get_value(code).ToString());
                                msg_data.Validate();
                            }
                        }

                        if (msg_data.val.Count > 0)
                        {
                            string msg = JsonConvert.SerializeObject(msg_data);
                            byte[] buf = Encoding.UTF8.GetBytes(msg);
                            await session.Value.WebSocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch(WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
                }
            };

            this.ELM327COM.BackgroundCommunicateStart();
        }
        private void updateOBDReadflag(object stateobj)
        {
            // Do nothing if the running state is false.
            if (!this.ELM327COM.IsCommunitateThreadAlive)
                return;

            //reset all ssmcom flag
            this.ELM327COM.set_all_disable(true);
            
            if (WebSocketDictionary.Count < 1)
                return;

            foreach (var session in WebSocketDictionary)
            {
                var websocket = session.Value.WebSocket;
                var sessionparam = session.Value.SessionParam;

                if (websocket.State !=  WebSocketState.Open) // Avoid null session bug
                    continue;

                foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
                {
                    if (sessionparam.FastSendlist[code])
                    {
                        if (!ELM327COM.get_fastread_flag(code))
                            ELM327COM.set_fastread_flag(code, true, true);
                    }
                    if (sessionparam.SlowSendlist[code])
                    {
                        if (!ELM327COM.get_slowread_flag(code))
                            ELM327COM.set_slowread_flag(code, true, true);
                    }
                }
            }
        }
        public void Dispose()
        {
            var stopTask = Task.Run(() => this.ELM327COM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
