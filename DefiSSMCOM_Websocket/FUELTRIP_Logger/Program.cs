using System;

namespace FUELTRIP_Logger
{
	class MainClass
	{
		private const string deficom_ws_URL = "ws://localhost:2012/";
		private const string ssmcom_ws_URL = "ws://localhost:2013/";

		public static void Main (string[] args)
		{
			FUELTRIP_Logger fueltriplogger1 = new FUELTRIP_Logger (deficom_ws_URL,ssmcom_ws_URL);
			fueltriplogger1.WebsocketServer_ListenPortNo = 2014;

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
