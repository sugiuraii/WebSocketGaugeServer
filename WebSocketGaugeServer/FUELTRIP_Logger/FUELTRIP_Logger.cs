using System;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.ClientEngine;
using SuperWebSocket;
using System.Net;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using DefiSSMCOM;
using DefiSSMCOM.Defi;
using DefiSSMCOM.SSM;
using DefiSSMCOM.Arduino;
using DefiSSMCOM.OBDII;
using DefiSSMCOM.WebSocket.JSON;
using DefiSSMCOM.WebSocket;
using log4net;

namespace FUELTRIP_Logger
{
	public class FUELTRIPLogger
	{
		private const int CONNECT_RETRY_SEC = 5;
        private const int DEFIPACKET_INTERVAL = 2;
		private enum SSM_DEFI_mode{
			Defi,
			SSM
		};

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private FuelTripCalculator fuelTripCalc;
		private WebSocketServer appServer;
		private WebSocket deficomWSClient;
		private WebSocket ssmcomWSClient;
		private bool running_state = false;

		private double currentEngineRev;
		private double currentVehicleSpeed;
		private double currentInjPulseWidth;

        /// <summary>
        /// Interval of sending keep alive dummy message in millisecond.
        /// </summary>
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// Porn No to listen connection.
        /// </summary>
		public int WebsocketServer_ListenPortNo { get; set; }

        /// <summary>
        /// Constructor of FUELTRIP_Logger
        /// </summary>
        /// <param name="deficom_WS_URL">Deficom websocket server URL.</param>
        /// <param name="ssmcom_WS_URL">SSMCOM websocket server URL.</param>
		public FUELTRIPLogger(AppSettings appSettings)
		{
            this.WebsocketServer_ListenPortNo = appSettings.websocket_port;

			fuelTripCalc = new FuelTripCalculator(appSettings.Calculation.CalculationOption, appSettings.Calculation.FuelCalculationMethod);
            
            // Default KeepAliveInterval : 60ms
            this.KeepAliveInterval = appSettings.keepalive_interval;

			//Websocket server setup
			appServer = new WebSocketServer ();
			appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(_appServer_NewMessageReceived);
			appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (_appServer_NewSessionConnected);
			appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (_appServer_SessionClosed);

			//deficom ws
			deficomWSClient = new WebSocket (appSettings.defiserver_url);
			deficomWSClient.Opened += new EventHandler(_deficom_ws_client_Opened);
			deficomWSClient.Error += new EventHandler<ErrorEventArgs>(_deficom_ws_client_Error);
			deficomWSClient.Closed += new EventHandler(_deficom_ws_client_Closed);
			deficomWSClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_deficom_ws_client_MessageReceived);
            //ssmcom ws
            ssmcomWSClient = new WebSocket (appSettings.ssmserver_url);
			ssmcomWSClient.Opened += new EventHandler(_ssmcom_ws_client_Opened);
			ssmcomWSClient.Error += new EventHandler<ErrorEventArgs>(_ssmcom_ws_client_Error);
			ssmcomWSClient.Closed += new EventHandler(_ssmcom_ws_client_Closed);
			ssmcomWSClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_ssmcom_ws_client_MessageReceived);
			running_state = false;

