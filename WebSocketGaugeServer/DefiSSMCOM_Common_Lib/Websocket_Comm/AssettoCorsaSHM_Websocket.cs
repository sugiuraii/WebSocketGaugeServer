using AssettoCorsaSharedMemory;
using DefiSSMCOM.AssetoCorsaSHM;
using DefiSSMCOM.Websocket.AssettoCorsaSHM;
using DefiSSMCOM.WebSocket.JSON;
using Newtonsoft.Json;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM.WebSocket
{
    public class AssetoCorsaSHMBackgroundCommunicator : IBackgroundCommunicate
    {
        public readonly AssettoCorsa AssettoCorsaSharedMemory;

        public AssetoCorsaSHMBackgroundCommunicator(double physicsInterval, double graphicsInterval, double staticInfoInterval)
        {
            AssettoCorsaSharedMemory = new AssettoCorsa();
            AssettoCorsaSharedMemory.PhysicsInterval = physicsInterval;
            AssettoCorsaSharedMemory.GraphicsInterval = graphicsInterval;
            AssettoCorsaSharedMemory.StaticInfoInterval = staticInfoInterval;
        }

        public void BackgroundCommunicateStart()
        {
            AssettoCorsaSharedMemory.Start();
        }
        
        public void BackGroundCommunicateStop()
        {
            AssettoCorsaSharedMemory.Stop();
        }

        public bool IsCommunitateThreadAlive 
        {
            get
            {
                return AssettoCorsaSharedMemory.IsRunning;
            }
        }
    }

    public class AssettoCorsaSHM_Websocket : WebSocketServerCommon
    {
        private AssetoCorsaSHMBackgroundCommunicator acBackComm;
        public AssettoCorsaSHM_Websocket(double physicaInterval, double graphicsInterval, double staticInfoInterval)
        {
            WebsocketPortNo = 2017;
            acBackComm = new AssetoCorsaSHMBackgroundCommunicator(physicaInterval, graphicsInterval, staticInfoInterval);
            acBackComm.AssettoCorsaSharedMemory.PhysicsUpdated += acBackComm_PhysicsUpdated;
            acBackComm.AssettoCorsaSharedMemory.GraphicsUpdated += acBackComm_GraphicsUpdated;
            acBackComm.AssettoCorsaSharedMemory.StaticInfoUpdated += acBackComm_StaticInfoUpdated;
        }

        protected override WebsocketSessionParam createSessionParam()
        {
            return new AssettoCorsaWebsocketSessionParam();
        }

        protected override void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session)
        {
            AssettoCorsaWebsocketSessionParam sessionparam = (AssettoCorsaWebsocketSessionParam)session.Items["Param"];
            switch (receivedJSONmode)
            {
                case (ResetJSONFormat.ModeCode):
                    sessionparam.reset();
                    send_response_msg(session, "AssettoCorsaSHM Websocket all parameter reset.");
                    break;
                case (AssettoCorsaPhysicsSendJSONFormat.ModeCode):
                    {
                        AssettoCorsaPhysicsSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaPhysicsSendJSONFormat>(message);
                        msg_obj_wssend.Validate();
                        sessionparam.PhysicsDataSendList[(AssettoCorsaSHMPhysicsParameterCode)Enum.Parse(typeof(AssettoCorsaSHMPhysicsParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                        send_response_msg(session, "AsstoCorsa Websocket Physics send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                        break;
                    }
                case (AssettoCorsaGraphicsSendJSONFormat.ModeCode):
                    {
                        AssettoCorsaGraphicsSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaGraphicsSendJSONFormat>(message);
                        msg_obj_wssend.Validate();
                        sessionparam.GraphicsDataSendList[(AssettoCorsaSHMGraphicsParameterCode)Enum.Parse(typeof(AssettoCorsaSHMGraphicsParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                        send_response_msg(session, "AsstoCorsa Websocket Graphics send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                        break;
                    }
                case (AssettoCorsaStaticInfoSendJSONFormat.ModeCode):
                    {
                        AssettoCorsaStaticInfoSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<AssettoCorsaStaticInfoSendJSONFormat>(message);
                        msg_obj_wssend.Validate();
                        sessionparam.StaticInfoDataSendList[(AssettoCorsaSHMStaticInfoParameterCode)Enum.Parse(typeof(AssettoCorsaSHMStaticInfoParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                        send_response_msg(session, "AsstoCorsa Websocket StaticInfo send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                        break;
                    }
                case (AssettoCorsaPhysicsWSIntervalJSONFormat.ModeCode):
                    {
                        AssettoCorsaPhysicsWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaPhysicsWSIntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        sessionparam.PhysicsDataSendInterval = msg_obj_interval.interval;

                        send_response_msg(session, "AsstoCorsa Websocket Physics send_interval to : " + msg_obj_interval.interval.ToString());
                        break;
                    }
                case (AssettoCorsaGraphicsWSIntervalJSONFormat.ModeCode):
                    {
                        AssettoCorsaGraphicsWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaGraphicsWSIntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        sessionparam.GraphicsDataSendInterval = msg_obj_interval.interval;

                        send_response_msg(session, "AsstoCorsa Websocket Graphics send_interval to : " + msg_obj_interval.interval.ToString());
                        break;
                    }
                case (AssettoCorsaStaticInfoWSIntervalJSONFormat.ModeCode):
                    {
                        AssettoCorsaStaticInfoWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<AssettoCorsaStaticInfoWSIntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        sessionparam.StaticInfoDataSendInterval = msg_obj_interval.interval;

                        send_response_msg(session, "AsstoCorsa Websocket StaticInfo send_interval to : " + msg_obj_interval.interval.ToString());
                        break;
                    }
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }

        private void acBackComm_PhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                AssettoCorsaWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (AssettoCorsaWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                if (sessionparam.PhysicsDataSendCount < sessionparam.PhysicsDataSendInterval)
                    sessionparam.PhysicsDataSendCount++;
                else
                {
                    AssettoCorsaSHMVALJSONMapper mapper = new AssettoCorsaSHMVALJSONMapper();
                    Physics physicsSHM = e.Physics;
                    ValueJSONFormat msg_data = mapper.CreatePhysicsParameterValueJSON(sessionparam.PhysicsDataSendList, physicsSHM);
                    
                    if (msg_data.val.Count > 0)
                    {
                        String msg = JsonConvert.SerializeObject(msg_data);
                        session.Send(msg);
                    }
                    sessionparam.PhysicsDataSendCount = 0;

                }
            }
        }

        private void acBackComm_GraphicsUpdated(object sender, GraphicsEventArgs e)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                AssettoCorsaWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (AssettoCorsaWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                if (sessionparam.GraphicsDataSendCount < sessionparam.GraphicsDataSendInterval)
                    sessionparam.GraphicsDataSendCount++;
                else
                {
                    AssettoCorsaSHMVALJSONMapper mapper = new AssettoCorsaSHMVALJSONMapper();
                    Graphics graphicsSHM = e.Graphics;
                    ValueJSONFormat msg_data = mapper.CreateGraphicsParameterValueJSON(sessionparam.GraphicsDataSendList, graphicsSHM);

                    if (msg_data.val.Count > 0)
                    {
                        String msg = JsonConvert.SerializeObject(msg_data);
                        session.Send(msg);
                    }
                    sessionparam.GraphicsDataSendCount = 0;
                }
            }
        }

        private void acBackComm_StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                AssettoCorsaWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (AssettoCorsaWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                if (sessionparam.StaticInfoDataSendCount < sessionparam.StaticInfoDataSendInterval)
                    sessionparam.StaticInfoDataSendCount++;
                else
                {
                    AssettoCorsaSHMVALJSONMapper mapper = new AssettoCorsaSHMVALJSONMapper();
                    StaticInfo staticInfoSHM = e.StaticInfo;
                    ValueJSONFormat msg_data = mapper.CreateStaticInfoParameterValueJSON(sessionparam.StaticInfoDataSendList, staticInfoSHM);

                    if (msg_data.val.Count > 0)
                    {
                        String msg = JsonConvert.SerializeObject(msg_data);
                        session.Send(msg);
                    }
                    sessionparam.StaticInfoDataSendCount = 0;
                }
            }
        }
    }
}
