using System;
using System.Threading;
using DefiSSMCOM.Communication.Defi;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections;

namespace DefiLinkTest1
{


	class MainClass
	{
		static private DefiSSMCOM_Websocket.DefiCOM_Websocket deficomserver1;

		MainClass()
		{
		}

		public static void Main (string[] args)
		{
		
			deficomserver1 = new DefiSSMCOM_Websocket.DefiCOM_Websocket ();
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
