using System.Threading;
using DefiSSMCOM.WebSocket;
using log4net;
using System.Xml;
using System.IO;

namespace DefiSSMCOM.Application.SSMCOM
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
        private readonly DefiSSMCOM.WebSocket.SSMCOMWebsocket ssmcomserver1 = new SSMCOMWebsocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Application()
        {
            setAppSettings(AppSettings.loadFromXml("ssmserver_settings.xml"));
            setWebSocketServerObj(ssmcomserver1);
        }
    }
}
