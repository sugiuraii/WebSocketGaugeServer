using System;
using System.Collections.Generic;
using DefiSSMCOM.Defi;
using DefiSSMCOM.SSM;
using DefiSSMCOM.Arduino;
using DefiSSMCOM.OBDII;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.Threading;
using DefiSSMCOM.WebSocket.JSON;

namespace FUELTRIP_Logger
{
    class WebSocketClients
    {
        private const int CONNECT_RETRY_SEC = 5;
        private const int DEFI_ARDUINO_PACKET_INTERVAL = 2;
        public readonly WebSocket DefiWSClient;
        public readonly WebSocket SSMWSClient;
        public readonly WebSocket ArduinoWSClient;
        public readonly WebSocket ELM327WSClient;

        private Boolean runningState;

        private double engineRev;
        public double EngineRev
        {
            get
            {
                return engineRev;
            }
        }
        private double vehicleSpeed;
        public double VehicleSpeed
        {
            get
            {
                return vehicleSpeed;
            }
        }
        private double injPulseWidth;
        public double InjPulseWidth
        {
            get
            {
                return injPulseWidth;
            }
        }
        private double massAirFlow;
        public double MassAirFlow
        {
            get
            {
                return massAirFlow;
            }
        }
        private double afRatio;
        public double AFRatio
        {
            get
            {
                return afRatio;
            }
        }
        private double fuelRate;
        public double FuelRate
        {
            get
            {
                return fuelRate;
            }
        }

        public event EventHandler<EventArgs> VALMessageParsed;

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// Construct WebsocketClients.
        /// </summary>
        /// <param name="appSettings">AppSettings</param>
        public WebSocketClients(AppSettings appSettings)
        {
            RequiredParameterCode requiredCodes = appSettings.getRequiredParameterCodes();
            DefiWSClient = initializeDefiCOMWSClient(appSettings.defiserver_url, requiredCodes.DefiCodes);
            SSMWSClient = initializeSSMCOMWSClient(appSettings.ssmserver_url, requiredCodes.SSMCodes);
            ArduinoWSClient = initializeArduinoCOMWSClient(appSettings.arduinoserver_url, requiredCodes.ArduinoCodes);
            ELM327WSClient = initializeELM327COMWSClient(appSettings.elm327server_url, requiredCodes.ELM327OBDCodes);

            runningState = false;
        }

        /// <summary>
        /// Open websocket clients.
        /// </summary>
        public void start()
        {
            if (runningState)
                throw new InvalidOperationException("Websocket clients are already started.");

            if (DefiWSClient != null)
                DefiWSClient.Open();
            if (SSMWSClient != null)
                SSMWSClient.Open();
            if (ArduinoWSClient != null)
                ArduinoWSClient.Open();
            if (ELM327WSClient != null)
                ELM327WSClient.Open();
            
            this.runningState = true;
        }

        /// <summary>
        /// Stop(close) websocket clients.
        /// </summary>
        public void stop()
        {
            if (!runningState)
                return;

            if (DefiWSClient != null)
                if (DefiWSClient.State == WebSocketState.Open)
                    DefiWSClient.Close();
            if (SSMWSClient != null)
                if (SSMWSClient.State == WebSocketState.Open)
                    SSMWSClient.Close();
            if (ArduinoWSClient != null)
                if (ArduinoWSClient.State == WebSocketState.Open)
                    ArduinoWSClient.Close();
            if (ELM327WSClient != null)
                if (ELM327WSClient.State == WebSocketState.Open)
                    ELM327WSClient.Close();
        }

        /// <summary>
        /// Common method to log error message
        /// </summary>
        /// <param name="clientType">WS client type</param>
        /// <param name="errorName">Error(exception name)</param>
        /// <param name="errorMsg">Error message</param>
        private void wsErrorMsg(string clientType, string errorName, string errorMsg)
        {
            logger.Error(clientType + " Websocket connection error occurs. Exception : " + errorName + "\n Message : " + errorMsg);
        }

