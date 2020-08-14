using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using DefiSSMCOM.Defi;
using DefiSSMCOM.WebSocket;
using DefiSSMCOM.WebSocket.JSON;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;

namespace ASPNetWebSocket.Service
{
    public class DefiCOMService
    {
        private readonly DefiCOM defiCOM;
        private readonly Dictionary<Guid, WebSocket> WebSockets = new Dictionary<Guid, WebSocket>();
        private readonly Dictionary<Guid, DefiCOMWebsocketSessionParam> SessionParams = new Dictionary<Guid, DefiCOMWebsocketSessionParam>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSockets.Add(sessionGuid, websocket);
            this.SessionParams.Add(sessionGuid, new DefiCOMWebsocketSessionParam());
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSockets.Remove(sessionGuid);
            this.SessionParams.Remove(sessionGuid);
        }

        public DefiCOMWebsocketSessionParam GetSessionParam(Guid guid) 
        {
            return this.SessionParams[guid];
        }

        public DefiCOM DefiCOM { get { return defiCOM; } }
        public DefiCOMService(string comportName)
        {
            this.defiCOM = new DefiCOM();
            this.defiCOM.PortName = comportName;

            // Register websocket broad cast
            this.defiCOM.DefiPacketReceived += async (sender, args) =>
            {
                foreach (var session in WebSockets)
                {
                    var guid = session.Key;
                    var websocket = session.Value;
                    var sessionparam = GetSessionParam(guid);

                    var msg_data = new ValueJSONFormat();        
                    if (sessionparam.SendCount < sessionparam.SendInterval)
                        sessionparam.SendCount++;
                    else
                    {
                        foreach (DefiParameterCode deficode in Enum.GetValues(typeof(DefiParameterCode)))
                        {
                            if (sessionparam.Sendlist[deficode])
                                msg_data.val.Add(deficode.ToString(), defiCOM.get_value(deficode).ToString());
                        }

                        if (msg_data.val.Count > 0)
                        {
                            string msg = JsonConvert.SerializeObject(msg_data);
                            byte[] buf = Encoding.UTF8.GetBytes(msg);
                            await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        sessionparam.SendCount = 0;
                    }
                }
            };

            this.DefiCOM.BackgroundCommunicateStart();
        }
    }
}
