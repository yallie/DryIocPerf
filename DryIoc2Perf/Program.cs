using System;
using System.Diagnostics;
using System.Linq;
using DryIoc;
using DryIoc.MefAttributedModel;
using Serilog;
using Ultima;
using Ultima.Server;
using Ultima.WebServices;


namespace DryIocPerf
{
	public class Program
	{
		static void Main(string[] args)
		{
			// initialize default Serilog logger
			var logger = Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File("DryIoc3Perf.log")
				.CreateLogger();

			logger.Information("Starting up...");

			// DryIoc container factory
			Func<IContainer> dryFactory = () => new Container().WithMef()
				.With(rules => rules
				.With(FactoryMethod.ConstructorWithResolvableArguments)
				.WithCaptureContainerDisposeStackTrace()
				.WithoutThrowIfDependencyHasShorterReuseLifespan()
				.WithDefaultReuseInsteadOfTransient(Reuse.ScopedOrSingleton));

			// run benchmarks
			SharedProgram.DynamicBenchmark(new ContainerAdapter(dryFactory()), logger.Information);
			SharedProgram.StaticBenchmark(new ContainerAdapter(dryFactory()), logger.Information);
		}
	}
}
