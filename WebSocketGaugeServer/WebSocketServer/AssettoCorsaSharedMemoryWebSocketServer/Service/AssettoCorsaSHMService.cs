using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.SessionItems;
using Microsoft.Extensions.Configuration;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.Service
{
    public class AssettoCorsaSHMService : IDisposable
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        private readonly AssetoCorsaSHMBackgroundCommunicator assettoCorsaCOM;

        private readonly Dictionary<Guid, (WebSocket WebSocket, AssettoCorsaWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, AssettoCorsaWebsocketSessionParam SessionParam)>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSocketDictionary.Add(sessionGuid, (websocket, new AssettoCorsaWebsocketSessionParam()));
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSocketDictionary.Remove(sessionGuid);
        }

        public AssettoCorsaWebsocketSessionParam GetSessionParam(Guid guid)
        {
            return this.WebSocketDictionary[guid].SessionParam;
        }
        public AssetoCorsaSHMBackgroundCommunicator AssettoCorsaCOM { get { return this.assettoCorsaCOM; } }
        public AssettoCorsaSHMService(IConfiguration configuration)
        {
            double physicaInterval = Double.Parse(configuration["physicaInterval"]);
            double graphicsInterval = Double.Parse(configuration["graphicsInterval"]);
            double staticInfoInterval = Double.Parse(configuration["staticInfoInterval"]);
            
            this.assettoCorsaCOM = new AssetoCorsaSHMBackgroundCommunicator(physicaInterval, graphicsInterval, staticInfoInterval);
            this.assettoCorsaCOM.AssettoCorsaSharedMemory.PhysicsUpdated += async (sender, e) =>
            {
                try
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
                                await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            sessionparam.PhysicsDataSendCount = 0;
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
                }
            };

            this.assettoCorsaCOM.AssettoCorsaSharedMemory.GraphicsUpdated += async (sender, e) =>
            {
                try
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
                                await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            sessionparam.GraphicsDataSendCount = 0;
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
                }
            };

            this.assettoCorsaCOM.AssettoCorsaSharedMemory.StaticInfoUpdated += async (sender, e) =>
            {
                try
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
                                await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            sessionparam.GraphicsDataSendCount = 0;
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
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
