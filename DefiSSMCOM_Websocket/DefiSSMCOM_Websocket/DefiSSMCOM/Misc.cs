using System;

namespace DefiSSMCOM
{
	namespace Communication
	{
		class Alert
		{
			public static void message(String content,String title)
			{
				Console.WriteLine (title);
				Console.WriteLine (content);
			}
		}
	}
}
