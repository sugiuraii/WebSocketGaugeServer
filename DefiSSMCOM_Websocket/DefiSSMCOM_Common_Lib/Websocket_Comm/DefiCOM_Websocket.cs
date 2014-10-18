using System;
using System.Threading;
using DefiSSMCOM.Defi;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace DefiSSMCOM.WebSocket
{
	public class DefiCOM_Websocket_sessionparam
	{
		public Dictionary<Defi_Parameter_Code,bool> Sendlist;
		public int SendInterval;
		public int SendCount;

		public DefiCOM_Websocket_sessionparam()
		{
			this.Sendlist = new Dictionary<Defi_Parameter_Code,bool> ();

			foreach (Defi_Parameter_Code code in Enum.GetValues(typeof(Defi_Parameter_Code)))
			{
				this.Sendlist.Add(code, false);
			}

			this.SendInterval = 0;
			this.SendCount = 0;
		}

		public void reset()
		{
			foreach (Defi_Parameter_Code code in Enum.GetValues(typeof(Defi_Parameter_Code)))
			{
				this.Sendlist[code]= false;
			}

			this.SendInterval = 0;
			this.SendCount = 0;
		}
	}
		
	public class DefiCOM_Websocket
	{
		private DefiCOM deficom1;
		private WebSocketServer appServer;

		private bool running_state = false;

		public int Websocket_PortNo { get; set; }
		public string DefiCOM_PortName
		{
			get
			{
				return deficom1.PortName;
			}
			set
			{
				deficom1.PortName = value;
			}
		}

		public DefiCOM_Websocket ()
		{
			// Create Deficom
			deficom1 = new DefiCOM ();
			this.Websocket_PortNo = 2013;
			this.DefiCOM_PortName = "COM1";
			deficom1.DefiLinkPacketReceived += new EventHandler (deficom1_DefiLinkPacketReceived);

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
			deficom1.communicate_realtime_start();
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

			deficom1.communicate_realtime_stop ();
		}

		private void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
			Console.WriteLine ("Session closed from : " + session.Host + " Reason :" + reason.ToString());
		}

		private void appServer_NewSessionConnected(WebSocketSession session)
		{
			Console.WriteLine ("New session connected from : " + session.Host);
			DefiCOM_Websocket_sessionparam sendparam = new DefiCOM_Websocket_sessionparam ();
			session.Items.Add ("Param", sendparam);
		}
			

		private void appServer_NewMessageReceived(WebSocketSession session, string message)
		{

			DefiCOM_Websocket_sessionparam sessionparam = (DefiCOM_Websocket_sessionparam)session.Items ["Param"];
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
				case ("RESET"):
					sessionparam.reset();
					send_response_msg(session, "Defi Websocket all parameter reset.");
					break;
				case ("DEFI_WS_SEND"):
					Defi_WS_SendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<Defi_WS_SendJSONFormat>(message);
					msg_obj_wssend.Validate();
					sessionparam.Sendlist[(Defi_Parameter_Code)Enum.Parse(typeof(Defi_Parameter_Code), msg_obj_wssend.code)]=msg_obj_wssend.flag;

					send_response_msg(session, "Defi Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
					break;

				case ("DEFI_WS_INTERVAL"):
					Defi_WS_IntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<Defi_WS_IntervalJSONFormat>(message);
					msg_obj_interval.Validate();
					sessionparam.SendInterval = msg_obj_interval.interval;

					send_response_msg(session, "Defi Websocket send_interval to : " + msg_obj_interval.interval.ToString());
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

		private void deficom1_DefiLinkPacketReceived(object sender,EventArgs args)
		{
			var sessions = appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				ValueJSONFormat msg_data = new ValueJSONFormat ();

				DefiCOM_Websocket_sessionparam sendparam = (DefiCOM_Websocket_sessionparam)session.Items["Param"];
				if (sendparam.SendCount < sendparam.SendInterval)
					sendparam.SendCount++;
				else {
					foreach (Defi_Parameter_Code deficode in Enum.GetValues(typeof(Defi_Parameter_Code) )) {
						if (sendparam.Sendlist [deficode]) {
							msg_data.val.Add(deficode.ToString(),deficom1.get_value(deficode).ToString());
						}
					}
					String msg = JsonConvert.SerializeObject (msg_data);

					session.Send (msg);
					sendparam.SendCount = 0;
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

