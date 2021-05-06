using System;
using System.Collections.Generic;
using SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.SharedMemoryCommunication;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.SessionItems
{
    /// <summary>
    /// Property data for AssettoCorsaWebsocket sessions.
    /// </summary>
    public class AssettoCorsaWebsocketSessionParam
    {
        public readonly Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool> PhysicsDataSendList;
        public readonly Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool> GraphicsDataSendList;
        public readonly Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool> StaticInfoDataSendList;

        public int PhysicsDataSendInterval = 0;
        public int GraphicsDataSendInterval = 0;
        public int StaticInfoDataSendInterval = 0;

        public int PhysicsDataSendCount = 0;
        public int GraphicsDataSendCount = 0;
        public int StaticInfoDataSendCount = 0;

        public void reset()
        {
            PhysicsDataSendInterval = 0;
            GraphicsDataSendInterval = 0;
            StaticInfoDataSendInterval = 0;

            PhysicsDataSendCount = 0;
            GraphicsDataSendCount = 0;
            StaticInfoDataSendCount = 0;

            foreach (var x in PhysicsDataSendList.Keys)
                PhysicsDataSendList[x] = false;
            foreach (var x in GraphicsDataSendList.Keys)
                GraphicsDataSendList[x] = false;
            foreach (var x in StaticInfoDataSendList.Keys)
                StaticInfoDataSendList[x] = false;
        }

        public AssettoCorsaWebsocketSessionParam()
        {
            PhysicsDataSendList = new Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool>();
            GraphicsDataSendList = new Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool>();
            StaticInfoDataSendList = new Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool>();
            foreach (AssettoCorsaSHMPhysicsParameterCode c in Enum.GetValues(typeof(AssettoCorsaSHMPhysicsParameterCode)))
                PhysicsDataSendList.Add(c, false);
            foreach (AssettoCorsaSHMGraphicsParameterCode c in Enum.GetValues(typeof(AssettoCorsaSHMGraphicsParameterCode)))
                GraphicsDataSendList.Add(c, false);
            foreach (AssettoCorsaSHMStaticInfoParameterCode c in Enum.GetValues(typeof(AssettoCorsaSHMStaticInfoParameterCode)))
                StaticInfoDataSendList.Add(c, false);
        }
    }
}
