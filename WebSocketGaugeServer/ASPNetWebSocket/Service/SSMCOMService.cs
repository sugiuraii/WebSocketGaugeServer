using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using DefiSSMCOM.SSM;
using DefiSSMCOM.WebSocket;
using DefiSSMCOM.WebSocket.JSON;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using log4net;

namespace ASPNetWebSocket.Service
{
    public class SSMCOMService : IDisposable
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));
        private readonly SSMCOM ssmCOM;
        private readonly Dictionary<Guid, WebSocket> WebSockets = new Dictionary<Guid, WebSocket>();
        private readonly Dictionary<Guid, SSMCOMWebsocketSessionParam> SessionParams = new Dictionary<Guid, SSMCOMWebsocketSessionParam>();

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSockets.Add(sessionGuid, websocket);
            this.SessionParams.Add(sessionGuid, new SSMCOMWebsocketSessionParam());
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSockets.Remove(sessionGuid);
            this.SessionParams.Remove(sessionGuid);
        }

        public SSMCOMWebsocketSessionParam GetSessionParam(Guid guid) 
        {
            return this.SessionParams[guid];
        }

        public SSMCOM SSMCOM { get { return ssmCOM; } }
        public SSMCOMService(string comportName)
        {
            this.ssmCOM = new SSMCOM();
            this.ssmCOM.PortName = comportName;

            // Register websocket broad cast
            this.ssmCOM.SSMDataReceived += async (sender, args) =>
            {
                try
                {
                    foreach (var session in WebSockets)
                    {
                        var guid = session.Key;
                        var websocket = session.Value;
                        var sessionparam = GetSessionParam(guid);

                        var msg_data = new ValueJSONFormat();        
                        foreach (SSMParameterCode ssmcode in args.Received_Parameter_Code) 
                        {
                            if (sessionparam.FastSendlist[ssmcode] || sessionparam.SlowSendlist[ssmcode])
                            {
                                // Return Switch content
                                if (ssmcode >= SSMParameterCode.Switch_P0x061 && ssmcode <= SSMParameterCode.Switch_P0x121)
                                {
                                    List<SSMSwitchCode> switch_code_list = SSMContentTable.getSwitchcodesFromParametercode(ssmcode);
                                    foreach (SSMSwitchCode switch_code in switch_code_list)
                                    {
                                        msg_data.val.Add(switch_code.ToString(), ssmCOM.get_switch(switch_code).ToString());
                                    }
                                }
                                // Return Numeric content
                                else
                                {
                                    msg_data.val.Add(ssmcode.ToString(), ssmCOM.get_value(ssmcode).ToString());
                                }
                                msg_data.Validate();
                            }
                        }

                        if (msg_data.val.Count > 0)
                        {
                            string msg = JsonConvert.SerializeObject(msg_data);
                            byte[] buf = Encoding.UTF8.GetBytes(msg);
                            await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch(WebSocketException ex)
                {
                    logger.Warn(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.Warn(ex.StackTrace);
                }
            };

            this.SSMCOM.BackgroundCommunicateStart();
        }

        public void Dispose()
        {
            var stopTask = Task.Run(() => this.SSMCOM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
