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
        public string comport;
        public int websocket_port;
    }

    public class ApplicationCommon
    {
        private AppSettings appsetting;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ApplicationCommon()
        {
            appsetting = new AppSettings();
        }

        protected void webSocketServerStart(string appSettingXmlFileName, WebSocketCommon websocketServerObj)
        {
            load_setting_xml(appSettingXmlFileName);

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

        private void load_setting_xml(string filepath)
        {
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
                return;
            }
            catch (FileNotFoundException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
            }
            catch (System.Security.SecurityException ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
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
        }
    }
}
