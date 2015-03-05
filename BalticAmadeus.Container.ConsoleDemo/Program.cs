using System;

namespace BalticAmadeus.Container.ConsoleDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			// basic container usage demo
			var builder = new ContainerBuilder();

		    builder.For<FileLogger>().Use(() => new FileLogger("output.txt"));
			builder.For<IOutput>().Use<ConsoleOutput>().DecorateWithProxy<FileLoggingProxy>();

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
