using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;
using SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.WebSocketCommon.Utils;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Service
{
    public class DefiCOMService : IDisposable
    {
        private readonly ILogger logger;
        private readonly IDefiCOM defiCOM;
        private readonly VirtualDefiCOM virtualDefiCOM;
        private readonly Dictionary<Guid, (WebSocket WebSocket, DefiCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, DefiCOMWebsocketSessionParam SessionParam)>();
        private readonly AsyncSemaphoreLock WebSocketDictionaryLock = new AsyncSemaphoreLock();

        public async Task AddWebSocketAsync(Guid sessionGuid, WebSocket websocket)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Add(sessionGuid, (websocket, new DefiCOMWebsocketSessionParam()));
            }
        }

        public async Task RemoveWebSocketAsync(Guid sessionGuid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Remove(sessionGuid);
            }
        }

        public async Task<DefiCOMWebsocketSessionParam> GetSessionParamAsync(Guid guid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                return this.WebSocketDictionary[guid].SessionParam;
            }
        }

        public IDefiCOM DefiCOM { get => defiCOM; }
        public VirtualDefiCOM VirtualDefiCOM
        {
            get
            {
                if (virtualDefiCOM != null)
                    return virtualDefiCOM;
                else
                    throw new InvalidOperationException("Virtual Defi COM is null. Virtual com mode is not be enabled.");
            }
        }
        public DefiCOMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<DefiCOMService> logger)
        {
            var serviceSetting = configuration.GetSection("ServiceConfig").GetSection("Defi");

            this.logger = logger;
            var useVirtual = Boolean.Parse(serviceSetting["usevirtual"]);
            logger.LogInformation("DefiCOM service is started.");
            if (useVirtual)
            {
                logger.LogInformation("DefiCOM is started with virtual mode.");
                int virtualDefiCOMWait = 15;
                logger.LogInformation("VirtualDefiCOM wait time is set to " + virtualDefiCOMWait.ToString() + " ms.");
                var virtualCOM = new VirtualDefiCOM(loggerFactory, virtualDefiCOMWait);
                this.defiCOM = virtualCOM;
                this.virtualDefiCOM = virtualCOM;
            }
            else
            {
                logger.LogInformation("DefiCOM is started with physical mode.");
                var comportName = serviceSetting["comport"];
                logger.LogInformation("DefiCOM COMPort is set to: " + comportName);
                this.defiCOM = new DefiCOM(loggerFactory, comportName);
                this.virtualDefiCOM = null;
            }

            var cancellationToken = lifetime.ApplicationStopping;
            // Register websocket broad cast
            this.defiCOM.DefiPacketReceived += async (sender, args) =>
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
                                foreach (DefiParameterCode deficode in Enum.GetValues(typeof(DefiParameterCode)))
                                {
                                    if (sessionparam.Sendlist[deficode])
                                        msg_data.val.Add(deficode.ToString(), defiCOM.get_value(deficode).ToString());
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
            this.DefiCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.DefiCOM.BackgroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
