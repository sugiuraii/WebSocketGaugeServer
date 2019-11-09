using DefiSSMCOM.AssetoCorsaSHM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM.Websocket.AssettoCorsaSHM
{
    /// <summary>
    /// Property data for AssettoCorsaWebsocket sessions.
    /// </summary>
    public class AssettoCorsaWebsocketSessionParam
    {
        public readonly Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool> PhysicsDataSendList;
        public readonly Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool> GraphicsDataSendList;
        public readonly Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool> StaticInfoDataSendList;

        public uint PhysicsDataSendInterval = 0;
        public uint GraphicsDataSendInterval = 0;
        public uint StaticInfoDataSendInterval = 0;

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
