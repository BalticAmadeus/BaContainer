using System;

namespace BalticAmadeus.Container.ConsoleDemo
{
	public class ConsoleOutput: IOutput
	{
		public void WriteLine(string text)
		{
			Console.WriteLine(text);
		}
	}
}