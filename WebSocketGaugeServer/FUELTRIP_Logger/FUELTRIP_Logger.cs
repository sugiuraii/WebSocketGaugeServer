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

		private readonly FuelTripCalculator fuelTripCalc;
		private readonly WebSocketServer appServer;
        private readonly WebSocketClients websocketClients;
		private bool running_state = false;

		private double currentEngineRev;
		private double currentVehicleSpeed;
		private double currentInjPulseWidth;
        private double currentMassAirFlow;
        private double currentAFRatio;
        private double currentFuelRate;

        /// <summary>
        /// Interval of sending keep alive dummy message in millisecond.
        /// </summary>
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// Porn No to listen connection.
        /// </summary>
		public int WebsocketServerListenPortNo { get; set; }

        /// <summary>
        /// Constructor of FUELTRIP_Logger
        /// </summary>
        /// <param name="deficom_WS_URL">Deficom websocket server URL.</param>
        /// <param name="ssmcom_WS_URL">SSMCOM websocket server URL.</param>
		public FUELTRIPLogger(AppSettings appSettings)
		{
            this.WebsocketServerListenPortNo = appSettings.websocket_port;

			this.fuelTripCalc = new FuelTripCalculator(appSettings.Calculation.CalculationOption, appSettings.Calculation.FuelCalculationMethod);
            
            // Default KeepAliveInterval : 60ms
            this.KeepAliveInterval = appSettings.keepalive_interval;

			//Websocket server setup
            this.appServer = this.initializeAppServer();

			//Websocket clients setup
            this.websocketClients = new WebSocketClients(appSettings);

			running_state = false;

            fuelTripCalc.SectFUELTRIPUpdated += new EventHandler(_nenpi_trip_calc_SectFUELTRIPUpdated);
		}

        /// <summary>
        /// Start server instance.
        /// </summary>
		public void start()
		{
			running_state = true;

            SuperSocket.SocketBase.Config.ServerConfig appserverConfig = new SuperSocket.SocketBase.Config.ServerConfig();
            appserverConfig.DisableSessionSnapshot = false;
            appserverConfig.SessionSnapshotInterval = 1;
            appserverConfig.Port = this.WebsocketServerListenPortNo;

            if (!appServer.Setup(appserverConfig))
            {
                logger.Fatal("Failed to setup websocket server.");
            }
            if (!appServer.Start())
            {
                logger.Fatal("Failed to start websocket server.");
                return;
            }

            this.fuelTripCalc.loadTripFuel();

            this.websocketClients.start();

            //Console.WriteLine("Websocket server is starting... DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
            logger.Info("Websocket server is starting... ListenPort: " + this.WebsocketServerListenPortNo.ToString());
        }

        /// <summary>
        /// Stop server instance.
        /// </summary>
		public void stop ()
		{
			running_state = false;
			fuelTripCalc.saveTripFuel ();

            this.websocketClients.stop();

            if(appServer.State == ServerState.Running)
                appServer.Stop ();

            logger.Info("Websocket server is stopped");

		}

        /// <summary>
        /// Initialize appserver with defining events.
        /// </summary>
        /// <returns>Initialized appserver.</returns>
        private WebSocketServer initializeAppServer()
        {
            WebSocketServer server = new WebSocketServer();
            server.NewSessionConnected += (session) =>
            {
                KeepAliveDMYMsgTimer keepAliveMsgTimer = new KeepAliveDMYMsgTimer(session, this.KeepAliveInterval);
                keepAliveMsgTimer.Start();
                session.Items.Add("KeepAliveTimer", keepAliveMsgTimer);

                IPAddress destinationAddress = session.RemoteEndPoint.Address;
                logger.Info("New session connected from : " + destinationAddress.ToString());
            };
            server.SessionClosed += (session, reason) =>
            {
                // Stop keepalive message timer
                KeepAliveDMYMsgTimer keepaliveMsgTimer = (KeepAliveDMYMsgTimer)session.Items["KeepAliveTimer"];
                keepaliveMsgTimer.Stop();

                IPAddress destinationAddress = session.RemoteEndPoint.Address;
                logger.Info("Session closed from : " + destinationAddress.ToString() + " Reason :" + reason.ToString());
            };
            server.NewMessageReceived += (session, message) =>
            {
                string received_JSON_mode;
                try
                {
                    var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                    received_JSON_mode = msg_dict["mode"];
                }
                catch (KeyNotFoundException ex)
                {
                    error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
                catch (JsonException ex)
                {
                    error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }

                try
                {
                    switch (received_JSON_mode)
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
                            fuelTripCalc.SectSpan = span_jsonobj.sect_span * 1000;
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
                catch (JSONFormatsException ex)
                {
                    error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
                catch (JsonException ex)
                {
                    error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
            };

            return server;
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

