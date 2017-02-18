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
		static private DefiSSMCOM.WebSocket.DefiCOM_Websocket deficomserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static AppSettings appsetting;

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
		
			deficomserver1 = new DefiSSMCOM.WebSocket.DefiCOM_Websocket ();

            try
            {
                load_setting_xml("defiserver_settings.xml");
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
            }
            catch (System.Security.SecurityException ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
                return;
            }

            
            deficomserver1.DefiCOM_PortName = appsetting.comport;
            deficomserver1.Websocket_PortNo = appsetting.websocket_port;

            deficomserver1.start();

			Console.WriteLine("The server started successfully, press key 'q' to stop it!");


			while (Console.ReadKey().KeyChar != 'q')
			{
                Thread.Sleep(500);
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
