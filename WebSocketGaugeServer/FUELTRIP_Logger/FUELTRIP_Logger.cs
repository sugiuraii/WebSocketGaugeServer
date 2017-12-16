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
using DefiSSMCOM.WebSocket.JSON;
using DefiSSMCOM.WebSocket;
using log4net;

namespace FUELTRIP_Logger
{
    class WebSocketClients
    {
    	private const int CONNECT_RETRY_SEC = 5;
        private const int DEFIPACKET_INTERVAL = 2;
        public WebSocket DefiCOMWSClient;
        public WebSocket SSMCOMWSClient;
        public WebSocket ArduinoWSClient;
        public WebSocket ELN327WSClient;

        public WebSocketClients(AppSettings appSettings)
        {

        }

        private WebSocket initializeDefiCOMWSClient(string url, List<DefiParameterCode> DefiCodes)
        {
            WebSocket wsClient = new WebSocket(url);
            
            wsClient.Opened += (sender, e) => 
            {
                foreach(DefiParameterCode code in DefiCodes)
                {
			        DefiWSSendJSONFormat defisendcode = new DefiWSSendJSONFormat ();
			        defisendcode.code = code.ToString ();
			        defisendcode.flag = true;

			        DefiWSIntervalJSONFormat definitervalcode = new DefiWSIntervalJSONFormat();
			        definitervalcode.interval=DEFIPACKET_INTERVAL;

			        wsClient.Send(defisendcode.Serialize());
			        wsClient.Send(definitervalcode.Serialize());
                }
		    };
            //deficom ws
			deficomWSClient.Error += new EventHandler<ErrorEventArgs>(_deficom_ws_client_Error);
			deficomWSClient.Closed += new EventHandler(_deficom_ws_client_Closed);
			deficomWSClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_deficom_ws_client_MessageReceived);

        }

    }
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


		// Parse VAL packet
		private void parse_val_paket(string jsonmsg, SSM_DEFI_mode ssm_defi_mode)
		{
            //Ignore "DMY" message. (DMY message is sent from server in order to keep-alive wifi connection (to prevent wifi low-power(high latency) mode).
            if (jsonmsg == "DMY")
                return;

			string received_JSON_mode;
			try{
                JObject jobject = JObject.Parse(jsonmsg);
				received_JSON_mode = jobject.Property("mode").Value.ToString();
			}
			catch( KeyNotFoundException ex) {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
            catch (JsonReaderException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
			catch (JsonException ex) {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}

			try
			{
				if(received_JSON_mode == ValueJSONFormat.ModeCode)
				{
					ValueJSONFormat val_json = JsonConvert.DeserializeObject<ValueJSONFormat>(jsonmsg);
					val_json.Validate();

					if(ssm_defi_mode == SSM_DEFI_mode.Defi)
					{
						currentEngineRev = double.Parse(val_json.val[DefiParameterCode.Engine_Speed.ToString()]);
					}
					else if(ssm_defi_mode == SSM_DEFI_mode.SSM)
					{
                        //Console.WriteLine(jsonmsg);
                        try
                        {
                            currentVehicleSpeed = double.Parse(val_json.val[SSMParameterCode.Vehicle_Speed.ToString()]);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            logger.Warn(SSMParameterCode.Vehicle_Speed.ToString() + "is not found in received json message. (You can ignore this warning, if this warning stops in several seconds.) Exception message : " + ex.Message + " " + ex.StackTrace);
                            return;
                        }
                        try
                        {
                            currentInjPulseWidth = double.Parse(val_json.val[SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString()]);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            logger.Warn(SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString() + "is not found in received json message. (You can ignore this warning, if this warning stops in several seconds.) Exception message : " + ex.Message + " " + ex.StackTrace);
                            return;
                        }
                        try
                        {
                            fuelTripCalc.update(currentEngineRev, currentVehicleSpeed, currentInjPulseWidth,0,0,0);
                            send_momentum_value();
                        }
                        catch (TimeoutException ex)
                        {
                            logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                            return;
                        }
					}
				}
                else if (received_JSON_mode == ErrorJSONFormat.ModeCode)
                {
                    ErrorJSONFormat err_json = JsonConvert.DeserializeObject<ErrorJSONFormat>(jsonmsg);
                    err_json.Validate();
                    logger.Error("Error occured from " + ssm_defi_mode.ToString() + ":" + err_json.msg);
                }
                else if (received_JSON_mode == ResponseJSONFormat.ModeCode)
                {
                    ResponseJSONFormat res_json = JsonConvert.DeserializeObject<ResponseJSONFormat>(jsonmsg);
                    res_json.Validate();
                    logger.Info("Response from " + ssm_defi_mode.ToString() + ":" + res_json.msg);
                }
			}
			catch(JSONFormatsException ex) {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(JsonException ex) {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(KeyNotFoundException ex){
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(FormatException ex) {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
		}

		// deficom WS client event
		private void _deficom_ws_client_Opened(object sender, EventArgs e)
		{
            //Sleep 5sec in order to wait until the session is registered to the SSM WS session snapshot.
            //Thread.Sleep(5000);

			// initialize setting
			DefiWSSendJSONFormat defisendcode = new DefiWSSendJSONFormat ();
			defisendcode.code = DefiParameterCode.Engine_Speed.ToString ();
			defisendcode.flag = true;

			DefiWSIntervalJSONFormat definitervalcode = new DefiWSIntervalJSONFormat();
			definitervalcode.interval=DEFIPACKET_INTERVAL;

			deficomWSClient.Send(defisendcode.Serialize());
			deficomWSClient.Send(definitervalcode.Serialize());
		}
		private void _deficom_ws_client_Error(object sender, ErrorEventArgs e)
		{
            logger.Error("DefiCOM Websocket connection error occurs. Exception : " + e.Exception.ToString() + "\n Message : " + e.Exception.Message);
		}
		private void _deficom_ws_client_Closed(object sender, EventArgs e)
		{
            logger.Info("DefiCOM Websocket connection is Closed. Wait " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
			Thread.Sleep (CONNECT_RETRY_SEC * 1000);
            while (deficomWSClient.State != WebSocketState.Closed)
            {
                logger.Info("DefiCOM Websocket is now closing, not closed completely. Wait more " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
                Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            }
			deficomWSClient.Open ();
		}
		private void _deficom_ws_client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			string message = e.Message;
			parse_val_paket (message, SSM_DEFI_mode.Defi);
		}


		// ssmcom WS client event
		private void _ssmcom_ws_client_Opened(object sender, EventArgs e)
		{
            //Sleep 5sec in order to wait until the session is registered to the SSM WS session snapshot.
            //Thread.Sleep(5000);

            SSMSLOWREADIntervalJSONFormat ssmcom_slowread_json = new SSMSLOWREADIntervalJSONFormat();
            ssmcom_slowread_json.interval = 20;
            ssmcomWSClient.Send(ssmcom_slowread_json.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json1 = new SSMCOMReadJSONFormat();
            ssmcom_read_json1.code = SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json1.read_mode = SSMCOMReadJSONFormat.SlowReadModeCOde;
            ssmcom_read_json1.flag = true;
            ssmcomWSClient.Send(ssmcom_read_json1.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json2 = new SSMCOMReadJSONFormat();
			ssmcom_read_json2.code = SSMParameterCode.Vehicle_Speed.ToString ();
			ssmcom_read_json2.read_mode = SSMCOMReadJSONFormat.FastReadModeCode;
			ssmcom_read_json2.flag = true;
			ssmcomWSClient.Send(ssmcom_read_json2.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json3 = new SSMCOMReadJSONFormat();
            ssmcom_read_json3 = new SSMCOMReadJSONFormat();
            ssmcom_read_json3.code = SSMParameterCode.Vehicle_Speed.ToString();
            ssmcom_read_json3.read_mode = SSMCOMReadJSONFormat.SlowReadModeCOde;
            ssmcom_read_json3.flag = true;
			ssmcomWSClient.Send (ssmcom_read_json3.Serialize ());

            SSMCOMReadJSONFormat ssmcom_read_json4 = new SSMCOMReadJSONFormat();
            ssmcom_read_json4 = new SSMCOMReadJSONFormat();
            ssmcom_read_json4.code = SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json4.read_mode = SSMCOMReadJSONFormat.FastReadModeCode;
            ssmcom_read_json4.flag = true;
            ssmcomWSClient.Send(ssmcom_read_json4.Serialize());

		}
		private void _ssmcom_ws_client_Error(object sender, ErrorEventArgs e)
		{
            logger.Info("SSMCOM Websocket connection error occurs. Exception : " + e.Exception.ToString() + "\n Message : " + e.Exception.Message);
        }
		private void _ssmcom_ws_client_Closed(object sender, EventArgs e)
		{
            logger.Info("SSMCOM Websocket connection is Closed. Wait " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
			Thread.Sleep (CONNECT_RETRY_SEC * 1000);
			while(ssmcomWSClient.State != WebSocketState.Closed)
            {
                logger.Info("SSMCOM Websocket is now closing, not closed completely. Wait more" + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
                Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            }
            ssmcomWSClient.Open ();
		}
		private void _ssmcom_ws_client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			string message = e.Message;
			parse_val_paket (message, SSM_DEFI_mode.SSM);

		}

        private void _nenpi_trip_calc_SectFUELTRIPUpdated(object sender, EventArgs e)
        {
            send_section_value_array();
        }
	}
}

