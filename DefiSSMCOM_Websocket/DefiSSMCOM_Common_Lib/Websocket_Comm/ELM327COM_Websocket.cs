using System;
using System.Threading;
using DefiSSMCOM.WebSocket.JSON;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using DefiSSMCOM.OBDII;

namespace DefiSSMCOM.WebSocket
{
    public class ELM327WebsocketSessionParam : SlowFastWebsocketSessionParam<OBDIIParameterCode>
    {
    }

    public class ELM327COM_Websocket : WebSocketCommon
    {
        private readonly ELM327COM elm327com;

        private readonly Timer update_obdflag_timer;

        public ELM327COM_Websocket ()
		{
			// Create Deficom
			elm327com = new ELM327COM ();
            com1 = elm327com;
			WebsocketPortNo = 2013;
			COMPortName = "COM5";
			elm327com.ELM327DataReceived += new EventHandler<ELM327DataReceivedEventArgs> (elm327com_ELMDataReceived);

            update_obdflag_timer = new Timer(new TimerCallback(update_obd_readflag), null, 0, Timeout.Infinite);
            update_obdflag_timer.Change(0, 2000);
		}

        protected override WebsocketSessionParam createSessionParam()
        {
            return new ELM327WebsocketSessionParam();
        }

        protected override void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session)
        {
            var sessionparam = (ELM327WebsocketSessionParam)session.Items["Param"];
            switch (receivedJSONmode)
            {
                //ELM327 COM all reset
                case (ResetJSONFormat.ModeCode):
                    sessionparam.reset();
                    send_response_msg(session, "ELM327COM session RESET. All send parameters are disabled.");
                    break;
                case (ELM327COMReadJSONFormat.ModeCode):
                    ELM327COMReadJSONFormat msg_obj_elm327read = JsonConvert.DeserializeObject<ELM327COMReadJSONFormat>(message);
                    msg_obj_elm327read.Validate();

                    OBDIIParameterCode target_code = (OBDIIParameterCode)Enum.Parse(typeof(OBDIIParameterCode), msg_obj_elm327read.code);
                    bool flag = msg_obj_elm327read.flag;

                    if (msg_obj_elm327read.read_mode == ELM327COMReadJSONFormat.FastReadModeCode)
                    {
                        sessionparam.FastSendlist[target_code] = flag;
                    }
                    else
                    {
                        sessionparam.SlowSendlist[target_code] = flag;
                    }
                    send_response_msg(session, "EM327COM session read flag for : " + target_code.ToString() + " read_mode :" + msg_obj_elm327read.read_mode + " set to : " + flag.ToString());
                    break;

                case (ELM327SLOWREADIntervalJSONFormat.ModeCode):
                    ELM327SLOWREADIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<ELM327SLOWREADIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    elm327com.SlowReadInterval = msg_obj_interval.interval;

                    send_response_msg(session, "ELM327COM slowread interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }

        private void elm327com_ELMDataReceived(object sender, ELM327DataReceivedEventArgs args)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                ELM327WebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (ELM327WebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Fatal("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                ValueJSONFormat msg_data = new ValueJSONFormat();
                foreach (OBDIIParameterCode obdcode in args.Received_Parameter_Code)
                {
                    if (sessionparam.FastSendlist[obdcode] || sessionparam.SlowSendlist[obdcode])
                    {
                        // Return Numeric content (currently switch flag content is not supported)
                        msg_data.val.Add(obdcode.ToString(), elm327com.get_value(obdcode).ToString());
                        msg_data.Validate();
                    }
                }

                if (msg_data.val.Count > 0)
                {
                    String msg = JsonConvert.SerializeObject(msg_data);
                    session.Send(msg);
                }
            }
        }

        private void update_obd_readflag(object stateobj)
        {
            // Do nothing if the running state is false.
            if (!this.running_state)
                return;

            //reset all ssmcom flag
            elm327com.set_all_disable(true);

            if (appServer.SessionCount < 1)
                return;

            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                //set again from the session param read parameter list
                ELM327WebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (ELM327WebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
                {
                    if (sessionparam.FastSendlist[code])
                    {
                        if (!elm327com.get_fastread_flag(code))
                            elm327com.set_fastread_flag(code, true, true);
                    }
                    if (sessionparam.SlowSendlist[code])
                    {
                        if (!elm327com.get_slowread_flag(code))
                            elm327com.set_slowread_flag(code, true, true);
                    }
                }
            }
        }
    }
}
