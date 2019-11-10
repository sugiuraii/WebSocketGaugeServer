using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DefiSSMCOM.WebSocket;
using DefiSSMCOM.Application;
using log4net;
using System.Xml;
using System.Threading;

namespace AssettoCorsaSharedMem_WebSocket_Server
{
    class MainClass
    {
        static void Main(string[] args)
        {
            AssettoCorsaSHMApplication app1 = new AssettoCorsaSHMApplication();
            app1.Start();
        }
    }

    class AssettoCorsaSHMApplicationSetting
    {
        public int websocket_port = 2017;
        public int keepalive_interval = 60;
        public double physicsInterval = 16.6;
        public double graphicsInterval = 1000;
        public double staticInfoInterval = 1000;
    }

    public class AssettoCorsaSHMApplication
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private AssettoCorsaSHMApplicationSetting LoadSettingXml(string filepath)
        {
            //Construct XmlSerializer
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));

            System.IO.FileStream fs =
                new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            try
            {
                return (AssettoCorsaSHMApplicationSetting)serializer.Deserialize(fs);
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

        public void Start()
        {
            AssettoCorsaSHMApplicationSetting appSetting = LoadSettingXml("assettocorsa_server_settings.xml");
            AssettoCorsaSHM_Websocket websocketServerObj = new AssettoCorsaSHM_Websocket(appSetting.physicsInterval, appSetting.graphicsInterval, appSetting.staticInfoInterval);
            websocketServerObj.KeepAliveInterval = appSetting.keepalive_interval;
            websocketServerObj.WebsocketPortNo = appSetting.websocket_port;

            websocketServerObj.start();

            while (true)
            {
                Thread.Sleep(500);
                if (!websocketServerObj.IsCommunicationThreadAlive)
                    break;

                continue;
            }

            websocketServerObj.stop();
        }
    }

}
