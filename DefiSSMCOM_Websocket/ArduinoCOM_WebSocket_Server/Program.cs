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
            ArduinoCOMApplication appli = new ArduinoCOMApplication();
            appli.webSocketServerStart();
        }
    }

    public class ArduinoCOMApplication : ApplicationCommon
    {
        private readonly DefiSSMCOM.WebSocket.ArduinoCOMWebsocket arduinocomserver1 = new ArduinoCOMWebsocket();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AppSettingsWithBaudRate appsettingWithBaudRate;

        public ArduinoCOMApplication()
        {
            this.appsettingWithBaudRate = AppSettingsWithBaudRate.loadFromXml("arduinoserver_settings.xml");
            setAppSettings(this.appsettingWithBaudRate);
            setWebSocketServerObj(arduinocomserver1);
        }

        public override void webSocketServerStart()
        {
            arduinocomserver1.overrideBaudRate(appsettingWithBaudRate.baudrate);
            base.webSocketServerStart();
        }
    }
}
