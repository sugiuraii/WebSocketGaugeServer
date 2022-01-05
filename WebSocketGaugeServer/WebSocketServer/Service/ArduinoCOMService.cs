using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;
using SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.WebSocketCommon.Utils;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Service
{
    public class ArduinoCOMService : IDisposable
    {
        private readonly ILogger logger;
        private readonly IArduinoCOM arduinoCOM;
        private readonly VirtualArduinoCOM virtualArduinoCOM;
        private readonly Dictionary<Guid, (WebSocket WebSocket, ArduinoCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, ArduinoCOMWebsocketSessionParam SessionParam)>();
        private readonly AsyncSemaphoreLock WebSocketDictionaryLock = new AsyncSemaphoreLock();

        public async Task AddWebSocketAsync(Guid sessionGuid, WebSocket websocket)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Add(sessionGuid, (websocket, new ArduinoCOMWebsocketSessionParam()));
            }
        }

        public async Task RemoveWebSocketAsync(Guid sessionGuid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Remove(sessionGuid);
            }
        }

        public async Task<ArduinoCOMWebsocketSessionParam> GetSessionParamAsync(Guid guid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                return this.WebSocketDictionary[guid].SessionParam;
            }
        }

        public IArduinoCOM ArduinoCOM { get => arduinoCOM; }

        public VirtualArduinoCOM VirtualArduinoCOM
        {
            get
            {
                if (virtualArduinoCOM != null)
                    return virtualArduinoCOM;
                else
                    throw new InvalidOperationException("Virtual arduino COM is null. Virtual com mode is not be enabled.");
            }
        }

        public ArduinoCOMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<ArduinoCOMService> logger)
        {
            var serviceSetting = configuration.GetSection("ServiceConfig").GetSection("Arduino");

            this.logger = logger;
            var useVirtual = Boolean.Parse(serviceSetting["usevirtual"]);
            logger.LogInformation("ArduinoCOM service is started.");
            if (useVirtual)
            {
                logger.LogInformation("ArduinoCOM is started with virtual mode.");
                int virtualArduinoCOMWait = 15;
                logger.LogInformation("VirtualArduinoCOM wait time is set to " + virtualArduinoCOMWait.ToString() + " ms.");
                var virtualCOM = new VirtualArduinoCOM(loggerFactory, virtualArduinoCOMWait);
                this.arduinoCOM = virtualCOM;
                this.virtualArduinoCOM = virtualCOM;
            }
            else
            {
                logger.LogInformation("ArduinoCOM is started with physical mode.");
                var comportName = serviceSetting["comport"];
                logger.LogInformation("ArduinoCOM COMPort is set to: " + comportName);
                this.arduinoCOM = new ArduinoCOM(loggerFactory, comportName);
                this.virtualArduinoCOM = null;
            }

            var cancellationToken = lifetime.ApplicationStopping;

            // Register websocket broad cast
            this.arduinoCOM.ArduinoPacketReceived += async (sender, args) =>
            {
                try
                {
                    using (await WebSocketDictionaryLock.LockAsync())
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
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogInformation(ex.Message);
                }
            };
            this.ArduinoCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.ArduinoCOM.BackgroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
