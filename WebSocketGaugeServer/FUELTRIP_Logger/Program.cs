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
        public string arduinoserver_url;
        public string elm327server_url;
        public int websocket_port;
        public int keepalive_interval;
        public calculation Calculation;

        public class calculation
        {
            public fuelCalculationMethod FuelCalculationMethod;
            public dataSource DataSource;
            public FuelTripCalculatorOption CalculationOption;
        }

        public enum fuelCalculationMethod
        {
            RPM_INJECTION_PW,
            MASS_AIR_FLOW,
            MASS_AIR_FLOW_AF,
            FUEL_RATE
        }

        public enum DataSourceType
        {
            DEFI,
            SSM,
            ARDUINO,
            ELM327
        }

        public class dataSource
        {

            public DataSourceType VehicleSpeedSource;
            public DataSourceType RPMSource;
            public DataSourceType InjectionPWSource;
            public DataSourceType MassAirFlowSource;
            public DataSourceType AFRatioSource;
            public DataSourceType FuelRateSource;
        }
        /*
        public void ValidateSettings()
        {
            dataSource datasource = this.Calculation.DataSource;


        }
        */
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

			FUELTRIPLogger fueltriplogger1 = new FUELTRIPLogger(appsetting);
            fueltriplogger1.WebsocketServer_ListenPortNo = appsetting.websocket_port;
            fueltriplogger1.KeepAliveInterval = appsetting.keepalive_interval;

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
                new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

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
