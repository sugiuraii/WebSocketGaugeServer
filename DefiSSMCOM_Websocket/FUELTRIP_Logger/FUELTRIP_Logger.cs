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
using log4net;

namespace FUELTRIP_Logger
{
	public class FUELTRIP_Logger
	{
		private const int CONNECT_RETRY_SEC = 5;
        private const int DEFIPACKET_INTERVAL = 2;
		private enum SSM_DEFI_mode{
			Defi,
			SSM
		};

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Nenpi_Trip_Calculator _nenpi_trip_calc;
		private WebSocketServer _appServer;
		private WebSocket _deficom_ws_client;
		private WebSocket _ssmcom_ws_client;
		private bool running_state = false;

		private double _current_tacho;
		private double _current_speed;
		private double _current_injpulse_width;

        private string _deficom_WS_URL;
        private string _ssmcom_WS_URL;

		public int WebsocketServer_ListenPortNo { get; set; }

		public FUELTRIP_Logger(string deficom_WS_URL, string ssmcom_WS_URL)
		{
            this.WebsocketServer_ListenPortNo = 2014;

			_nenpi_trip_calc = new Nenpi_Trip_Calculator ();

			//Websocket server setup
			_appServer = new WebSocketServer ();
			_appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(_appServer_NewMessageReceived);
			_appServer.NewSessionConnected += new SessionHandler<WebSocketSession> (_appServer_NewSessionConnected);
			_appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason> (_appServer_SessionClosed);

            _deficom_WS_URL = deficom_WS_URL;
            _ssmcom_WS_URL = ssmcom_WS_URL;

			//deficom ws
			_deficom_ws_client = new WebSocket (deficom_WS_URL);
			_deficom_ws_client.Opened += new EventHandler(_deficom_ws_client_Opened);
			_deficom_ws_client.Error += new EventHandler<ErrorEventArgs>(_deficom_ws_client_Error);
			_deficom_ws_client.Closed += new EventHandler(_deficom_ws_client_Closed);
			_deficom_ws_client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_deficom_ws_client_MessageReceived);
            //ssmcom ws
            _ssmcom_ws_client = new WebSocket (ssmcom_WS_URL);
			_ssmcom_ws_client.Opened += new EventHandler(_ssmcom_ws_client_Opened);
			_ssmcom_ws_client.Error += new EventHandler<ErrorEventArgs>(_ssmcom_ws_client_Error);
			_ssmcom_ws_client.Closed += new EventHandler(_ssmcom_ws_client_Closed);
			_ssmcom_ws_client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_ssmcom_ws_client_MessageReceived);
			running_state = false;

