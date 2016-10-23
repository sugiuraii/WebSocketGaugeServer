using System.Threading;
using DefiSSMCOM.WebSocket;
using log4net;
using System.Xml;
using System.IO;

namespace ELM327COM_WebSocket_Server
{
    public class AppSettings
    {
        public string comport;
        public int websocket_port;
    }

    class Program
    {
        static private ELM327COM_Websocket elm327comserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static AppSettings appsetting;

        public static void Main(string[] args)
        {
            elm327comserver1 = new ELM327COM_Websocket();

            try
            {
                load_setting_xml("elm327server_settings.xml");
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

            elm327comserver1.COMPortName = appsetting.comport;
            elm327comserver1.WebsocketPortNo = appsetting.websocket_port;

            elm327comserver1.start();
            System.Console.WriteLine("Start");

            while (true)
            {
                Thread.Sleep(500);
                if (!elm327comserver1.IsCOMThreadAlive)
                    break;
                continue;
            }

            elm327comserver1.stop();
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
