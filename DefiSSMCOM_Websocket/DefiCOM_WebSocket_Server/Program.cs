using System;
using System.Threading;
using System.Collections;
using DefiSSMCOM.WebSocket;
using log4net;

namespace DefiCOM_WebSocket_Server
{
	class MainClass
	{
		static private DefiSSMCOM.WebSocket.DefiCOM_Websocket deficomserver1;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
		
			deficomserver1 = new DefiSSMCOM.WebSocket.DefiCOM_Websocket ();
            deficomserver1.DefiCOM_PortName = Properties.Settings.Default.DefiCOM_PortName;
            deficomserver1.Websocket_PortNo = Properties.Settings.Default.Websocket_Port;

			Console.WriteLine("The server started successfully, press key 'q' to stop it!");

			deficomserver1.start ();

			while (Console.ReadKey().KeyChar != 'q')
			{
				Console.WriteLine();
				continue;
			}

			deficomserver1.stop ();
		}


	}
}
