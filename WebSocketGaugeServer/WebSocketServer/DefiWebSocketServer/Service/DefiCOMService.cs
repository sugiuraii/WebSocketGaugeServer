using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;
using log4net;
using SZ2.WebSocketGaugeServer.WebSocketServer.DefiWebSocketServer.SessionItems;
using Newtonsoft.Json;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using Microsoft.Extensions.Configuration;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.DefiWebSocketServer.Service
{
    public class DefiCOMService : IDisposable
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        private readonly DefiCOM defiCOM;
        private readonly Dictionary<Guid, (WebSocket WebSocket, DefiCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, DefiCOMWebsocketSessionParam SessionParam)>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSocketDictionary.Add(sessionGuid, (websocket, new DefiCOMWebsocketSessionParam()));
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSocketDictionary.Remove(sessionGuid);
        }

        public DefiCOMWebsocketSessionParam GetSessionParam(Guid guid) 
        {
            return this.WebSocketDictionary[guid].SessionParam;
        }

        public DefiCOM DefiCOM { get { return defiCOM; } }
        public DefiCOMService(IConfiguration configuration)
        {
            var comportName = configuration["comport"];
            
            this.defiCOM = new DefiCOM();
            this.defiCOM.PortName = comportName;

            // Register websocket broad cast
            this.defiCOM.DefiPacketReceived += async (sender, args) =>
            {
                try
                {
                    foreach (var session in WebSocketDictionary)
                    {
                        var guid = session.Key;
                        var websocket = session.Value.WebSocket;
                        var sessionparam = session.Value.SessionParam;

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
                }
                catch(WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
                }
            };
            this.DefiCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.DefiCOM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
