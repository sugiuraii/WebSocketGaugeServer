using System;
using DefiSSMCOM.WebSocket.JSON;
using DefiSSMCOM.Arduino;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket
{
    public class ArduinoCOMWebsocketSessionParam : SimpleWebsocketSessionParam<ArduinoParameterCode> 
    {
        //主要な機能はDefi/ArduinoWebsocketSessionParamは共通につき、SimpleWebsocketSessionParamに実装
    }

    public class ArduinoCOMWebsocket : WebSocketCommon
    {
        private ArduinoCOM arduinocom1;

        public ArduinoCOMWebsocket()
        {
            // Create Arduinocom
            arduinocom1 = new ArduinoCOM();
            com1 = arduinocom1;
            WebsocketPortNo = 2013;
            COMPortName = "COM1";
            arduinocom1.ArduinoPacketReceived += new EventHandler(arduinocom1_ArduinoDataReceived);
        }

        protected override WebsocketSessionParam createSessionParam()
        {
            return new ArduinoCOMWebsocketSessionParam();
        }

        protected override void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session)
        {
            ArduinoCOMWebsocketSessionParam sessionparam = (ArduinoCOMWebsocketSessionParam)session.Items["Param"];
            switch (receivedJSONmode)
            {
                case (ResetJSONFormat.ModeCode):
                    sessionparam.reset();
                    send_response_msg(session, "Arduino Websocket all parameter reset.");
                    break;
                case (ArduinoWSSendJSONFormat.ModeCode):
                    ArduinoWSSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<ArduinoWSSendJSONFormat>(message);
                    msg_obj_wssend.Validate();
                    sessionparam.Sendlist[(ArduinoParameterCode)Enum.Parse(typeof(ArduinoParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                    send_response_msg(session, "Arduino Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                    break;

                case (ArduinoWSIntervalJSONFormat.ModeCode):
                    ArduinoWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<ArduinoWSIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    sessionparam.SendInterval = msg_obj_interval.interval;

                    send_response_msg(session, "Arduino Websocket send_interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }
        
        private void arduinocom1_ArduinoDataReceived(object sender, EventArgs args)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                ArduinoCOMWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (ArduinoCOMWebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }

                ValueJSONFormat msg_data = new ValueJSONFormat();
                if (sessionparam.SendCount < sessionparam.SendInterval)
                    sessionparam.SendCount++;
                else
                {
                    foreach (ArduinoParameterCode Arduinocode in Enum.GetValues(typeof(ArduinoParameterCode)))
                    {
                        if (sessionparam.Sendlist[Arduinocode])
                        {
                            msg_data.val.Add(Arduinocode.ToString(), arduinocom1.get_value(Arduinocode).ToString());
                        }
                    }

                    if (msg_data.val.Count > 0)
                    {
                        String msg = JsonConvert.SerializeObject(msg_data);
                        session.Send(msg);
                    }
                    sessionparam.SendCount = 0;
                }
            }
        }
    }
}

