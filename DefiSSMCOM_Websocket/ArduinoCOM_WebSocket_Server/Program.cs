using System.Threading;
using System.Xml;
using System.IO;
using DefiSSMCOM.WebSocket;
using log4net;

namespace DefiSSMCOM.Application.ArduinoCOM
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Application appli = new Application();
            appli.webSocketServerStart();
        }
    }

    public class Application : ApplicationCommon
    {
        private readonly DefiSSMCOM.WebSocket.ArduinoCOMWebsocket arduinocomserver1 = new ArduinoCOMWebsocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Application()
        {
            setAppSettings(AppSettings.loadFromXml("arduinoserver_settings.xml"));
            setWebSocketServerObj(arduinocomserver1);
        }
    }
}
