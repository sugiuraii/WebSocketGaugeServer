using System.Threading;
using System.Xml;
using System.IO;
using log4net;
using DefiSSMCOM.WebSocket;

namespace DefiSSMCOM.Application.DefiCOM
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
        private readonly DefiSSMCOM.WebSocket.DefiCOMWebsocket deficomserver1 = new DefiCOMWebsocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Application()
        {
            setAppSettings(AppSettings.loadFromXml("defiserver_settings.xml"));
            setWebSocketServerObj(deficomserver1);
        }
    }
}