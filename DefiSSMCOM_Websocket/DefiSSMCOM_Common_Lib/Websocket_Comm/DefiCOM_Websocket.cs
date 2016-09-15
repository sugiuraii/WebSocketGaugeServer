using System;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using log4net;

namespace DefiSSMCOM.WebSocket
{
	public class DefiCOMWebsocketSessionparam : WebsocketSessionParam
	{
		public Dictionary<DefiParameterCode,bool> Sendlist;
		public int SendInterval;
		public int SendCount;

		public DefiCOMWebsocketSessionparam()
		{
			this.Sendlist = new Dictionary<DefiParameterCode,bool> ();

			foreach (DefiParameterCode code in Enum.GetValues(typeof(DefiParameterCode)))
			{
				this.Sendlist.Add(code, false);
			}

			this.SendInterval = 0;
			this.SendCount = 0;
		}

		public override void reset()
		{
			foreach (DefiParameterCode code in Enum.GetValues(typeof(DefiParameterCode)))
			{
				this.Sendlist[code]= false;
			}

			this.SendInterval = 0;
			this.SendCount = 0;
		}
	}

    public class DefiCOMWebsocket : WebSocketCommon
	{
		private DefiCOM deficom1;

		public DefiCOMWebsocket ()
		{
			// Create Deficom
			deficom1 = new DefiCOM ();
            com1 = deficom1;
			WebsocketPortNo = 2013;
			COMPortName = "COM1";
			deficom1.DefiPacketReceived += new EventHandler (deficom1_DefiLinkPacketReceived);

			appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
		}

        protected override WebsocketSessionParam createSessionParam()
        {
            return new DefiCOMWebsocketSessionparam();
        }

		private void appServer_NewMessageReceived(WebSocketSession session, string message)
		{
            lock (create_session_busy_lock_obj)//Websocketセッション作成処理が終わるまで、後続のパケット処理を待つ
            {
                DefiCOMWebsocketSessionparam sessionparam;
                try
                {
                    sessionparam = (DefiCOMWebsocketSessionparam)session.Items["Param"];
                    //Console.WriteLine (message);
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    return;
                }

                if (message == "")
                {
                    send_error_msg(session, "Empty message is received.");
                    return;
                }
                string received_JSON_mode;
                try
                {
                    var msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                    received_JSON_mode = msg_dict["mode"];
                }
                catch (KeyNotFoundException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
                catch (JsonException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }

                try
                {
                    switch (received_JSON_mode)
                    {
                        case ("RESET"):
                            sessionparam.reset();
                            send_response_msg(session, "Defi Websocket all parameter reset.");
                            break;
                        case ("DEFI_WS_SEND"):
                            Defi_WS_SendJSONFormat msg_obj_wssend = JsonConvert.DeserializeObject<Defi_WS_SendJSONFormat>(message);
                            msg_obj_wssend.Validate();
                            sessionparam.Sendlist[(DefiParameterCode)Enum.Parse(typeof(DefiParameterCode), msg_obj_wssend.code)] = msg_obj_wssend.flag;

                            send_response_msg(session, "Defi Websocket send_flag for : " + msg_obj_wssend.code.ToString() + " set to : " + msg_obj_wssend.flag.ToString());
                            break;

                        case ("DEFI_WS_INTERVAL"):
                            Defi_WS_IntervalJSONFormat msg_obj_interval = JsonConvert.DeserializeObject<Defi_WS_IntervalJSONFormat>(message);
                            msg_obj_interval.Validate();
                            sessionparam.SendInterval = msg_obj_interval.interval;

                            send_response_msg(session, "Defi Websocket send_interval to : " + msg_obj_interval.interval.ToString());
                            break;
                        default:
                            throw new JSONFormatsException("Unsuppoted mode property.");
                    }
                }
                catch (JSONFormatsException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
                catch (JsonException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                    return;
                }
            }
		}

		private void deficom1_DefiLinkPacketReceived(object sender,EventArgs args)
		{
			var sessions = appServer.GetAllSessions ();

			foreach (var session in sessions) 
			{
                if (session == null || !session.Connected || session.Connection == "") // Avoid null session bug
                    continue;

				ValueJSONFormat msg_data = new ValueJSONFormat ();

                DefiCOMWebsocketSessionparam sendparam;
                try
                {
                    sendparam = (DefiCOMWebsocketSessionparam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    continue;
                }
                
                if (sendparam.SendCount < sendparam.SendInterval)
					sendparam.SendCount++;
				else {
					foreach (DefiParameterCode deficode in Enum.GetValues(typeof(DefiParameterCode) )) {
						if (sendparam.Sendlist [deficode]) {
							msg_data.val.Add(deficode.ToString(),deficom1.get_value(deficode).ToString());
						}
					}

                    if (msg_data.val.Count > 0)
                    {
                        String msg = JsonConvert.SerializeObject(msg_data);
                        session.Send(msg);
                    }
					sendparam.SendCount = 0;
				}
			}
		}
	}
}