            fuelTripCalc.SectFUELTRIPUpdated += new EventHandler(_nenpi_trip_calc_SectFUELTRIPUpdated);
		}

        /// <summary>
        /// Start server instance.
        /// </summary>
		public void start()
		{
			running_state = true;

            SuperSocket.SocketBase.Config.ServerConfig appserver_config = new SuperSocket.SocketBase.Config.ServerConfig();
            appserver_config.DisableSessionSnapshot = false;
            appserver_config.SessionSnapshotInterval = 1;
            appserver_config.Port = this.WebsocketServer_ListenPortNo;

            //Start Websocket server
            if (!appServer.Setup(appserver_config)) //Setup with listening port
            {
                //Console.WriteLine("Failed to setup!");
                logger.Fatal("Failed to setup websocket server.");
            }
            if (!appServer.Start())
            {
                //Console.WriteLine("Failed to start!");
                logger.Fatal("Failed to start websocket server.");
                return;
            }

            fuelTripCalc.loadTripFuel();
            deficomWSClient.Open();
            ssmcomWSClient.Open();

            //Console.WriteLine("Websocket server is starting... DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
            logger.Info("Websocket server is starting... ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
        }

        /// <summary>
        /// Stop server instance.
        /// </summary>
		public void stop ()
		{
			running_state = false;
			fuelTripCalc.saveTripFuel ();
            if(deficomWSClient.State == WebSocketState.Open)
    			deficomWSClient.Close ();
            if(ssmcomWSClient.State == WebSocketState.Open)
    			ssmcomWSClient.Close ();
            if(appServer.State == ServerState.Running)
                appServer.Stop ();

            //Console.WriteLine("The server was stopped!");
            logger.Info("Websocket server is stopped");

		}

		// Websocket server events
		private void _appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
            // Stop keepalive message timer
            KeepAliveDMYMsgTimer keepaliveMsgTimer = (KeepAliveDMYMsgTimer)session.Items["KeepAliveTimer"];
            keepaliveMsgTimer.Stop();

            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Session closed from : " + destinationAddress.ToString() + " Reason :" + reason.ToString());
        }
		private void _appServer_NewSessionConnected(WebSocketSession session)
		{
            KeepAliveDMYMsgTimer keepAliveMsgTimer = new KeepAliveDMYMsgTimer(session, this.KeepAliveInterval);
            keepAliveMsgTimer.Start();
            session.Items.Add("KeepAliveTimer", keepAliveMsgTimer);

            IPAddress destinationAddress = session.RemoteEndPoint.Address; 
            logger.Info("New session connected from : " + destinationAddress.ToString());
        }
		private void _appServer_NewMessageReceived(WebSocketSession session, string message)
		{
			string received_JSON_mode;
			try{
				var msg_dict = JsonConvert.DeserializeObject<Dictionary<string,string>> (message);
				received_JSON_mode = msg_dict ["mode"];
			}
			catch( KeyNotFoundException ex) {
				error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch (JsonException ex) {
				error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}

			try
			{
				switch(received_JSON_mode)
				{
				//SSM COM all reset
				case (ResetJSONFormat.ModeCode):
					fuelTripCalc.resetSectTripFuel();
					fuelTripCalc.resetTotalTripFuel();
					response_msg(session, "NenpiCalc AllRESET.");
					break;
				case (SectResetJSONFormat.ModeCode):
					fuelTripCalc.resetSectTripFuel();
					response_msg(session, "NenpiCalc SectRESET.");
					break;
				case (SectSpanJSONFormat.ModeCode):
					SectSpanJSONFormat span_jsonobj = JsonConvert.DeserializeObject<SectSpanJSONFormat>(message);
					span_jsonobj.Validate();
					fuelTripCalc.SectSpan = span_jsonobj.sect_span*1000;
					response_msg(session, "NenpiCalc SectSpan Set to : " + span_jsonobj.sect_span.ToString() + "sec");
					break;
				case (SectStoreMaxJSONFormat.ModeCode):
					SectStoreMaxJSONFormat storemax_jsonobj = JsonConvert.DeserializeObject<SectStoreMaxJSONFormat>(message);
					storemax_jsonobj.Validate();
					fuelTripCalc.SectStoreMax = storemax_jsonobj.storemax;
					response_msg(session, "NenpiCalc SectStoreMax Set to : " + storemax_jsonobj.storemax.ToString());
					break;
				default:
					throw new JSONFormatsException("Unsuppoted mode property.");
				}
			}
			catch(JSONFormatsException ex) {
				error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch(JsonException ex) {
				error_msg (session, ex.GetType().ToString() + " " + ex.Message);
				return;
			}
		}

		// Error message method
		private void error_msg(WebSocketSession session,string message)
		{
            //Console.WriteLine("Error message " + " : " + message);
            logger.Error("Send Error message " + " : " + message);
            ErrorJSONFormat errormsg_json = new ErrorJSONFormat();
			errormsg_json.msg = message;

			session.Send (errormsg_json.Serialize ());
		}

		private void response_msg(WebSocketSession session, string message)
		{
            //Console.WriteLine("Response message " + " : " + message);
            logger.Info("Send Response message " + " : " + message);
            ResponseJSONFormat resmsg_json = new ResponseJSONFormat();
			resmsg_json.msg = message;

			session.Send (resmsg_json.Serialize ());
		}

		private void send_momentum_value()
		{
			FUELTRIPJSONFormat fueltrip_json = new FUELTRIPJSONFormat ();
			fueltrip_json.moment_gasmilage = fuelTripCalc.MomentaryTripPerFuel;
			fueltrip_json.total_gas = fuelTripCalc.TotalFuelConsumption;
			fueltrip_json.total_trip = fuelTripCalc.TotalTrip;
			fueltrip_json.total_gasmilage = fuelTripCalc.TotalTripPerFuel;


            broadcast_websocket_msg(fueltrip_json.Serialize());
		}

		private void send_section_value_array()
		{
			SectFUELTRIPJSONFormat sectfueltrip_json = new SectFUELTRIPJSONFormat ();
			sectfueltrip_json.sect_gas = fuelTripCalc.SectFuelArray;
			sectfueltrip_json.sect_trip = fuelTripCalc.SectTripArray;
			sectfueltrip_json.sect_gasmilage = fuelTripCalc.SectTripPerFuelArray;
			sectfueltrip_json.sect_span = fuelTripCalc.SectSpan;

            broadcast_websocket_msg(sectfueltrip_json.Serialize());

		}

        // Broadcast : Send message to all active sessions
        private void broadcast_websocket_msg(string message)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;
                session.Send(message);
            }
        }



        private void _nenpi_trip_calc_SectFUELTRIPUpdated(object sender, EventArgs e)
        {
            send_section_value_array();
        }
	}
}

