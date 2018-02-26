using System;
using DefiSSMCOM.WebSocket.JSON;
using DefiSSMCOM.Defi;
using SuperSocket.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket
{
    public class DefiCOMWebsocketSessionParam : SimpleWebsocketSessionParam<DefiParameterCode>
    {
        //主要な機能はDefi/ArduinoWebsocketSessionParamは共通につき、SimpleWebsocketSessionParamに実装
    }

    public class DefiCOMWebsocket : WebSocketCommon
    {
        private readonly DefiCOM deficom1;

        public DefiCOMWebsocket()
        {
            // Create Deficom
            deficom1 = new DefiCOM();
            com1 = deficom1;
            WebsocketPortNo = 2013;
            COMPortName = "COM1";
            deficom1.DefiPacketReceived += new EventHandler(deficom1_DefiDataReceived);
        }

        protected override WebsocketSessionParam createSessionParam()
        {
            return new DefiCOMWebsocketSessionParam();
        }

        protected override void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session)
        {
            DefiCOMWebsocketSessionParam sessionparam = (DefiCOMWebsocketSessionParam)session.Items["Param"];
            switch (receivedJSONmode)
            {
                case (ResetJSONFormat.ModeCode):
                    sessionparam.reset();
                    send_response_msg(session, "Defi Websocket all parameter reset.");
                    break;
                case (DefiWSSendJSONFormat.ModeCode):
                    DefiWSSendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<DefiWSSendJSONFormat>(message);
                    msg_obj_wssend.Validate();
                    sessionparam.Sendlist[(DefiParameterCode)Enum.Parse(typeof(DefiParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                    send_response_msg(session, "Defi Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                    break;

                case (DefiWSIntervalJSONFormat.ModeCode):
                    DefiWSIntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<DefiWSIntervalJSONFormat>(message);
                    msg_obj_interval.Validate();
                    sessionparam.SendInterval = msg_obj_interval.interval;

                    send_response_msg(session, "Defi Websocket send_interval to : " + msg_obj_interval.interval.ToString());
                    break;
                default:
                    throw new JSONFormatsException("Unsuppoted mode property.");
            }
        }
        
        private void deficom1_DefiDataReceived(object sender, EventArgs args)
        {
            var sessions = appServer.GetAllSessions();

            foreach (var session in sessions)
            {
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

                DefiCOMWebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (DefiCOMWebsocketSessionParam)session.Items["Param"];
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
                    foreach (DefiParameterCode deficode in Enum.GetValues(typeof(DefiParameterCode)))
                    {
                        if (sessionparam.Sendlist[deficode])
                        {
                            msg_data.val.Add(deficode.ToString(), deficom1.get_value(deficode).ToString());
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

