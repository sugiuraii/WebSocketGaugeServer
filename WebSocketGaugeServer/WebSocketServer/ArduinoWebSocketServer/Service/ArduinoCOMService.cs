using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;
using SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer.SessionItems;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.ArduinoWebSocketServer.Service
{
    public class ArduinoCOMService : IDisposable
    {
        private readonly ArduinoCOM arduinoCOM;
        private readonly Dictionary<Guid, (WebSocket WebSocket, ArduinoCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, ArduinoCOMWebsocketSessionParam SessionParam)>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSocketDictionary.Add(sessionGuid, (websocket, new ArduinoCOMWebsocketSessionParam()));
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSocketDictionary.Remove(sessionGuid);
        }

        public ArduinoCOMWebsocketSessionParam GetSessionParam(Guid guid) 
        {
            return this.WebSocketDictionary[guid].SessionParam;
        }

        public ArduinoCOM ArduinoCOM { get { return arduinoCOM; } }
        public ArduinoCOMService(IConfiguration configuration, IHostApplicationLifetime lifetime)
        {
            var comportName = configuration["comport"];
            this.arduinoCOM = new ArduinoCOM();
            this.arduinoCOM.PortName = comportName;

            var cancellationToken = lifetime.ApplicationStopping;

            // Register websocket broad cast
            this.arduinoCOM.ArduinoPacketReceived += async (sender, args) =>
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
                            foreach (ArduinoParameterCode code in Enum.GetValues(typeof(ArduinoParameterCode)))
                            {
                                if (sessionparam.Sendlist[code])
                                    msg_data.val.Add(code.ToString(), arduinoCOM.get_value(code).ToString());
                            }

                            if (msg_data.val.Count > 0)
                            {
                                string msg = JsonConvert.SerializeObject(msg_data);
                                byte[] buf = Encoding.UTF8.GetBytes(msg);
                                if (websocket.State == WebSocketState.Open)
                                    await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                            }
                            sessionparam.SendCount = 0;
                        }
                    }
                }
                catch(WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };
            this.ArduinoCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.ArduinoCOM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
