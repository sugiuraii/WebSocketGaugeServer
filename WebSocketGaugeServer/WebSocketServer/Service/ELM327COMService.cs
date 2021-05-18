using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327;
using SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Service
{
    public class ELM327COMService : IDisposable
    {
        private readonly IELM327COM elm327COM;
        private readonly VirtualELM327COM virtualElm327COM;
        private readonly Timer update_obdflag_timer;
        private readonly Dictionary<Guid, (WebSocket WebSocket, ELM327WebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, ELM327WebsocketSessionParam SessionParam)>();
        private readonly ILogger logger;

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

        public IELM327COM ELM327COM { get  => elm327COM; }
        public VirtualELM327COM VirtualELM327COM { 
            get 
            {
                if(virtualElm327COM != null)
                    return virtualElm327COM;
                else
                    throw new InvalidOperationException("Virtual ELM327 COM is null. Virtual com mode is not be enabled.");
            }
        }

        public ELM327COMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<ELM327COMService> logger)
        {
            var serviceSetting = configuration.GetSection("ServiceConfig").GetSection("ELM327");
                        
            this.logger = logger;
            var useVirtual = Boolean.Parse(serviceSetting["usevirtual"]);
            logger.LogInformation("ELM327COM service is started.");
            if(useVirtual)
            {
                logger.LogInformation("ELM327COM is started with virtual mode.");
                int comWait = 15;
                logger.LogInformation("VirtualELM327COM wait time is set to " + comWait.ToString() + " ms.");
                var virtualCOM = new VirtualELM327COM(loggerFactory, comWait);
                this.elm327COM = virtualCOM;
                this.virtualElm327COM = virtualCOM;               
            }
            else
            {
                var comportName = serviceSetting["comport"];
                var baudRate = Int32.Parse(serviceSetting["baudrate"]);
                var elm327ProtocolMode = serviceSetting["elm327ProtocolMode"];
                var elm327AdaptiveTimingMode = serviceSetting["elm327AdaptiveTimingControl"];
                var elm327Timeout = serviceSetting["elm327Timeout"];
                
                ELM327COM elm327COM;
                if(elm327ProtocolMode == null || elm327AdaptiveTimingMode == null || elm327Timeout == null)
                    elm327COM = new ELM327COM(loggerFactory, comportName);
                else
                    elm327COM = new ELM327COM(loggerFactory, comportName, elm327ProtocolMode, Int32.Parse(elm327AdaptiveTimingMode), Int32.Parse(elm327Timeout));
                
                elm327COM.overrideDefaultBaudRate(baudRate);

                this.elm327COM = elm327COM;
                this.virtualElm327COM = null;
            }

            var cancellationToken = lifetime.ApplicationStopping;
            
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
                            if (websocket.State == WebSocketState.Open)
                                await session.Value.WebSocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };

            // Start ELM327COM communitcation thread.
            this.ELM327COM.BackgroundCommunicateStart();
            // Start perioddical OBDFlag update.
            this.update_obdflag_timer = new Timer(new TimerCallback(updateOBDReadflag), null, 0, Timeout.Infinite);
            update_obdflag_timer.Change(0, 2000);
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

                if (websocket.State != WebSocketState.Open) // Avoid null session bug
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
            var stopTask = Task.Run(() => this.ELM327COM.BackgroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