        /// <summary>
        /// Common method to reconnect Websocket client on the client is closed.
        /// </summary>
        /// <param name="clientType">Websocket client type</param>
        /// <param name="wsClient"> Websocket client object</param>
        private void wsClosedReconnect(string clientType, WebSocket wsClient)
        {
            logger.Info(clientType + " Websocket connection is Closed. Wait " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
            Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            while (wsClient.State != WebSocketState.Closed)
            {
                logger.Info(clientType + " Websocket is now closing, not closed completely. Wait more " + CONNECT_RETRY_SEC.ToString() + "sec and reconnect.");
                Thread.Sleep(CONNECT_RETRY_SEC * 1000);
            }
            wsClient.Open();
        }

        /// <summary>
        /// Initialize DefiCOM websocket client
        /// </summary>
        /// <param name="url">URL of the server</param>
        /// <param name="codes">parameter code list to activate</param>
        /// <returns></returns>
        private WebSocket initializeDefiCOMWSClient(string url, List<DefiParameterCode> codes)
        {
            if (codes.Count == 0)
                return null;

            const WebSocketType WSType = WebSocketType.DEFI;
            WebSocket wsClient = new WebSocket(url);

            wsClient.Opened += (sender, e) =>
            {
                foreach (DefiParameterCode code in codes)
                {
                    DefiWSSendJSONFormat sendcode = new DefiWSSendJSONFormat();
                    sendcode.code = code.ToString();
                    sendcode.flag = true;

                    DefiWSIntervalJSONFormat definitervalcode = new DefiWSIntervalJSONFormat();
                    definitervalcode.interval = DEFI_ARDUINO_PACKET_INTERVAL;

                    wsClient.Send(sendcode.Serialize());
                    wsClient.Send(definitervalcode.Serialize());
                }
            };
            wsClient.Error += (sender, e) => wsErrorMsg(WSType.ToString(), e.Exception.ToString(), e.Exception.Message);
            wsClient.Closed += (sender, e) => wsClosedReconnect(WSType.ToString(), wsClient);
            wsClient.MessageReceived += (sender, e) => parseVALMessage(e.Message, WSType);

            return wsClient;
        }

        /// <summary>
        /// Initialize ArduinoCOM websocket client
        /// </summary>
        /// <param name="url">URL of the server</param>
        /// <param name="codes">parameter code list to activate</param>
        /// <returns></returns>
        private WebSocket initializeArduinoCOMWSClient(string url, List<ArduinoParameterCode> codes)
        {
            if (codes.Count == 0)
                return null;

            const WebSocketType WSType = WebSocketType.ARDUINO;
            WebSocket wsClient = new WebSocket(url);

            wsClient.Opened += (sender, e) =>
            {
                foreach (ArduinoParameterCode code in codes)
                {
                    ArduinoWSSendJSONFormat sendcode = new ArduinoWSSendJSONFormat();
                    sendcode.code = code.ToString();
                    sendcode.flag = true;

                    ArduinoWSIntervalJSONFormat definitervalcode = new ArduinoWSIntervalJSONFormat();
                    definitervalcode.interval = DEFI_ARDUINO_PACKET_INTERVAL;

                    wsClient.Send(sendcode.Serialize());
                    wsClient.Send(definitervalcode.Serialize());
                }
            };
            wsClient.Error += (sender, e) => wsErrorMsg(WSType.ToString(), e.Exception.ToString(), e.Exception.Message);
            wsClient.Closed += (sender, e) => wsClosedReconnect(WSType.ToString(), wsClient);
            wsClient.MessageReceived += (sender, e) => parseVALMessage(e.Message, WSType);

            return wsClient;
        }

        /// <summary>
        /// Initialize SSMCOM websocket client.
        /// </summary>
        /// <param name="url">URL of the server.</param>
        /// <param name="codes">Parameter code list</param>
        /// <returns></returns>
        private WebSocket initializeSSMCOMWSClient(string url, List<SSMParameterCode> codes)
        {
            if (codes.Count == 0)
                return null;

            const WebSocketType WSType = WebSocketType.SSM;
            WebSocket wsClient = new WebSocket(url);

            wsClient.Opened += (sender, e) =>
            {
                foreach (SSMParameterCode code in codes)
                {
                    // Send read packet both slow and fast readmode.
                    SSMCOMReadJSONFormat sendcode = new SSMCOMReadJSONFormat();
                    sendcode.code = code.ToString();
                    sendcode.read_mode = SSMCOMReadJSONFormat.SlowReadModeCode;
                    sendcode.flag = true;
                    wsClient.Send(sendcode.Serialize());

                    sendcode.read_mode = SSMCOMReadJSONFormat.FastReadModeCode;
                    wsClient.Send(sendcode.Serialize());
                }
            };
            wsClient.Error += (sender, e) => wsErrorMsg(WSType.ToString(), e.Exception.ToString(), e.Exception.Message);
            wsClient.Closed += (sender, e) => wsClosedReconnect(WSType.ToString(), wsClient);
            wsClient.MessageReceived += (sender, e) => parseVALMessage(e.Message, WSType);

            return wsClient;
        }

        /// <summary>
        /// Initialize ELM327COM websocket client.
        /// </summary>
        /// <param name="url">URL of the server.</param>
        /// <param name="codes">Parameter code list</param>
        /// <returns></returns>
        private WebSocket initializeELM327COMWSClient(string url, List<OBDIIParameterCode> codes)
        {
            if (codes.Count == 0)
                return null;

            const WebSocketType WSType = WebSocketType.ELM327;
            WebSocket wsClient = new WebSocket(url);

            wsClient.Opened += (sender, e) =>
            {
                foreach (OBDIIParameterCode code in codes)
                {
                    // Send read packet both slow and fast readmode.
                    ELM327COMReadJSONFormat sendcode = new ELM327COMReadJSONFormat();
                    sendcode.code = code.ToString();
                    sendcode.read_mode = ELM327COMReadJSONFormat.SlowReadModeCode;
                    sendcode.flag = true;
                    wsClient.Send(sendcode.Serialize());

                    sendcode.read_mode = ELM327COMReadJSONFormat.FastReadModeCode;
                    wsClient.Send(sendcode.Serialize());
                }
            };
            wsClient.Error += (sender, e) => wsErrorMsg(WSType.ToString(), e.Exception.ToString(), e.Exception.Message);
            wsClient.Closed += (sender, e) => wsClosedReconnect(WSType.ToString(), wsClient);
            wsClient.MessageReceived += (sender, e) => parseVALMessage(e.Message, WSType);

            return wsClient;
        }

        /// <summary>
        /// Parse VAL message from Websocket server.
        /// </summary>
        /// <param name="jsonmsg">JSONMessage</param>
        /// <param name="wsClientType">WebsocketClientType</param>
        private void parseVALMessage(string jsonmsg, WebSocketType wsClientType)
        {
            //Ignore "DMY" message. (DMY message is sent from server in order to keep-alive wifi connection (to prevent wifi low-power(high latency) mode).
            if (jsonmsg == "DMY")
                return;

            string receivedJSONMode;
            try
            {
                JObject jobject = JObject.Parse(jsonmsg);
                receivedJSONMode = jobject.Property("mode").Value.ToString();
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (JsonReaderException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (JsonException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }

            try
            {
                if (receivedJSONMode == ValueJSONFormat.ModeCode)
                {
                    ValueJSONFormat valJson = JsonConvert.DeserializeObject<ValueJSONFormat>(jsonmsg);
                    valJson.Validate();

                    switch(wsClientType)
                    {
                        case WebSocketType.DEFI:
                            foreach(String code in valJson.val.Keys)
                            {
                                double value = double.Parse(valJson.val[code]);
                                if(code == DefiParameterCode.Engine_Speed.ToString())
                                    engineRev = value;
                                else
                                    throw new InvalidOperationException("Unexpected parameter code is returned on parsing VAL. ServerType:" + wsClientType.ToString() + " Code:"  + code);
                            }
                            break;
                        case WebSocketType.SSM:
                            foreach(string code in valJson.val.Keys)
                            {
                                double value = double.Parse(valJson.val[code]);
                                if(code == SSMParameterCode.Vehicle_Speed.ToString())
                                    vehicleSpeed = value;
                                else if (code == SSMParameterCode.Engine_Speed.ToString())
                                    engineRev = value;
                                else if (code == SSMParameterCode.Fuel_Injection_1_Pulse_Width.ToString())
                                    injPulseWidth = value;
                                else if (code == SSMParameterCode.Mass_Air_Flow.ToString())
                                    massAirFlow = value;
                                else if (code == SSMParameterCode.Air_Fuel_Sensor_1.ToString())
                                    afRatio = value;
                                else
                                    throw new InvalidOperationException("Unexpected parameter code is returned on parsing VAL. ServerType:" + wsClientType.ToString() + " Code:"  + code);
                            }
                            break;
                        case WebSocketType.ARDUINO:
                            foreach(string code in valJson.val.Keys)
                            {
                                double value = double.Parse(valJson.val[code]);
                                if(code == ArduinoParameterCode.Engine_Speed.ToString())
                                    engineRev = value;
                                else if (code == ArduinoParameterCode.Vehicle_Speed.ToString())
                                    vehicleSpeed = value;
                                else
                                    throw new InvalidOperationException("Unexpected parameter code is returned on parsing VAL. ServerType:" + wsClientType.ToString() + " Code:"  + code);
                            }
                            break;
                        case WebSocketType.ELM327:
                            foreach(string code in valJson.val.Keys)
                            {
                                double value = double.Parse(valJson.val[code]);
                                if (code == OBDIIParameterCode.Engine_Speed.ToString())
                                    engineRev = value;
                                else if (code == OBDIIParameterCode.Vehicle_Speed.ToString())
                                    vehicleSpeed = value;
                                else if (code == OBDIIParameterCode.Mass_Air_Flow.ToString())
                                    massAirFlow = value;
                                else if (code == OBDIIParameterCode.Command_equivalence_ratio.ToString())
                                    afRatio = value;
                                else if (code == OBDIIParameterCode.Engine_fuel_rate.ToString())
                                    fuelRate = value;
                                else
                                    throw new InvalidOperationException("Unexpected parameter code is returned on parsing VAL. ServerType:" + wsClientType.ToString() + " Code:" + code);
                            }
                            break;
                    }

                    //Finally fire VALMessageReceived event
                    VALMessageParsed(this, null);
                }
                else if (receivedJSONMode == ErrorJSONFormat.ModeCode)
                {
                    ErrorJSONFormat err_json = JsonConvert.DeserializeObject<ErrorJSONFormat>(jsonmsg);
                    err_json.Validate();
                    logger.Error("Error occured from " + wsClientType.ToString() + ":" + err_json.msg);
                }
                else if (receivedJSONMode == ResponseJSONFormat.ModeCode)
                {
                    ResponseJSONFormat res_json = JsonConvert.DeserializeObject<ResponseJSONFormat>(jsonmsg);
                    res_json.Validate();
                    logger.Info("Response from " + wsClientType.ToString() + ":" + res_json.msg);
                }
            }
            catch (JSONFormatsException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (JsonException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (FormatException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message + " JSON:" + jsonmsg);
                return;
            }


        }
    }
}
