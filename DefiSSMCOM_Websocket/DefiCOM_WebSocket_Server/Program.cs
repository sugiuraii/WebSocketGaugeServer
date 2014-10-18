using System;
using System.Threading;
using System.Collections;
using DefiSSMCOM.WebSocket;

namespace DefiSSMCOM.WebSocket.Defi
{
	class MainClass
	{
		static private DefiSSMCOM.WebSocket.DefiCOM_Websocket deficomserver1;

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
		
			deficomserver1 = new DefiSSMCOM.WebSocket.DefiCOM_Websocket ();
			deficomserver1.DefiCOM_PortName = "COM4";
			deficomserver1.Websocket_PortNo = 2012;

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
