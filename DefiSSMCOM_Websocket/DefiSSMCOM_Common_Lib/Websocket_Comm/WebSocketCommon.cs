using System;
using DefiSSMCOM.WebSocket.JSON;
using SuperSocket.SocketBase;
using System.Net;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using log4net;

namespace DefiSSMCOM.WebSocket
{
    public abstract class WebSocketCommon
    {
        protected readonly WebSocketServer appServer;
        protected bool running_state = false;
        protected COMCommon com1;

        /// <summary>
        /// Log4Net logger.
        /// </summary>
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Lock object to wait until websocket session is completely created (or deleted).
        /// Websocketセッション作成（消去）が完了するまではフラグ変更等の処理を待つためのlock object
        /// </summary>
        protected object create_session_busy_lock_obj = new object();

        /// <summary>
        /// Port number to listen websocket connection.
        /// </summary>
        public int WebsocketPortNo { get; set; }

        /// <summary>
        /// COM port name to communicate sensors.
        /// </summary>
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

        public bool IsCommunicationThreadAlive
        {
            get
            {
                return com1.IsCommunitateThreadAlive;
            }
        }

        /// <summary>
        /// Constructor of WebSocketCommon.
        /// </summary>
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

        /// <summary>
        /// Start server instance.
        /// </summary>
        public void start()
        {
            SuperSocket.SocketBase.Config.ServerConfig appserver_config = new SuperSocket.SocketBase.Config.ServerConfig();
            appserver_config.DisableSessionSnapshot = false;
            appserver_config.SessionSnapshotInterval = 2;
            appserver_config.Port = this.WebsocketPortNo;

            //Try to start the appServer
            if (!appServer.Setup(appserver_config)) //Setup with listening por
            {
                logger.Fatal("Failed to setup websocket server.");
            }
            if (!appServer.Start())
            {
                logger.Fatal("Failed to start websocket server.");
                return;
            }

            logger.Info("Websocket server is started. WebsocketPort:" + this.WebsocketPortNo.ToString() + " COMPort: " + this.COMPortName);

            com1.CommunicateRealtimeStart();

            this.running_state = true;
        }

        /// <summary>
        /// Stop server instance.
        /// </summary>
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

        /// <summary>
        /// Send error message to client.
        /// </summary>
        /// <param name="session">WebSocket session.</param>
        /// <param name="message">Message to send.</param>
        protected void send_error_msg(WebSocketSession session, string message)
        {
            ErrorJSONFormat json_error_msg = new ErrorJSONFormat();
            json_error_msg.msg = message;

            session.Send(json_error_msg.Serialize());
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Error("Send Error message to " + destinationAddress.ToString() + " : " + message);
        }

        /// <summary>
        /// Send response message to client.
        /// </summary>
        /// <param name="session">WebSocket session.</param>
        /// <param name="message">Message to send.</param>
        protected void send_response_msg(WebSocketSession session, string message)
        {
            ResponseJSONFormat json_response_msg = new ResponseJSONFormat();
            json_response_msg.msg = message;
            session.Send(json_response_msg.Serialize());

            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Send Response message to " + destinationAddress.ToString() + " : " + message);
        }

        private void appServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            IPAddress destinationAddress = session.RemoteEndPoint.Address;
            logger.Info("Session closed from : " + destinationAddress.ToString() + " Reason :" + reason.ToString());
        }

        private void appServer_NewSessionConnected(WebSocketSession session)
        {
            lock (create_session_busy_lock_obj)//Wait websocket session is created.
            {
                WebsocketSessionParam sendparam = createSessionParam();
                session.Items.Add("Param", sendparam);
                
                IPAddress destinationAddress = session.RemoteEndPoint.Address;
                logger.Info("New session connected from : " + destinationAddress.ToString());
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
