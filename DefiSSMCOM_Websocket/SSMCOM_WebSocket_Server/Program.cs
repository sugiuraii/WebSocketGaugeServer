using System;
using System.Threading;
using System.Collections;
using DefiSSMCOM.WebSocket;
using log4net;

namespace SSMCOM_WebSocket_Server
{
	class MainClass
	{
		static private SSMCOM_Websocket ssmcomserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main (string[] args)
		{
			ssmcomserver1 = new SSMCOM_Websocket ();
            ssmcomserver1.SSMCOM_PortName = Properties.Settings.Default.SSMCOM_PortName;
            ssmcomserver1.Websocket_PortNo = Properties.Settings.Default.Websocket_Port;
			
            Console.WriteLine("The server started successfully, press key 'q' to stop it!");

			ssmcomserver1.start ();

			while (Console.ReadKey().KeyChar != 'q')
			{
				Console.WriteLine();
				continue;
			}

			ssmcomserver1.stop ();
		}

	}
}
