using System;
using System.Threading;
using DefiSSMCOM.WebSocket.JSON;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket
{
	public class SSMCOMWebsocketSessionParam : WebsocketSessionParam
	{
        public Dictionary<SSMParameterCode, bool> SlowSendlist,FastSendlist;
		public SSMCOMWebsocketSessionParam()
		{
            this.SlowSendlist = new Dictionary<SSMParameterCode, bool>();
            this.FastSendlist = new Dictionary<SSMParameterCode, bool>();

            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
		}

		public override void reset()
		{
            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
		}
	}


	public class SSMCOMWebsocket : WebSocketCommon
	{
		private SSMCOM ssmcom1;

        private Timer update_ssmflag_timer;

		public SSMCOMWebsocket ()
		{
			// Create Deficom
			ssmcom1 = new SSMCOM ();
            com1 = ssmcom1;
			WebsocketPortNo = 2012;
			COMPortName = "COM1";
			ssmcom1.SSMDataReceived += new EventHandler<SSMCOMDataReceivedEventArgs> (ssmcom1_SSMDataReceived);

            update_ssmflag_timer = new Timer(new TimerCallback(update_ssmcom_readflag), null, 0, Timeout.Infinite);
            update_ssmflag_timer.Change(0, 2000);
		}

        protected override WebsocketSessionParam createSessionParam()
        {
            return new SSMCOMWebsocketSessionParam();
        }

        protected override void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session)
        {
            var sessionparam = (SSMCOMWebsocketSessionParam)session.Items["Param"];
            switch (receivedJSONmode)
            {
                //SSM COM all reset
                case (ResetJSONFormat.ModeCode):
                    sessionparam.reset();
                    send_response_msg(session, "SSMCOM session RESET. All send parameters are disabled.");
                    break;
                case (SSMCOMReadJSONFormat.ModeCode):
                    SSMCOMReadJSONFormat msg_obj_ssmread = JsonConvert.DeserializeObject<SSMCOMReadJSONFormat>(message);
                    msg_obj_ssmread.Validate();

                    SSMParameterCode target_code = (SSMParameterCode)Enum.Parse(typeof(SSMParameterCode), msg_obj_ssmread.code);
                    bool flag = msg_obj_ssmread.flag;

                    if (msg_obj_ssmread.read_mode == SSMCOMReadJSONFormat.FastReadModeCode)
                    {
                        sessionparam.FastSendlist[target_code] = flag;
                    }
                    else
                    {
                        sessionparam.SlowSendlist[target_code] = flag;
                    }
                    send_response_msg(session, "SSMCOM session read flag for : " + target_code.ToString() + " read_mode :" + msg_obj_ssmread.read_mode + " set to : " + flag.ToString());
                    break;

                case (SSMSLOWREADIntervalJSONFormat.ModeCode):
                    SSMSLOWREADIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<SSMSLOWREADIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    ssmcom1.SlowReadInterval = msg_obj_interval.interval;

                    send_response_msg(session, "SSMCOM slowread interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }

		private void ssmcom1_SSMDataReceived(object sender,SSMCOMDataReceivedEventArgs args)
		{   
			var sessions = appServer.GetAllSessions ();
            
			foreach (var session in sessions) 
			{
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                SSMCOMWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (SSMCOMWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Fatal("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                ValueJSONFormat msg_data = new ValueJSONFormat();
				foreach (SSMParameterCode ssmcode in args.Received_Parameter_Code) 
				{
                    if (sessionparam.FastSendlist[ssmcode] || sessionparam.SlowSendlist[ssmcode])
                    {
                        // Return Switch content
                        if (ssmcode >= SSMParameterCode.Switch_P0x061 && ssmcode <= SSMParameterCode.Switch_P0x121)
                        {
                            List<SSMSwitchCode> switch_code_list = SSMContentTable.get_Switchcodes_from_Parametercode(ssmcode);
                            foreach (SSMSwitchCode switch_code in switch_code_list)
                            {
                                msg_data.val.Add(switch_code.ToString(), ssmcom1.get_switch(switch_code).ToString());
                            }
                        }
                        // Return Numeric content
                        else
                        {
                            msg_data.val.Add(ssmcode.ToString(), ssmcom1.get_value(ssmcode).ToString());
                        }
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

        private void update_ssmcom_readflag(object stateobj)
        {
            // Do nothing if the running state is false.
            if (!this.running_state)
                return;

            //reset all ssmcom flag
            ssmcom1.set_all_disable(true);
            
            if (appServer.SessionCount < 1)
                return;

            var sessions = appServer.GetAllSessions ();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                //set again from the session param read parameter list
                SSMCOMWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (SSMCOMWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
                {
                    if (sessionparam.FastSendlist[code])
                    {
                        if (!ssmcom1.get_fastread_flag(code))
                            ssmcom1.set_fastread_flag(code, true, true);
                    }
                    if (sessionparam.SlowSendlist[code])
                    {
                        if (!ssmcom1.get_slowread_flag(code))
                            ssmcom1.set_slowread_flag(code, true, true);
                    }
                }
            }
        }
	}
}

