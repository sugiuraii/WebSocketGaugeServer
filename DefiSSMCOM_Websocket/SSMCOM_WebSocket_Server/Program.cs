using System;
using System.Threading;
using System.Collections;
using DefiSSMCOM.WebSocket;

namespace DefiSSMCOM.WebSocket.SSM
{
	class MainClass
	{
		static private SSMCOM_Websocket ssmcomserver1;

		public static void Main (string[] args)
		{

			ssmcomserver1 = new SSMCOM_Websocket ();
			ssmcomserver1.SSMCOM_PortName = "COM4";
			ssmcomserver1.Websocket_PortNo = 2012;

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
