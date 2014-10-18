using System;
using log4net;

namespace FUELTRIP_Logger
{
	class MainClass
	{

        //log4net
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main (string[] args)
		{
			FUELTRIP_Logger fueltriplogger1 = new FUELTRIP_Logger (Properties.Settings.Default.DefiCOM_Websocket_URL,Properties.Settings.Default.SSMCOM_WebSocket_URL);
            fueltriplogger1.WebsocketServer_ListenPortNo = Properties.Settings.Default.Listen_Port;

			Console.WriteLine("The server started successfully, press key 'q' to stop it!");

			fueltriplogger1.start ();

			while (Console.ReadKey().KeyChar != 'q')
			{
				Console.WriteLine();
				continue;
			}

			fueltriplogger1.stop ();
		}
	}
}
