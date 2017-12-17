using System;
using System.Xml;
using System.IO;
using log4net;
using System.Threading;

namespace FUELTRIP_Logger
{
	class MainClass
	{
        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main (string[] args)
		{
            AppSettings appsetting;
            try
            {
                appsetting = LoadSettingXml("fueltriplogger_settings.xml");
                appsetting.ValidateSettings();
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
            catch(ArgumentException ex)
            {
                logger.Error(ex.Message);
                return;
            }
            RequiredParameterCode codelist = appsetting.getRequiredParameterCodes();

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

        private static AppSettings LoadSettingXml(string filepath)
        {
            //Construct XmlSerializer
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));

            System.IO.FileStream fs =
                new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            try
            {
                return (AppSettings)serializer.Deserialize(fs);
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
