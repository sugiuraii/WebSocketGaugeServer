using System;
using System.Xml;
using System.IO;
using log4net;
using System.Threading;

namespace FUELTRIP_Logger
{
    public class AppSettings
    {
        public string defiserver_url;
        public string ssmserver_url;
        public int websocket_port;
    }

	class MainClass
	{

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static AppSettings appsetting;
		public static void Main (string[] args)
		{
            try
            {
                load_setting_xml("fueltriplogger_settings.xml");
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

			FUELTRIP_Logger fueltriplogger1 = new FUELTRIP_Logger (appsetting.defiserver_url,appsetting.ssmserver_url);
            fueltriplogger1.WebsocketServer_ListenPortNo = appsetting.websocket_port;

			Console.WriteLine("The server started successfully, press key 'q' to stop it!");

			fueltriplogger1.start ();

			while (Console.ReadKey().KeyChar != 'q')
			{
                Thread.Sleep(500);
				continue;
			}

			fueltriplogger1.stop ();
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
