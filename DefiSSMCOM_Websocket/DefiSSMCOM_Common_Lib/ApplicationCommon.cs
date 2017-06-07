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

        public string comport;
        public int websocket_port;

        public static AppSettings loadFromXml(string filepath)
        {
            AppSettings appsetting = new AppSettings();
            FileStream fs;

            //XmlSerializerオブジェクトの作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));

            try
            {
                //ファイルを開く
                fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open);
            }
            catch (DirectoryNotFoundException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (FileNotFoundException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (System.Security.SecurityException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }

            try
            {
                //XMLファイルから読み込み、逆シリアル化する
                appsetting =
                    (AppSettings)serializer.Deserialize(fs);
            }
            catch (XmlException ex)
            {
                //Console.WriteLine(ex.Message);
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
        public int baudrate;
        public static AppSettingsWithBaudRate loadFromXml(string filepath)
        {
            AppSettingsWithBaudRate appsetting = new AppSettingsWithBaudRate();
            FileStream fs;

            //XmlSerializerオブジェクトの作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettingsWithBaudRate));

            try
            {
                //ファイルを開く
                fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open);
            }
            catch (DirectoryNotFoundException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (FileNotFoundException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }
            catch (System.Security.SecurityException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return appsetting;
            }

            try
            {
                //XMLファイルから読み込み、逆シリアル化する
                appsetting =
                    (AppSettingsWithBaudRate)serializer.Deserialize(fs);
            }
            catch (XmlException ex)
            {
                //Console.WriteLine(ex.Message);
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

            websocketServerObj.start();

            while (true)
            {
                Thread.Sleep(500);
                //DefiCOMスレッドが異常終了した場合は、プログラム自体も終了する。
                if (!websocketServerObj.IsCOMThreadAlive)
                    break;

                continue;
            }

            websocketServerObj.stop();
        }
    }
}
