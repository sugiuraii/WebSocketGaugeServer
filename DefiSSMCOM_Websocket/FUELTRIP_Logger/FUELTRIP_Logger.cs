using System;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.ClientEngine;
using SuperWebSocket;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using DefiSSMCOM.Defi;
using DefiSSMCOM.SSM;
using DefiSSMCOM.WebSocket.JSON;
using log4net;

namespace FUELTRIP_Logger
{
	public class FUELTRIP_JSONFormat : JSONFormats
	{
		public double moment_gasmilage;
		public double total_gas;
		public double total_trip;
		public double total_gasmilage;

		public FUELTRIP_JSONFormat()
		{
			mode = "MOMENT_FUELTRIP";
		}

		public override void Validate()
		{
			if (mode != "MOMENT_FUELTRIP") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
	}

	public class SectFUELTRIP_JSONFormat : JSONFormats
	{
		public long sect_span;
		public double[] sect_trip;
		public double[] sect_gas;
		public double[] sect_gasmilage;

		public SectFUELTRIP_JSONFormat()
		{
			mode = "SECT_FUELTRIP";
		}

		public override void Validate()
		{
			if (mode != "SECT_FUELTRIP") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
	}

	public class SectSpan_JSONFormat : JSONFormats
	{
		public int sect_span;

		public SectSpan_JSONFormat()
		{
			mode = "SECT_SPAN";
		}

		public override void Validate()
		{
			if (mode != "SECT_SPAN") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
	}

	public class SectStoreMax_JSONFormat : JSONFormats
	{
		public int storemax;

		public SectStoreMax_JSONFormat()
		{
			mode = "SECT_STOREMAX";
		}

		public override void Validate()
		{
			if (mode != "SECT_STOREMAX") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
	}

	public class FUELTRIP_Logger
	{
		private const int connet_retry_sec = 5;
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

            //Start Websocket server
            if (!_appServer.Setup(this.WebsocketServer_ListenPortNo)) //Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                logger.Fatal("Failed to setup websocket server.");
            }
            if (!_appServer.Start())
            {
                Console.WriteLine("Failed to start!");
                logger.Fatal("Failed to start websocket server.");
                return;
            }

            _nenpi_trip_calc.load_trip_gas();
            _deficom_ws_client.Open();
            _ssmcom_ws_client.Open();

            Console.WriteLine("Websocket server is started. DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
            logger.Info("Websocket server is started. DefiCOM_WS_URL:" + _deficom_WS_URL + " SSMCOM_WS_URL:" + _ssmcom_WS_URL + " ListenPort: " + this.WebsocketServer_ListenPortNo.ToString());
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

            Console.WriteLine("The server was stopped!");
            logger.Info("Websocket server is stopped");

		}

		// Websocket server events
		private void _appServer_SessionClosed(WebSocketSession session, CloseReason reason)
		{
            Console.WriteLine("Session closed from : " + session.Host + " Reason :" + reason.ToString());
            logger.Info("Session closed from : " + session.Host + " Reason :" + reason.ToString());
        }
		private void _appServer_NewSessionConnected(WebSocketSession session)
		{
            Console.WriteLine("New session connected from : " + session.Host);
            logger.Info("New session connected from : " + session.Host);
        }
		private void _appServer_NewMessageReceived(WebSocketSession session, string message)
		{
			string received_JSON_mode;
			try{
				var msg_dict = JsonConvert.DeserializeObject<Dictionary<string,string>> (message);
				received_JSON_mode = msg_dict ["mode"];
			}
			catch( KeyNotFoundException ex) {
				error_msg (ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch (JsonException ex) {
				error_msg (ex.GetType().ToString() + " " + ex.Message);
				return;
			}

			try
			{
				switch(received_JSON_mode)
				{
				//SSM COM all reset
				case ("RESET"):
					_nenpi_trip_calc.reset_sect_trip_gas();
					_nenpi_trip_calc.reset_total_trip_gas();
					response_msg("NenpiCalc AllRESET.");
					break;
				case ("SECTRESET"):
					_nenpi_trip_calc.reset_sect_trip_gas();
					response_msg("NenpiCalc SectRESET.");
					break;
				case ("SECT_SPAN"):
					SectSpan_JSONFormat span_jsonobj = JsonConvert.DeserializeObject<SectSpan_JSONFormat>(message);
					span_jsonobj.Validate();
					_nenpi_trip_calc.Sect_Span = span_jsonobj.sect_span*1000;
					response_msg("NenpiCalc SectSpan Set to : " + span_jsonobj.sect_span.ToString() + "sec");
					break;
				case ("SECT_STOREMAX"):
					SectStoreMax_JSONFormat storemax_jsonobj = JsonConvert.DeserializeObject<SectStoreMax_JSONFormat>(message);
					storemax_jsonobj.Validate();
					_nenpi_trip_calc.Sect_Store_Max = storemax_jsonobj.storemax;
					response_msg("NenpiCalc SectStoreMax Set to : " + storemax_jsonobj.storemax.ToString());
					break;
				default:
					throw new JSONFormatsException("Unsuppoted mode property.");
				}
			}
			catch(JSONFormatsException ex) {
				error_msg (ex.GetType().ToString() + " " + ex.Message);
				return;
			}
			catch(JsonException ex) {
				error_msg (ex.GetType().ToString() + " " + ex.Message);
				return;
			}
		}

		// Error message method
		private void error_msg(string message)
		{
            Console.WriteLine("Error message " + " : " + message);
            logger.Error("Send Error message " + " : " + message);
            ErrorJSONFormat errormsg_json = new ErrorJSONFormat();
			errormsg_json.msg = message;

			var sessions = _appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				session.Send (errormsg_json.Serialize ());
			}
		}

		private void response_msg(string message)
		{
            Console.WriteLine("Response message " + " : " + message);
            logger.Info("Send Response message " + " : " + message);
            ResponseJSONFormat resmsg_json = new ResponseJSONFormat();
			resmsg_json.msg = message;

			var sessions = _appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				session.Send (resmsg_json.Serialize ());
			}
		}

