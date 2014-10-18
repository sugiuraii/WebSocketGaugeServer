using System;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.ClientEngine;
using SuperWebSocket;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FUELTRIP_Logger
{
	public class FUELTRIP_Logger
	{
		private Nenpi_Trip_Calculator _nenpi_trip_calc;
		private WebSocketServer _appServer;
		private WebSocket _deficom_ws_client;
		private WebSocket _ssmcom_ws_client;
		private bool running_state = false;

		private double _current_tacho;
		private double _current_speed;
		private double _current_injpulse_width;

		public int WebsocketServer_ListenPortNo { get; set; }

		public FUELTRIP_Logger(string deficom_WS_URL, string ssmcom_WS_URL)
		{
			_nenpi_trip_calc = new Nenpi_Trip_Calculator ();

			//Websocket server setup
			_appServer = new WebSocketServer ();
			this.WebsocketServer_ListenPortNo = 2012;
			if (!_appServer.Setup(this.WebsocketServer_ListenPortNo)) //Setup with listening port
			{
				Console.WriteLine("Failed to setup!");
			}
			_appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(_appServer_NewMessageReceived);
			_appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (_appServer_NewSessionConnected);
			_appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (_appServer_SessionClosed);

			//deficom ws
			_deficom_ws_client = new WebSocket (deficom_WS_URL);
			_deficom_ws_client.Opened += new EventHandler(_deficom_ws_client_Opened);
			_deficom_ws_client.Error += new EventHandler<ErrorEventArgs>(_deficom_ws_client_Error);
			_deficom_ws_client.Closed += new EventHandler(_deficom_ws_client_Closed);
			_deficom_ws_client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_deficom_ws_client_MessageReceived);
			_ssmcom_ws_client = new WebSocket (ssmcom_WS_URL);
			//ssmcom ws
			_ssmcom_ws_client.Opened += new EventHandler(_deficom_ws_client_Opened);
			_ssmcom_ws_client.Error += new EventHandler<ErrorEventArgs>(_deficom_ws_client_Error);
			_ssmcom_ws_client.Closed += new EventHandler(_deficom_ws_client_Closed);
			_ssmcom_ws_client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_deficom_ws_client_MessageReceived);
		}

		// Websocket server events
		private void _appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
			Console.WriteLine ("Session closed from : " + session.Host + " Reason :" + reason.ToString());
		}
		private void _appServer_NewSessionConnected(WebSocketSession session)
		{
			Console.WriteLine ("New session connected from : " + session.Host);
		}
		private void _appServer_NewMessageReceived(WebSocketSession session, string message)
		{

		}
			
		// deficom WS client event
		private void _deficom_ws_client_Opened(object sender, EventArgs e)
		{
			_deficom_ws_client.Send("Hello World!");
		}
		private void _deficom_ws_client_Error(object sender, EventArgs e)
		{
			_deficom_ws_client.Send("Hello World!");
		}
		private void _deficom_ws_client_Closed(object sender, EventArgs e)
		{
			_deficom_ws_client.Send("Hello World!");
		}
		private void _deficom_ws_client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			_deficom_ws_client.Send("Hello World!");
		}


		// ssmcom WS client event
		private void _ssmcom_ws_client_Opened(object sender, EventArgs e)
		{
			_ssmcom_ws_client.Send("Hello World!");
		}
		private void _ssmcom_ws_client_Error(object sender, EventArgs e)
		{
			_ssmcom_ws_client.Send("Hello World!");
		}
		private void _ssmcom_ws_client_Closed(object sender, EventArgs e)
		{
			_ssmcom_ws_client.Send("Hello World!");
		}
		private void _ssmcom_ws_client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			_ssmcom_ws_client.Send("Hello World!");
		}
	}

}

