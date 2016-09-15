using System;
using System.Threading;
using System.Collections;
using System.Xml;
using System.IO;
using DefiSSMCOM.WebSocket;
using log4net;

namespace DefiCOM_WebSocket_Server
{
    public class AppSettings
    {
        public string comport;
        public int websocket_port;
    }

	class MainClass
	{
		static private DefiSSMCOM.WebSocket.DefiCOMWebsocket deficomserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static AppSettings appsetting;

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
		
			deficomserver1 = new DefiSSMCOM.WebSocket.DefiCOMWebsocket ();

            try
            {
                load_setting_xml("defiserver_settings.xml");
            }
            catch (XmlException ex)
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
            catch (DirectoryNotFoundException ex)
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

            
            deficomserver1.COMPortName = appsetting.comport;
            deficomserver1.WebsocketPortNo = appsetting.websocket_port;

            deficomserver1.start();

			while (true)
			{
                Thread.Sleep(500);
                //DefiCOMスレッドが異常終了した場合は、プログラム自体も終了する。
                if (!deficomserver1.IsCOMThreadAlive)
                    break;

				continue;
			}

			deficomserver1.stop ();
		}

        private static void load_setting_xml(string filepath)
        {
            //XmlSerializerオブジェクトの作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));

            //ファイルを開く
            System.IO.FileStream fs =
                new System.IO.FileStream(filepath, System.IO.FileMode.Open);

            try
            {
                //XMLファイルから読み込み、逆シリアル化する
                appsetting =
                    (AppSettings)serializer.Deserialize(fs);

            }
            catch (XmlException ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

	}
}
