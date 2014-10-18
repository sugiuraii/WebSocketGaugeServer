using System;
using System.Threading;
using DefiSSMCOM.SSM;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace DefiSSMCOM.WebSocket
{
	public class SSMCOM_Websocket_sessionparam
	{
		public SSMCOM_Websocket_sessionparam()
		{
		}

		public void reset()
		{
		}
	}


	public class SSMCOM_Websocket
	{
		private SSMCOM ssmcom1;
		private WebSocketServer appServer;

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
			ssmcom1.SSMDataReceived += new SSMCOMDataReceivedEventHandler (ssmcom1_SSMDataReceived);

			// Create Websocket server
			appServer = new WebSocketServer();
			if (!appServer.Setup(this.Websocket_PortNo)) //Setup with listening port
			{
				Console.WriteLine("Failed to setup!");
			}
			appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
			appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (appServer_NewSessionConnected);
			appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (appServer_SessionClosed);
		}

		public void start()
		{
			ssmcom1.communicate_start();
			//Try to start the appServer
			if (!appServer.Start())
			{
				Console.WriteLine("Failed to start!");
				Console.ReadKey();
				return;
			}

			this.running_state = true;
		}

		public void stop ()
		{
			if (!this.running_state) {
				Console.WriteLine ("Websocket server is not running");
				return;
			}
			//Stop the appServer
			appServer.Stop();

			Console.WriteLine();
			Console.WriteLine("The server was stopped!");
			Console.ReadKey();

			ssmcom1.communicate_stop ();
		}

		private void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
			Console.WriteLine ("Session closed from : " + session.Host + " Reason :" + reason.ToString());
		}

		private void appServer_NewSessionConnected(WebSocketSession session)
		{
			Console.WriteLine ("New session connected from : " + session.Host);
			SSMCOM_Websocket_sessionparam sendparam = new SSMCOM_Websocket_sessionparam ();
			session.Items.Add ("Param", sendparam);
		}
			

		private void appServer_NewMessageReceived(WebSocketSession session, string message)
		{

			SSMCOM_Websocket_sessionparam sessionparam = (SSMCOM_Websocket_sessionparam)session.Items ["Param"];
			//Console.WriteLine (message);

			if (message == "") {
				send_error_msg (session, "Empty message is received.");
				return;
			}
			string received_JSON_mode;
			try{
				var msg_dict = JsonConvert.DeserializeObject<Dictionary<string,string>> (message);
				received_JSON_mode = msg_dict ["mode"];
			}
			catch( KeyNotFoundException ex) {
				send_error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch (JsonException ex) {
				send_error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}

			try
			{
				switch(received_JSON_mode)
				{
				//SSM COM all reset
				case ("RESET"):
					ssmcom1.set_all_disable();
					send_response_msg(session, "SSMCOM RESET. All parameters are disabled.");
					break;
				case ("SSM_COM_READ"):
					SSM_COM_ReadJSONFormat msg_obj_ssmread = JsonConvert.DeserializeObject<SSM_COM_ReadJSONFormat>(message);
					msg_obj_ssmread.Validate();

					SSM_Parameter_Code target_code = (SSM_Parameter_Code)Enum.Parse(typeof(SSM_Parameter_Code),msg_obj_ssmread.code);
					bool flag = msg_obj_ssmread.flag;

					if(msg_obj_ssmread.read_mode == "FAST"){
						ssmcom1.set_fastread_flag(target_code,flag);
					}
					else{
						ssmcom1.set_slowread_flag(target_code,flag);
					}
						
					send_response_msg(session, "SSMCOM read flag for : " + target_code.ToString() + " set to : " + flag.ToString());
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
			catch(JSONFormatsException ex) {
				send_error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch(JsonException ex) {
				send_error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}

		}

		private void ssmcom1_SSMDataReceived(object sender,SSMCOMDataReceivedEventArgs args)
		{
			var sessions = appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				ValueJSONFormat msg_data = new ValueJSONFormat ();

				//SSMCOM_Websocket_sessionparam sendparam = (SSMCOM_Websocket_sessionparam)session.Items["Param"];

				foreach (SSM_Parameter_Code ssmcode in args.Received_Parameter_Code) 
				{
					// Return Switch content
					if (ssmcode >= SSM_Parameter_Code.Switch_P0x061 && ssmcode <= SSM_Parameter_Code.Switch_P0x121) {
						List<SSM_Switch_Code> switch_code_list = SSM_Content_Table.get_Switchcodes_from_Parametercode (ssmcode);
						foreach (SSM_Switch_Code switch_code in switch_code_list) {
							msg_data.val.Add (switch_code.ToString (), ssmcom1.get_switch (switch_code).ToString ());
						}
					}
					// Return Numeric content
					else {
						msg_data.val.Add(ssmcode.ToString(),ssmcom1.get_value(ssmcode).ToString());
					}
					msg_data.Validate ();

					String msg = JsonConvert.SerializeObject (msg_data);

					session.Send (msg);
				} 
			}
		}

		private void send_error_msg(WebSocketSession session,string message)
		{
			ErrorJSONFormat json_error_msg = new ErrorJSONFormat ();
			json_error_msg.msg = message;

			session.Send (json_error_msg.Serialize());
			Console.WriteLine ("Error:"+message);
		}

		private void send_response_msg(WebSocketSession session,string message)
		{
			ResponseJSONFormat json_response_msg = new ResponseJSONFormat ();
			json_response_msg.msg = message;
			session.Send (json_response_msg.Serialize());

			Console.WriteLine ("Response:"+message);
		}

	}
}

