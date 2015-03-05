using System;

namespace BalticAmadeus.Container.ConsoleDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			// basic container usage demo
			var builder = new ContainerBuilder();
			builder.For<IOutput>().Use<ConsoleOutput>();

			using (var container = builder.Build())
			{
				var output = container.Resolve<IOutput>();

				output.WriteLine("Testing..");

				container.Release(output);
			}

			Console.Read();
		}
	}
}
