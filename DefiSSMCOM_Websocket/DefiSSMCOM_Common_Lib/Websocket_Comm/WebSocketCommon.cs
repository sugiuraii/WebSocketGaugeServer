using System;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using log4net;

namespace DefiSSMCOM.WebSocket
{
    public abstract class WebsocketSessionParam
    {
        public abstract void reset();
    }

    public class SimpleWebsocketSessionParam<parameterCodeType> : WebsocketSessionParam where parameterCodeType : struct
    {
        public Dictionary<parameterCodeType, bool> Sendlist;
        public int SendInterval { get; set; }
        public int SendCount { get; set; }

        public SimpleWebsocketSessionParam()
        {
            this.Sendlist = new Dictionary<parameterCodeType, bool>();

            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.Sendlist.Add(code, false);
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }

        public override void reset()
        {
            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.Sendlist[code] = false;
            }

            this.SendInterval = 0;
            this.SendCount = 0;
        }
    }

    public class SlowFastWebsocketSessionParam<parameterCodeType> : WebsocketSessionParam where parameterCodeType : struct
    {
        public Dictionary<parameterCodeType, bool> SlowSendlist, FastSendlist;

        public SlowFastWebsocketSessionParam()
		{
            this.SlowSendlist = new Dictionary<parameterCodeType, bool>();
            this.FastSendlist = new Dictionary<parameterCodeType, bool>();

            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.SlowSendlist.Add(code, false);
                this.FastSendlist.Add(code, false);
            }
		}

		public override void reset()
		{
            foreach (parameterCodeType code in Enum.GetValues(typeof(parameterCodeType)))
            {
                this.SlowSendlist[code] = false;
                this.FastSendlist[code] = false;
            }
		}

    }

    public abstract class WebSocketCommon
    {
        protected WebSocketServer appServer;
        protected bool running_state = false;
        protected COMCommon com1;

        //log4net
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Websocketセッション作成（消去）が完了するまではフラグ変更等の処理を待つためのlock object
        protected object create_session_busy_lock_obj = new object();

        public int WebsocketPortNo { get; set; }

        public string COMPortName
        {
            get
            {
                return com1.PortName;
            }
            set
            {
                com1.PortName = value;
            }
        }

        public bool IsCOMThreadAlive
        {
            get
            {
                return com1.IsCommunitateThreadAlive;
            }
        }

        public WebSocketCommon()
        {
            // Create Websocket server
            appServer = new WebSocketServer();
            appServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(appServer_SessionClosed);
            appServer.NewSessionConnected += new SessionHandler<WebSocketSession>(appServer_NewSessionConnected);
            appServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
        }

        // SessionParam(DefiSessionParam or SSMSessionParamを生成して返すメソッド
        protected abstract WebsocketSessionParam createSessionParam();

        public void start()
        {
            SuperSocket.SocketBase.Config.ServerConfig appserver_config = new SuperSocket.SocketBase.Config.ServerConfig();
            appserver_config.DisableSessionSnapshot = false;
            appserver_config.SessionSnapshotInterval = 2;
            appserver_config.Port = this.WebsocketPortNo;

            //Try to start the appServer
            if (!appServer.Setup(appserver_config)) //Setup with listening por
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

            //Console.WriteLine("Websocket server is started. WebsocketPort:" + this.Websocket_PortNo.ToString() + " DefiCOMPort: " + this.DefiCOM_PortName);
            logger.Info("Websocket server is started. WebsocketPort:" + this.WebsocketPortNo.ToString() + " DefiCOMPort: " + this.COMPortName);

            com1.CommunicateRealtimeStart();

            this.running_state = true;
        }

        public void stop()
        {
            if (!this.running_state)
            {
                //Console.WriteLine("Websocket server is not running");
                logger.Error("Websocket stop is called. But the websocket server is not running.");
                return;
            }
            //Stop the appServer
            appServer.Stop();
            this.running_state = false;

            Console.WriteLine();
            //Console.WriteLine("The server was stopped!");
            logger.Info("Websocket server is stopped");

            com1.CommunicateRealtimeStop();
        }

        protected void send_error_msg(WebSocketSession session, string message)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;

            session.Send(json_error_msg.Serialize());
            //Console.WriteLine("Send Error message to " + session.Host + " : " + message);
            logger.Error("Send Error message to " + session.Host + " : " + message);
        }

        protected void send_response_msg(WebSocketSession session, string message)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;
            session.Send(json_response_msg.Serialize());

            //Console.WriteLine("Send Response message to " + session.Host + " : " + message);
            logger.Info("Send Response message to " + session.Host + " : " + message);
        }

        private void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            //Console.WriteLine("Session closed from : " + session.Host + " Reason :" + reason.ToString());
            logger.Info("Session closed from : " + session.Host + " Reason :" + reason.ToString());
        }

        private void appServer_NewSessionConnected(WebSocketSession session)
        {
            lock (create_session_busy_lock_obj)//Websocketセッション作成処理が終わるまで、後続のパケット処理を待つ
            {
                WebsocketSessionParam sendparam = createSessionParam();
                session.Items.Add("Param", sendparam);

                //Console.WriteLine("New session connected from : " + session.Host);
                logger.Info("New session connected from : " + session.Host);
            }
        }

        protected abstract void processReceivedJSONMessage(string receivedJSONmode, string message, WebSocketSession session);

        private void appServer_NewMessageReceived(WebSocketSession session, string message)
        {
            lock (create_session_busy_lock_obj)//Websocketセッション作成処理が終わるまで、後続のパケット処理を待つ
            {
                // Get session parameter
                WebsocketSessionParam sessionparam;
                try
                {
                    sessionparam = (WebsocketSessionParam)session.Items["Param"];
                }
                catch (KeyNotFoundException ex)
                {
                    logger.Warn("Sesssion param is not set. Exception message : " + ex.Message + " " + ex.StackTrace);
                    return;
                }

                // Check if the message is empty
                if (message == "")
                {
                    send_error_msg(session, "Empty message is received.");
                    return;
                }

                // Parse JSON message mode
                string received_JSON_mode;
                try
                {
                    Dictionary<string,string> msg_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                    received_JSON_mode = msg_dict["mode"];
                    processReceivedJSONMessage(received_JSON_mode, message, session);
                }
                catch (KeyNotFoundException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                }
                catch (JsonException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                }
                catch (JSONFormatsException ex)
                {
                    send_error_msg(session, ex.GetType().ToString() + " " + ex.Message);
                }

            }
        }

    }
}
