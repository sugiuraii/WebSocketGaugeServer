using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;
using SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer.SessionItems;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.SSMWebSocketServer.Service
{
    public class SSMCOMService : IDisposable
    {        
        private readonly SSMCOM ssmCOM;
        private readonly Timer update_ssmflag_timer;
        private readonly Dictionary<Guid, (WebSocket WebSocket, SSMCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, SSMCOMWebsocketSessionParam SessionParam)>();
        private readonly ILogger logger;

        public void AddWebSocket(Guid sessionGuid, WebSocket websocket)
        {
            this.WebSocketDictionary.Add(sessionGuid, (websocket, new SSMCOMWebsocketSessionParam()));
        }

        public void RemoveWebSocket(Guid sessionGuid)
        {
            this.WebSocketDictionary.Remove(sessionGuid);
        }

        public SSMCOMWebsocketSessionParam GetSessionParam(Guid guid)
        {
            return this.WebSocketDictionary[guid].SessionParam;
        }

        public SSMCOM SSMCOM { get { return ssmCOM; } }
        public SSMCOMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<SSMCOMService> logger)
        {
            this.logger = logger;
            var comportName = configuration["comport"];

            this.ssmCOM = new SSMCOM(loggerFactory);
            this.ssmCOM.PortName = comportName;

            var cancellationToken = lifetime.ApplicationStopping;

            // Register websocket broad cast
            this.ssmCOM.SSMDataReceived += async (sender, args) =>
            {
                try
                {
                    foreach (var session in WebSocketDictionary)
                    {
                        var guid = session.Key;
                        var websocket = session.Value.WebSocket;
                        var sessionparam = session.Value.SessionParam;

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

            // Start SSMCOM communitcation thread.
            this.SSMCOM.BackgroundCommunicateStart();
            // Start perioddical SSMFlag update.
            this.update_ssmflag_timer = new Timer(new TimerCallback(updateSSMCOMReadflag), null, 0, Timeout.Infinite);
            update_ssmflag_timer.Change(0, 2000);
        }
        private void updateSSMCOMReadflag(object stateobj)
        {
            // Do nothing if the running state is false.
            if (!this.SSMCOM.IsCommunitateThreadAlive)
                return;

            //reset all ssmcom flag
            this.SSMCOM.set_all_disable(true);

            if (WebSocketDictionary.Count < 1)
                return;

            foreach (var session in WebSocketDictionary)
            {
                var websocket = session.Value.WebSocket;
                var sessionparam = session.Value.SessionParam;

                if (websocket.State != WebSocketState.Open) // Avoid null session bug
                    continue;

                foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
                {
                    if (sessionparam.FastSendlist[code])
                    {
                        if (!SSMCOM.get_fastread_flag(code))
                            SSMCOM.set_fastread_flag(code, true, true);
                    }
                    if (sessionparam.SlowSendlist[code])
                    {
                        if (!SSMCOM.get_slowread_flag(code))
                            SSMCOM.set_slowread_flag(code, true, true);
                    }
                }
            }
        }
        public void Dispose()
        {
            var stopTask = Task.Run(() => this.SSMCOM.BackGroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
