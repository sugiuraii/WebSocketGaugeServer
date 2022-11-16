using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;
using SZ2.WebSocketGaugeServer.WebSocketServer.SessionItems;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat;
using SZ2.WebSocketGaugeServer.WebSocketCommon.Utils;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Service
{
    public class SSMCOMService : IDisposable
    {
        private readonly ISSMCOM ssmCOM;
        private readonly VirtualSSMCOM virtualSSMCOM;
        private readonly Timer update_ssmflag_timer;
        private readonly Dictionary<Guid, (WebSocket WebSocket, SSMCOMWebsocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, SSMCOMWebsocketSessionParam SessionParam)>();
        private readonly AsyncSemaphoreLock WebSocketDictionaryLock = new AsyncSemaphoreLock();
        private readonly ILogger logger;

        public async Task AddWebSocketAsync(Guid sessionGuid, WebSocket websocket)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Add(sessionGuid, (websocket, new SSMCOMWebsocketSessionParam()));
            }
        }

        public async Task RemoveWebSocketAsync(Guid sessionGuid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Remove(sessionGuid);
            }
        }

        public async Task<SSMCOMWebsocketSessionParam> GetSessionParamAsync(Guid guid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                return this.WebSocketDictionary[guid].SessionParam;
            }
        }
        public ISSMCOM SSMCOM { get => ssmCOM; }
        public VirtualSSMCOM VirtualSSMCOM
        {
            get
            {
                if (virtualSSMCOM != null)
                    return virtualSSMCOM;
                else
                    throw new InvalidOperationException("VirtualSSMCOM is null. Virtual com mode is not be enabled.");
            }
        }

        public SSMCOMService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<SSMCOMService> logger)
        {
            var serviceSetting = configuration.GetSection("ServiceConfig").GetSection("SSM");

            this.logger = logger;

            var useVirtual = Boolean.Parse(serviceSetting["usevirtual"]);
            logger.LogInformation("SSMCOM service is started.");
            if (useVirtual)
            {
                logger.LogInformation("SSMCOM is started with virtual mode.");
                int comWait = 15;
                logger.LogInformation("VirtualSSMCOM wait time is set to " + comWait.ToString() + " ms.");
                var virtualCOM = new VirtualSSMCOM(loggerFactory, comWait);
                this.ssmCOM = virtualCOM;
                this.virtualSSMCOM = virtualCOM;
            }
            else
            {
                logger.LogInformation("SSMCOM is started with physical mode.");
                var comportName = serviceSetting["comport"];
                logger.LogInformation("SSMCOM COMPort is set to: " + comportName);
                this.ssmCOM = new SSMCOM(loggerFactory, comportName);
                this.virtualSSMCOM = null;
            }

            var cancellationToken = lifetime.ApplicationStopping;

            // Register websocket broad cast
            this.ssmCOM.SSMDataReceived += async (sender, args) =>
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

            // Start SSMCOM communitcation thread.
            this.SSMCOM.BackgroundCommunicateStart();
            // Start perioddical SSMFlag update.
            this.update_ssmflag_timer = new Timer(new TimerCallback(updateSSMCOMReadflag), null, 0, Timeout.Infinite);
            update_ssmflag_timer.Change(0, 2000);
        }
        private async void updateSSMCOMReadflag(object stateobj)
        {
            // Do nothing if the running state is false.
            if (!this.SSMCOM.IsCommunitateThreadAlive)
                return;

            //reset all ssmcom flag
            this.SSMCOM.set_all_disable(true);
            using (await WebSocketDictionaryLock.LockAsync())
            {
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
        }
        public void Dispose()
        {
            var stopTask = Task.Run(() => this.SSMCOM.BackgroundCommunicateStop());
            Task.WhenAny(stopTask, Task.Delay(10000));
        }
    }
}
