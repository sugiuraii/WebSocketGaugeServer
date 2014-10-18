using System;
using System.Threading;
using DefiSSMCOM.Communication.Defi;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace DefiSSMCOM_Websocket
{
	public abstract class JSONFormats
	{
		public string mode;

		public string Serialize ()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	public class JSONFormatsException : Exception
	{
		public JSONFormatsException(){
		}
		public JSONFormatsException(string message): base(message){
		}
		public JSONFormatsException(string message, Exception inner) : base(message) { }
	}

	public class ValueJSONFormat : JSONFormats
	{
		public Dictionary<string,double> val;

		public ValueJSONFormat()
		{
			mode = "VAL";
			val = new Dictionary<string, double> ();
		}
	}
	public class ErrorJSONFormat : JSONFormats
	{
		public ErrorJSONFormat()
		{
			mode = "ERR";
		}
		public string msg;
	}
	public class ResponseJSONFormat : JSONFormats
	{
		public ResponseJSONFormat()
		{
			mode = "RES";
		}
		public string msg;
	}
	public class Defi_WS_SendJSONFormat : JSONFormats
	{
		public Defi_WS_SendJSONFormat()
		{
			mode = "DEFI_WS_SEND";
		}
		[JsonConverter(typeof(StringEnumConverter))]
		public Defi_Parameter_Code code;
		public bool flag;
	}

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
			this.Websocket_PortNo = 2012;
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
			/*
			DefiCOM_Websocket_sessionparam sessionparam = (DefiCOM_Websocket_sessionparam)session.Items ["Param"];

			Console.WriteLine (message);

			var msg_obj = JsonConvert.DeserializeObject<CommandJSONFormat> (message);

			if (msg_obj.mode == "CMD") {
				if (msg_obj.command == "WSReset") {
					sessionparam.reset ();
				} else if (msg_obj.command == "WSEnable") {
				} else if (msg_obj.command == "WSDisable") {
				} else if (msg_obj.command == "WSInverval") {
				} else {
				}
			}
			//Send the received message back
			*/
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
							msg_data.val.Add(deficode.ToString(),deficom1.get_value(deficode));
						}
					}
					String msg = JsonConvert.SerializeObject (msg_data);

					session.Send (msg);
					sendparam.SendCount = 0;
				}
			}
		}
	}
}

