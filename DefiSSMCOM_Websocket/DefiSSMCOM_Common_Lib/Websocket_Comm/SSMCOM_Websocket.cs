using System;
using System.Threading;
using DefiSSMCOM.SSM;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace DefiSSMCOM.WebSocket
{
	public class SSMCOM_Websocket_sessionparam
	{
        public Dictionary<SSM_Parameter_Code, bool> SlowSendlist,FastSendlist;
		public SSMCOM_Websocket_sessionparam()
		{
            this.SlowSendlist = new Dictionary<SSM_Parameter_Code, bool>();
            this.FastSendlist = new Dictionary<SSM_Parameter_Code, bool>();

            foreach (SSM_Parameter_Code code in Enum.GetValues(typeof(SSM_Parameter_Code)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
		}

		public void reset()
		{
            foreach (SSM_Parameter_Code code in Enum.GetValues(typeof(SSM_Parameter_Code)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
		}
	}


	public class SSMCOM_Websocket
	{
		private SSMCOM ssmcom1;
		private WebSocketServer appServer;

        private Timer update_ssmflag_timer;

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool running_state = false;

		public int Websocket_PortNo { get; set; }
		public string SSMCOM_PortName
		{
			get
			{
				return ssmcom1.PortName;
			}
			set
			{
				ssmcom1.PortName = value;
			}
		}

		public SSMCOM_Websocket ()
		{
			// Create Deficom
			ssmcom1 = new SSMCOM ();
			this.Websocket_PortNo = 2012;
			this.SSMCOM_PortName = "COM1";
			ssmcom1.SSMDataReceived += new EventHandler<SSMCOMDataReceivedEventArgs> (ssmcom1_SSMDataReceived);

			// Create Websocket server
			appServer = new WebSocketServer();

			appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
			appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (appServer_NewSessionConnected);
			appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (appServer_SessionClosed);

            update_ssmflag_timer = new Timer(new TimerCallback(update_ssmcom_readflag), null, 0, Timeout.Infinite);
		}

		public void start()
		{
            SuperSocket.SocketBase.Config.ServerConfig appserver_config = new SuperSocket.SocketBase.Config.ServerConfig();
            appserver_config.DisableSessionSnapshot = false;
            appserver_config.SessionSnapshotInterval = 2;
            appserver_config.Port = this.Websocket_PortNo;

			//Try to start the appServer
            if (!appServer.Setup(appserver_config)) //Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                logger.Fatal("Failed to setup websocket server.");
            }
			if (!appServer.Start())
			{
                Console.WriteLine("Failed to start!");
                logger.Fatal("Failed to start websocket server.");
                return;
			}
            Console.WriteLine("Websocket server is started. WebsocketPort:" + this.Websocket_PortNo.ToString() + " SSMCOMPort: " + this.SSMCOM_PortName);
            logger.Info("Websocket server is started. WebsocketPort:" + this.Websocket_PortNo.ToString() + " SSMCOMPort: " + this.SSMCOM_PortName);

            ssmcom1.communicate_start();

            update_ssmflag_timer.Change(0, 2000);

			this.running_state = true;
		}

		public void stop ()
		{
			if (!this.running_state) {
                Console.WriteLine("Websocket server is not running");
                logger.Error("Websocket stop is called. But the websocket server is not running.");
                return;
			}
			//Stop the appServer
			appServer.Stop();

			Console.WriteLine();
            Console.WriteLine("The server was stopped!");
            logger.Info("Websocket server is stopped");

			ssmcom1.communicate_stop ();
		}

		private void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
            Console.WriteLine("Session closed from : " + session.Host + " Reason :" + reason.ToString());
            logger.Info("Session closed from : " + session.Host + " Reason :" + reason.ToString());
        }

		private void appServer_NewSessionConnected(WebSocketSession session)
		{
            SSMCOM_Websocket_sessionparam sendparam = new SSMCOM_Websocket_sessionparam();
            session.Items.Add("Param", sendparam);
                
            Console.WriteLine("New session connected from : " + session.Host);
            logger.Info("New session connected from : " + session.Host);
		}
			

		private void appServer_NewMessageReceived(WebSocketSession session, string message)
		{
            SSMCOM_Websocket_sessionparam sessionparam;
            try
            {
                sessionparam = (SSMCOM_Websocket_sessionparam)session.Items["Param"];
                //Console.WriteLine (message);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                return;
            }


            if (message == "")
            {
                send_error_msg(session, "Empty message is received.");
                return;
            }
            string received_JSON_mode;
            try
            {
                var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                received_JSON_mode = msg_dict["mode"];
            }
            catch (KeyNotFoundException ex)
            {
                send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                return;
            }
            catch (JsonException ex)
            {
                send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                return;
            }

            try
            {
                switch (received_JSON_mode)
                {
                    //SSM COM all reset
                    case ("RESET"):
                        sessionparam.reset();
                        send_response_msg(session, "SSMCOM session RESET. All send parameters are disabled.");
                        break;
                    case ("SSM_COM_READ"):
                        SSM_COM_ReadJSONFormat msg_obj_ssmread = JsonConvert.DeserializeObject<SSM_COM_ReadJSONFormat>(message);
                        msg_obj_ssmread.Validate();

                        SSM_Parameter_Code target_code = (SSM_Parameter_Code)Enum.Parse(typeof(SSM_Parameter_Code), msg_obj_ssmread.code);
                        bool flag = msg_obj_ssmread.flag;

                        if (msg_obj_ssmread.read_mode == "FAST")
                        {
                            sessionparam.FastSendlist[target_code] = flag;
                        }
                        else
                        {
                            sessionparam.SlowSendlist[target_code] = flag;
                        }
                        send_response_msg(session, "SSMCOM session read flag for : " + target_code.ToString() + " read_mode :" + msg_obj_ssmread.read_mode + " set to : " + flag.ToString());
                        break;

                    case ("SSM_SLOWREAD_INTERVAL"):
                        SSM_SLOWREAD_IntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<SSM_SLOWREAD_IntervalJSONFormat>(message);
                        msg_obj_interval.Validate();
                        ssmcom1.Slow_Read_Interval = msg_obj_interval.interval;

                        send_response_msg(session, "SSMCOM slowread interval to : " + msg_obj_interval.interval.ToString());
                        break;
                    default:
                        throw new JSONFormatsException("Unsuppoted mode property.");
                }
            }
            catch (JSONFormatsException ex)
            {
                send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                return;
            }
            catch (JsonException ex)
            {
                send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                return;
            }
		}

		private void ssmcom1_SSMDataReceived(object sender,SSMCOMDataReceivedEventArgs args)
		{   
			var sessions = appServer.GetAllSessions ();
            
			foreach (var session in sessions) 
			{
                if (session == null)
                    continue;

				ValueJSONFormat msg_data = new ValueJSONFormat ();
                SSMCOM_Websocket_sessionparam sendparam;
                try
                {
                    sendparam = (SSMCOM_Websocket_sessionparam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

				foreach (SSM_Parameter_Code ssmcode in args.Received_Parameter_Code) 
				{
                    if (sendparam.FastSendlist[ssmcode] || sendparam.SlowSendlist[ssmcode])
                    {
                        // Return Switch content
                        if (ssmcode >= SSM_Parameter_Code.Switch_P0x061 && ssmcode <= SSM_Parameter_Code.Switch_P0x121)
                        {
                            List<SSM_Switch_Code> switch_code_list = SSM_Content_Table.get_Switchcodes_from_Parametercode(ssmcode);
                            foreach (SSM_Switch_Code switch_code in switch_code_list)
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
            //reset all ssmcom flag
            ssmcom1.set_all_disable();
            
            if (appServer.SessionCount < 1)
                return;

            var sessions = appServer.GetAllSessions ();

            foreach (var session in sessions)
            {
                if (session == null)
                    continue;

                //set again from the session param read parameter list
                SSMCOM_Websocket_sessionparam sessionparam;
                try
                {
                    sessionparam = (SSMCOM_Websocket_sessionparam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                foreach (SSM_Parameter_Code code in Enum.GetValues(typeof(SSM_Parameter_Code)))
                {
                    if (sessionparam.FastSendlist[code])
                    {
                        if (!ssmcom1.get_fastread_flag(code))
                            ssmcom1.set_fastread_flag(code, true);
                    }
                    if (sessionparam.SlowSendlist[code])
                    {
                        if (!ssmcom1.get_slowread_flag(code))
                            ssmcom1.set_slowread_flag(code, true);
                    }
                }
            }
        }

		private void send_error_msg(WebSocketSession session,string message)
		{
			ErrorJSONFormat json_error_msg = new ErrorJSONFormat ();
			json_error_msg.msg = message;

			session.Send (json_error_msg.Serialize());
            Console.WriteLine("Send Error message to " + session.Host + " : " + message);
            logger.Error("Send Error message to " + session.Host + " : " + message);
        }

		private void send_response_msg(WebSocketSession session,string message)
		{
			ResponseJSONFormat json_response_msg = new ResponseJSONFormat ();
			json_response_msg.msg = message;
			session.Send (json_response_msg.Serialize());

            Console.WriteLine("Send Response message to " + session.Host + " : " + message);
            logger.Info("Send Response message to " + session.Host + " : " + message);
        }

	}
}

