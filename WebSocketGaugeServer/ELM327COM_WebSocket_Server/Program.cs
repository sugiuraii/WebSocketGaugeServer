using System.Threading;
using DefiSSMCOM.WebSocket;
using log4net;
using System.Xml;
using System.IO;

namespace DefiSSMCOM.Application.ELM327COM
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            ELM327COMApplication appli = new ELM327COMApplication();
            appli.webSocketServerStart();
        }
    }

    public class ELM327COMApplication : ApplicationCommon
    {
        private readonly DefiSSMCOM.WebSocket.ELM327COM_Websocket elm327comserver1 = new ELM327COM_Websocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AppSettingsWithBaudRate appsettingWithBaudRate;

        public ELM327COMApplication()
        {
            this.appsettingWithBaudRate = AppSettingsWithBaudRate.loadFromXml("elm327server_settings.xml");
            setAppSettings(this.appsettingWithBaudRate);
            setWebSocketServerObj(elm327comserver1);
        }

        public override void webSocketServerStart()
        {
            elm327comserver1.overrideBaudRate(appsettingWithBaudRate.baudrate);
            base.webSocketServerStart();
        }
    }
}
