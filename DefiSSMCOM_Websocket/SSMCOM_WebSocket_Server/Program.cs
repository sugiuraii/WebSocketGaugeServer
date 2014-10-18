using System;
using System.Threading;
using System.Collections;
using DefiSSMCOM.WebSocket;

namespace SSMCOM_WebSocket_Server
{
	class MainClass
	{
		static private SSMCOM_Websocket ssmcomserver1;

		public static void Main (string[] args)
		{

			ssmcomserver1 = new SSMCOM_Websocket ();
			ssmcomserver1.SSMCOM_PortName = "COM6";
			ssmcomserver1.Websocket_PortNo = 2013;

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
