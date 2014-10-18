using System;
using System.Threading;
using DefiSSMCOM.Communication.Defi;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections;

namespace DefiLinkTest1
{
	class JSONFormats
	{
		public class ValueJSONFormat
		{
			public string mode = "VAL";
			public ArrayList val;
		}
		public class ErrorJSONFormat
		{
			public string mode = "ERR";
			public string msg;
		}
		public class ResponseJSONFormat
		{
			public string mode = "RES";
			public string msg;
		}
	}

	class MainClass
	{
		static private DefiCOM deficom1;
		static private WebSocketServer appServer;

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
			deficom1 = new DefiCOM ();

			deficom1.PortName = "COM1";

			deficom1.communicate_realtime_start ();

			deficom1.DefiLinkPacketReceived += new EventHandler (deficom1_DefiLinkPacketReceived);

			// --------------------------------- WebSocket
			appServer = new WebSocketServer();
			//Setup the appServer
			if (!appServer.Setup(2012)) //Setup with listening port
			{
				Console.WriteLine("Failed to setup!");
				Console.ReadKey();
				return;
			}

			appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
			appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (appServer_NewSessionConnected);
			appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (appServer_SessionClosed);

			Console.WriteLine();

			//Try to start the appServer
			if (!appServer.Start())
			{
				Console.WriteLine("Failed to start!");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("The server started successfully, press key 'q' to stop it!");

			while (Console.ReadKey().KeyChar != 'q')
			{
				Console.WriteLine();
				continue;
			}

			//Stop the appServer
			appServer.Stop();

			Console.WriteLine();
			Console.WriteLine("The server was stopped!");
			Console.ReadKey();

			// -----------------------------------------------------------------------------

			deficom1.communicate_realtime_stop ();
		}

		static void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
			Console.WriteLine ("Session closed from : " + session.Host + " Reason :" + reason.ToString());
		}

		static void appServer_NewSessionConnected(WebSocketSession session)
		{
			Console.WriteLine ("New session connected from : " + session.Host);
		}

		static void appServer_NewMessageReceived(WebSocketSession session, string message)
		{
			//Send the received message back
			session.Send("Server: " + message);
			Console.WriteLine (message);
		}

		static void deficom1_DefiLinkPacketReceived(object sender,EventArgs args)
		{
			var sessions = appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				JSONFormats.ValueJSONFormat msg_data = new JSONFormats.ValueJSONFormat ();
				msg_data.val = new ArrayList ();

				foreach (Defi_Parameter_Code deficode in Enum.GetValues(typeof(Defi_Parameter_Code) ) )
				{
					string[] val_packet = new string[2];

					val_packet[0] = deficode.ToString();
					val_packet[1] = deficom1.get_value(deficode).ToString();
					msg_data.val.Add(val_packet);
				}
				String msg = JsonConvert.SerializeObject (msg_data);
				session.Send (msg);
			}
		}
	}
}
