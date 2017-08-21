using System;
using System.Xml;
using System.IO;
using log4net;
using DefiSSMCOM.WebSocket;
using System.Threading;

namespace DefiSSMCOM.Application
{
    public class AppSettings
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// COM port name.
        /// </summary>
        public string comport;
        /// <summary>
        /// Websocket port number.
        /// </summary>
        public int websocket_port;
        /// <summary>
        /// Keep alive dummy message interval (in millisecond).
        /// </summary>
        public int keepalive_interval;

        /// <summary>
        /// Create Appsetting with reading xml file.
        /// </summary>
        /// <param name="filepath">XML file path</param>
        /// <returns>AppSettings read from XML.</returns>
        public static AppSettings loadFromXml(string filepath)
        {
            AppSettings appsetting = new AppSettings();
            FileStream fs;

            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));

            try
            {
                fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            catch (DirectoryNotFoundException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (FileNotFoundException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (System.Security.SecurityException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }

            try
            {
                appsetting =
                    (AppSettings)serializer.Deserialize(fs);
            }
            catch (XmlException ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                fs.Close();
            }

            return appsetting;
        }
    }

    public class AppSettingsWithBaudRate : AppSettings
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Communication baudrate.
        /// </summary>
        public int baudrate;

        public static AppSettingsWithBaudRate loadFromXml(string filepath)
        {
            AppSettingsWithBaudRate appsetting = new AppSettingsWithBaudRate();
            FileStream fs;

            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettingsWithBaudRate));

            try
            {
                fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            catch (DirectoryNotFoundException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (FileNotFoundException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (System.Security.SecurityException ex)
            {
                logger.Error(ex.Message);
                return appsetting;
            }

            try
            {
                appsetting =
                    (AppSettingsWithBaudRate)serializer.Deserialize(fs);
            }
            catch (XmlException ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                fs.Close();
            }

            return appsetting;
        }
    }

    public class ApplicationCommon
    {
        private AppSettings appsetting;
        private WebSocketCommon websocketServerObj;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void setAppSettings(AppSettings appsetting)
        {
            this.appsetting = appsetting;
        }

        protected void setWebSocketServerObj(WebSocketCommon websocketServerObj)
        {
            this.websocketServerObj = websocketServerObj;
        }

        public virtual void webSocketServerStart()
        {
            websocketServerObj.COMPortName = appsetting.comport;
            websocketServerObj.WebsocketPortNo = appsetting.websocket_port;
            websocketServerObj.KeepAliveInterval = appsetting.keepalive_interval;

            websocketServerObj.start();

            while (true)
            {
                Thread.Sleep(500);
                //DefiCOMスレッドが異常終了した場合は、プログラム自体も終了する。
                if (!websocketServerObj.IsCommunicationThreadAlive)
                    break;

                continue;
            }

            websocketServerObj.stop();
        }
    }
}