            _nenpi_trip_calc.SectFUELTRIPUpdated += new EventHandler(_nenpi_trip_calc_SectFUELTRIPUpdated);
		}

		public void start()
		{
			running_state = true;

            SuperSocket.SocketBase.Config.ServerConfig appserver_config = new SuperSocket.SocketBase.Config.ServerConfig();
            appserver_config.DisableSessionSnapshot = false;
            appserver_config.SessionSnapshotInterval = 1;
            appserver_config.Port = this.WebsocketServer_ListenPortNo;

            //Start Websocket server
            if (!_appServer.Setup(appserver_config)) //Setup with listening port
            {
                //Console.WriteLine("Failed to setup!");
                logger.Fatal("Failed to setup websocket server.");
            }
            if (!_appServer.Start())
            {
                //Console.WriteLine("Failed to start!");
                logger.Fatal("Failed to start websocket server.");
                return;
            }

            _nenpi_trip_calc.load_trip_gas();
            _deficom_ws_client.Open();
            _ssmcom_ws_client.Open();

            //Console.WriteLine("Websocket server is starting... DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
            logger.Info("Websocket server is starting... DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
        }

		public void stop ()
		{
			running_state = false;
			_nenpi_trip_calc.save_trip_gas ();
            if(_deficom_ws_client.State == WebSocketState.Open)
    			_deficom_ws_client.Close ();
            if(_ssmcom_ws_client.State == WebSocketState.Open)
    			_ssmcom_ws_client.Close ();
            if(_appServer.State == ServerState.Running)
                _appServer.Stop ();

            //Console.WriteLine("The server was stopped!");
            logger.Info("Websocket server is stopped");

		}

		// Websocket server events
		private void _appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Session closed from : " + destinationAddress.ToString() + " Reason :" + reason.ToString());
        }
		private void _appServer_NewSessionConnected(WebSocketSession session)
		{
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
					_nenpi_trip_calc.reset_sect_trip_gas();
					_nenpi_trip_calc.reset_total_trip_gas();
					response_msg(session, "NenpiCalc AllRESET.");
					break;
				case (SectResetJSONFormat.ModeCode):
					_nenpi_trip_calc.reset_sect_trip_gas();
					response_msg(session, "NenpiCalc SectRESET.");
					break;
				case (SectSpanJSONFormat.ModeCode):
					SectSpanJSONFormat span_jsonobj = JsonConvert.DeserializeObject<SectSpanJSONFormat>(message);
					span_jsonobj.Validate();
					_nenpi_trip_calc.Sect_Span = span_jsonobj.sect_span*1000;
					response_msg(session, "NenpiCalc SectSpan Set to : " + span_jsonobj.sect_span.ToString() + "sec");
					break;
				case (SectStoreMaxJSONFormat.ModeCode):
					SectStoreMaxJSONFormat storemax_jsonobj = JsonConvert.DeserializeObject<SectStoreMaxJSONFormat>(message);
					storemax_jsonobj.Validate();
					_nenpi_trip_calc.Sect_Store_Max = storemax_jsonobj.storemax;
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
			fueltrip_json.moment_gasmilage = _nenpi_trip_calc.Momentary_Gas_Milage;
			fueltrip_json.total_gas = _nenpi_trip_calc.Total_Gas_Consumption;
			fueltrip_json.total_trip = _nenpi_trip_calc.Total_Trip;
			fueltrip_json.total_gasmilage = _nenpi_trip_calc.Total_Gas_Milage;


            broadcast_websocket_msg(fueltrip_json.Serialize());
		}

		private void send_section_value_array()
		{
			SectFUELTRIPJSONFormat sectfueltrip_json = new SectFUELTRIPJSONFormat ();
			sectfueltrip_json.sect_gas = _nenpi_trip_calc.Sect_gas_array;
			sectfueltrip_json.sect_trip = _nenpi_trip_calc.Sect_trip_array;
			sectfueltrip_json.sect_gasmilage = _nenpi_trip_calc.Sect_gasmilage_array;
			sectfueltrip_json.sect_span = _nenpi_trip_calc.Sect_Span;

            broadcast_websocket_msg(sectfueltrip_json.Serialize());

		}

        // Broadcast : Send message to all active sessions
        private void broadcast_websocket_msg(string message)
        {
            var sessions = _appServer.GetAllSessions();

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
						_current_tacho = double.Parse(val_json.val[DefiParameterCode.Engine_Speed.ToString()]);
					}
					else if(ssm_defi_mode == SSM_DEFI_mode.SSM)
					{
                        //Console.WriteLine(jsonmsg);
                        try
                        {
                            _current_speed = double.Parse(val_json.val[SSMParameterCode.Vehicle_Speed.ToString()]);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            logger.Warn(SSMParameterCode.Vehicle_Speed.ToString() + "is not found in received json message. (You can ignore this warning, if this warning stops in several seconds.) Exception message : " + ex.Message + " " + ex.StackTrace);
                            return;
                        }
                        try
                        {
                            _current_injpulse_width = double.Parse(val_json.val[SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString()]);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            logger.Warn(SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString() + "is not found in received json message. (You can ignore this warning, if this warning stops in several seconds.) Exception message : " + ex.Message + " " + ex.StackTrace);
                            return;
                        }
                        try
                        {
                            _nenpi_trip_calc.update(_current_tacho, _current_speed, _current_injpulse_width);
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

			_deficom_ws_client.Send(defisendcode.Serialize());
			_deficom_ws_client.Send(definitervalcode.Serialize());
		}
		private void _deficom_ws_client_Error(object sender, ErrorEventArgs e)
		{
            logger.Error("DefiCOM Websocket connection error occurs. Exception : " + e.Exception.ToString() + "\n Message : " + e.Exception.Message);
		}
		private void _deficom_ws_client_Closed(object sender, EventArgs e)
		{
            logger.Info("DefiCOM Websocket connection is Closed. Wait " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
			Thread.Sleep (CONNECT_RETRY_SEC * 1000);
            while (_deficom_ws_client.State != WebSocketState.Closed)
            {
                logger.Info("DefiCOM Websocket is now closing, not closed completely. Wait more " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
                Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            }
			_deficom_ws_client.Open ();
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
            _ssmcom_ws_client.Send(ssmcom_slowread_json.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json1 = new SSMCOMReadJSONFormat();
            ssmcom_read_json1.code = SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json1.read_mode = SSMCOMReadJSONFormat.SlowReadModeCOde;
            ssmcom_read_json1.flag = true;
            _ssmcom_ws_client.Send(ssmcom_read_json1.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json2 = new SSMCOMReadJSONFormat();
			ssmcom_read_json2.code = SSMParameterCode.Vehicle_Speed.ToString ();
			ssmcom_read_json2.read_mode = SSMCOMReadJSONFormat.FastReadModeCode;
			ssmcom_read_json2.flag = true;
			_ssmcom_ws_client.Send(ssmcom_read_json2.Serialize());

            SSMCOMReadJSONFormat ssmcom_read_json3 = new SSMCOMReadJSONFormat();
            ssmcom_read_json3 = new SSMCOMReadJSONFormat();
            ssmcom_read_json3.code = SSMParameterCode.Vehicle_Speed.ToString();
            ssmcom_read_json3.read_mode = SSMCOMReadJSONFormat.SlowReadModeCOde;
            ssmcom_read_json3.flag = true;
			_ssmcom_ws_client.Send (ssmcom_read_json3.Serialize ());

            SSMCOMReadJSONFormat ssmcom_read_json4 = new SSMCOMReadJSONFormat();
            ssmcom_read_json4 = new SSMCOMReadJSONFormat();
            ssmcom_read_json4.code = SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json4.read_mode = SSMCOMReadJSONFormat.FastReadModeCode;
            ssmcom_read_json4.flag = true;
            _ssmcom_ws_client.Send(ssmcom_read_json4.Serialize());

		}
		private void _ssmcom_ws_client_Error(object sender, ErrorEventArgs e)
		{
            logger.Info("SSMCOM Websocket connection error occurs. Exception : " + e.Exception.ToString() + "\n Message : " + e.Exception.Message);
        }
		private void _ssmcom_ws_client_Closed(object sender, EventArgs e)
		{
            logger.Info("SSMCOM Websocket connection is Closed. Wait " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
			Thread.Sleep (CONNECT_RETRY_SEC * 1000);
			while(_ssmcom_ws_client.State != WebSocketState.Closed)
            {
                logger.Info("SSMCOM Websocket is now closing, not closed completely. Wait more" + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
                Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            }
            _ssmcom_ws_client.Open ();
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

