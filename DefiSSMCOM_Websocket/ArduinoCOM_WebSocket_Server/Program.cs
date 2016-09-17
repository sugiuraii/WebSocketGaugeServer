using System.Threading;
using System.Xml;
using System.IO;
using log4net;

namespace ArduinoCOM_WebSocket_Server
{
    public class AppSettings
    {
        public string comport;
        public int websocket_port;
    }

    class MainClass
    {
        static private DefiSSMCOM.WebSocket.ArduinoCOMWebsocket arduinocomserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static AppSettings appsetting;

        MainClass()
        {
        }

        public static void Main(string[] args)
        {

            arduinocomserver1 = new DefiSSMCOM.WebSocket.ArduinoCOMWebsocket();

            try
            {
                load_setting_xml("arduinoserver_settings.xml");
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


            arduinocomserver1.COMPortName = appsetting.comport;
            arduinocomserver1.WebsocketPortNo = appsetting.websocket_port;

            arduinocomserver1.start();

            while (true)
            {
                Thread.Sleep(500);
                //DefiCOMスレッドが異常終了した場合は、プログラム自体も終了する。
                if (!arduinocomserver1.IsCOMThreadAlive)
                    break;

                continue;
            }

            arduinocomserver1.stop();
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
