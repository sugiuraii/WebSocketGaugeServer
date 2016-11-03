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
            appli.start();
        }
    }

    public class Application : ApplicationCommon
    {
        private readonly DefiSSMCOM.WebSocket.DefiCOMWebsocket deficomserver1 = new DefiCOMWebsocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void start()
        {
            webSocketServerStart("defiserver_settings.xml", deficomserver1);
        }
    }
}