		private void send_momentum_value()
		{
			FUELTRIP_JSONFormat fueltrip_json = new FUELTRIP_JSONFormat ();
			fueltrip_json.moment_gasmilage = _nenpi_trip_calc.Momentary_Gas_Milage;
			fueltrip_json.total_gas = _nenpi_trip_calc.Total_Gas_Consumption;
			fueltrip_json.total_trip = _nenpi_trip_calc.Total_Trip;
			fueltrip_json.total_gasmilage = _nenpi_trip_calc.Total_Gas_Milage;

			var sessions = _appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				session.Send (fueltrip_json.Serialize());
			}
		}

		private void send_section_value_array()
		{
			SectFUELTRIP_JSONFormat sectfueltrip_json = new SectFUELTRIP_JSONFormat ();
			sectfueltrip_json.sect_gas = _nenpi_trip_calc.Sect_gas_array;
			sectfueltrip_json.sect_trip = _nenpi_trip_calc.Sect_trip_array;
			sectfueltrip_json.sect_gasmilage = _nenpi_trip_calc.Sect_gasmilage_array;
			sectfueltrip_json.sect_span = _nenpi_trip_calc.Sect_Span;

			var sessions = _appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
				session.Send (sectfueltrip_json.Serialize());
			}

		}

		// Parse VAL packet
		private void parse_val_paket(string jsonmsg, SSM_DEFI_mode ssm_defi_mode)
		{


			string received_JSON_mode;
			try{
                JObject jobject = JObject.Parse(jsonmsg);
				received_JSON_mode = jobject.Property("mode").Value.ToString();
			}
			catch( KeyNotFoundException ex) {
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
            catch (JsonReaderException ex)
            {
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
			catch (JsonException ex) {
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}

			try
			{
				if(received_JSON_mode == "VAL")
				{
					ValueJSONFormat val_json = JsonConvert.DeserializeObject<ValueJSONFormat>(jsonmsg);
					val_json.Validate();

					if(ssm_defi_mode == SSM_DEFI_mode.Defi)
					{
						_current_tacho = double.Parse(val_json.val[Defi_Parameter_Code.Tacho.ToString()]);
					}
					else if(ssm_defi_mode == SSM_DEFI_mode.SSM)
					{
                        //Console.WriteLine(jsonmsg);
						_current_speed = double.Parse(val_json.val[SSM_Parameter_Code.Vehicle_Speed.ToString()]);
						_current_injpulse_width = double.Parse(val_json.val[SSM_Parameter_Code.Fuel_Injection_1_Pulse_Width.ToString()]);
                        try
                        {
                            _nenpi_trip_calc.update(_current_tacho, _current_speed, _current_injpulse_width);
                            send_momentum_value();
                        }
                        catch (TimeoutException ex)
                        {
                            error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                            return;
                        }
					}
				}
                else if (received_JSON_mode == "ERR")
                {
                    ErrorJSONFormat err_json = JsonConvert.DeserializeObject<ErrorJSONFormat>(jsonmsg);
                    err_json.Validate();
                    error_msg("Error occured from " + ssm_defi_mode.ToString() + ":" + err_json.msg);
                }
                else if (received_JSON_mode == "RES")
                {
                    ResponseJSONFormat res_json = JsonConvert.DeserializeObject<ResponseJSONFormat>(jsonmsg);
                    res_json.Validate();
                    response_msg("Response from " + ssm_defi_mode.ToString() + ":" + res_json.msg);
                }
			}
			catch(JSONFormatsException ex) {
				error_msg (ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(JsonException ex) {
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(KeyNotFoundException ex){
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
			catch(FormatException ex) {
                error_msg(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
				return;
			}
		}

		// deficom WS client event
		private void _deficom_ws_client_Opened(object sender, EventArgs e)
		{
			// initialize setting
			Defi_WS_SendJSONFormat defisendcode = new Defi_WS_SendJSONFormat ();
			defisendcode.code = Defi_Parameter_Code.Tacho.ToString ();
			defisendcode.flag = true;

			Defi_WS_IntervalJSONFormat definitervalcode = new Defi_WS_IntervalJSONFormat();
			definitervalcode.interval=0;

			_deficom_ws_client.Send(defisendcode.Serialize());
			_deficom_ws_client.Send(definitervalcode.Serialize());
		}
		private void _deficom_ws_client_Error(object sender, EventArgs e)
		{
			error_msg("Deficom Websocket connection error. Wait " + connet_retry_sec.ToString() +"sec and reconnect.");
            Thread.Sleep(connet_retry_sec * 1000);
            _deficom_ws_client.Open();
		}
		private void _deficom_ws_client_Closed(object sender, EventArgs e)
		{
			error_msg("Deficom Websocket connection is Closed. Wait " + connet_retry_sec.ToString() +"sec and reconnect.");
			Thread.Sleep (connet_retry_sec * 1000);
			//_deficom_ws_client.Open ();
		}
		private void _deficom_ws_client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			string message = e.Message;
			parse_val_paket (message, SSM_DEFI_mode.Defi);
		}


		// ssmcom WS client event
		private void _ssmcom_ws_client_Opened(object sender, EventArgs e)
		{
            SSM_SLOWREAD_IntervalJSONFormat ssmcom_slowread_json = new SSM_SLOWREAD_IntervalJSONFormat();
            ssmcom_slowread_json.interval = 20;
            _ssmcom_ws_client.Send(ssmcom_slowread_json.Serialize());

            SSM_COM_ReadJSONFormat ssmcom_read_json1 = new SSM_COM_ReadJSONFormat();
            ssmcom_read_json1.code = SSM_Parameter_Code.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json1.read_mode = "SLOW";
            ssmcom_read_json1.flag = true;
            _ssmcom_ws_client.Send(ssmcom_read_json1.Serialize());

            SSM_COM_ReadJSONFormat ssmcom_read_json2 = new SSM_COM_ReadJSONFormat();
			ssmcom_read_json2.code = SSM_Parameter_Code.Vehicle_Speed.ToString ();
			ssmcom_read_json2.read_mode = "FAST";
			ssmcom_read_json2.flag = true;
			_ssmcom_ws_client.Send(ssmcom_read_json2.Serialize());

            SSM_COM_ReadJSONFormat ssmcom_read_json3 = new SSM_COM_ReadJSONFormat();
            ssmcom_read_json3 = new SSM_COM_ReadJSONFormat();
            ssmcom_read_json3.code = SSM_Parameter_Code.Vehicle_Speed.ToString();
            ssmcom_read_json3.read_mode = "SLOW";
            ssmcom_read_json3.flag = true;
			_ssmcom_ws_client.Send (ssmcom_read_json3.Serialize ());

            SSM_COM_ReadJSONFormat ssmcom_read_json4 = new SSM_COM_ReadJSONFormat();
            ssmcom_read_json4 = new SSM_COM_ReadJSONFormat();
            ssmcom_read_json4.code = SSM_Parameter_Code.Fuel_Injection_1_Pulse_Width.ToString();
            ssmcom_read_json4.read_mode = "FAST";
            ssmcom_read_json4.flag = true;
            _ssmcom_ws_client.Send(ssmcom_read_json4.Serialize());

		}
		private void _ssmcom_ws_client_Error(object sender, EventArgs e)
		{
			error_msg("SSMcom Websocket connection error. Wait " + connet_retry_sec.ToString() +"sec and reconnect.");
            Thread.Sleep(connet_retry_sec * 1000);
            _ssmcom_ws_client.Open();
        }
		private void _ssmcom_ws_client_Closed(object sender, EventArgs e)
		{
			error_msg("com Websocket connection is Closed. Wait " + connet_retry_sec.ToString() +"sec and reconnect.");
			Thread.Sleep (connet_retry_sec * 1000);
			//_deficom_ws_client.Open ();
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

