using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Service;
using SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.SessionItems;
using Newtonsoft.Json;
using SZ2.WebSocketGaugeServer.WebSocketCommon.Utils;

namespace SSZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Service
{
    public class AssettoCorsaSHMService : IDisposable
    {
        private readonly ILogger logger;
        private readonly AssetoCorsaSHMBackgroundCommunicator assettoCorsaCOM;

        private readonly Dictionary<Guid, (WebSocket WebSocket, AssettoCorsaWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, AssettoCorsaWebsocketSessionParam SessionParam)>();
        private readonly AsyncSemaphoreLock WebSocketDictionaryLock = new AsyncSemaphoreLock();

        public async Task AddWebSocketAsync(Guid sessionGuid, WebSocket websocket)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Add(sessionGuid, (websocket, new AssettoCorsaWebsocketSessionParam()));
            }
        }

        public async Task RemoveWebSocketAsync(Guid sessionGuid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Remove(sessionGuid);
            }
        }

        public async Task<AssettoCorsaWebsocketSessionParam> GetSessionParamAsync(Guid guid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                return this.WebSocketDictionary[guid].SessionParam;
            }
        }
        public AssetoCorsaSHMBackgroundCommunicator AssettoCorsaCOM { get { return this.assettoCorsaCOM; } }
        public AssettoCorsaSHMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILogger<AssettoCorsaSHMService> logger)
        {
            this.logger = logger;
            double physicaInterval = Double.Parse(configuration["physicaInterval"]);
            double graphicsInterval = Double.Parse(configuration["graphicsInterval"]);
            double staticInfoInterval = Double.Parse(configuration["staticInfoInterval"]);

            var cancellationToken = lifetime.ApplicationStopping;

            this.assettoCorsaCOM = new AssetoCorsaSHMBackgroundCommunicator(physicaInterval, graphicsInterval, staticInfoInterval);
            this.assettoCorsaCOM.AssettoCorsaSharedMemory.PhysicsUpdated += async (sender, e) =>
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

                            if (sessionparam.PhysicsDataSendCount < sessionparam.PhysicsDataSendInterval)
                                sessionparam.PhysicsDataSendCount++;
                            else
                            {
                                var mapper = new AssettoCorsaSHMVALJSONMapper();
                                var physicsSHM = e.Physics;
                                var msg_data = mapper.CreatePhysicsParameterValueJSON(sessionparam.PhysicsDataSendList, physicsSHM);

                                if (msg_data.val.Count > 0)
                                {
                                    string msg = JsonConvert.SerializeObject(msg_data);
                                    byte[] buf = Encoding.UTF8.GetBytes(msg);
                                    if (websocket.State == WebSocketState.Open)
                                        await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                                }
                                sessionparam.PhysicsDataSendCount = 0;
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };

            this.assettoCorsaCOM.AssettoCorsaSharedMemory.GraphicsUpdated += async (sender, e) =>
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

                            if (sessionparam.GraphicsDataSendCount < sessionparam.GraphicsDataSendInterval)
                                sessionparam.GraphicsDataSendCount++;
                            else
                            {
                                var mapper = new AssettoCorsaSHMVALJSONMapper();
                                var graphicsSHM = e.Graphics;
                                var msg_data = mapper.CreateGraphicsParameterValueJSON(sessionparam.GraphicsDataSendList, graphicsSHM);

                                if (msg_data.val.Count > 0)
                                {
                                    string msg = JsonConvert.SerializeObject(msg_data);
                                    byte[] buf = Encoding.UTF8.GetBytes(msg);
                                    if (websocket.State == WebSocketState.Open)
                                        await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                                }
                                sessionparam.GraphicsDataSendCount = 0;
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };

            this.assettoCorsaCOM.AssettoCorsaSharedMemory.StaticInfoUpdated += async (sender, e) =>
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

                            if (sessionparam.GraphicsDataSendCount < sessionparam.GraphicsDataSendInterval)
                                sessionparam.GraphicsDataSendCount++;
                            else
                            {
                                var mapper = new AssettoCorsaSHMVALJSONMapper();
                                var staticInfoSHM = e.StaticInfo;
                                var msg_data = mapper.CreateStaticInfoParameterValueJSON(sessionparam.StaticInfoDataSendList, staticInfoSHM);

                                if (msg_data.val.Count > 0)
                                {
                                    string msg = JsonConvert.SerializeObject(msg_data);
                                    byte[] buf = Encoding.UTF8.GetBytes(msg);
                                    if (websocket.State == WebSocketState.Open)
                                        await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                                }
                                sessionparam.GraphicsDataSendCount = 0;
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };
            this.assettoCorsaCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.AssettoCorsaCOM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
