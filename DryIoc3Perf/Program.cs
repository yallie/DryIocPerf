using System;
using System.Diagnostics;
using System.Linq;
using DryIoc;
using DryIoc.MefAttributedModel;
using Serilog;
using Ultima;
using Ultima.Server;
using Ultima.WebServices;
using IWebService = Ultima.WebServices.IWebService;
using SampleScript = Ultima.Scripting.WebService;

namespace DryIoc4Perf
{
	public class Program
	{
		static void Main(string[] args)
		{
			// initialize default Serilog logger
			var logger = Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File("DryIoc2Perf.log")
				.CreateLogger();

			logger.Information("Starting up...");

			// initialize DryIoc container
			var dry = new Container().WithMef()
				.With(rules => rules
				.With(FactoryMethod.ConstructorWithResolvableArguments)
				.WithCaptureContainerDisposeStackTrace()
				.WithoutThrowIfDependencyHasShorterReuseLifespan()
				.WithDefaultReuse(Reuse.ScopedOrSingleton));

			// adaptor
			var container = new ContainerAdapter(dry);

			// register builtin services
			container.RegisterExports(Services.Assembly);

			// add dynamic registrations
			var regs = new ScriptRegistrator();
			regs.IndexScriptRegistrations(typeof(SampleScript).Assembly);
			container.AddDynamicRegistrations(regs.GetDynamicRegistrations);

			logger.Information("Started.");

			// test if everything is ok
			using (var scope = container.OpenScope())
			{
				var helper = new ImportHelper<Ultima.Log.ILogger>();
				scope.InjectPropertiesAndFields(helper);
				var log = helper.Imported;
				log.Info("Hello there. Starting the benchmark.");

				var ws = new ImportHelper<Lazy<IWebService, IWebServiceMetadata>[]>();
				scope.InjectPropertiesAndFields(ws);
				log.Info("Imported web services: {Count}", ws.Imported.Length);

				var now = ws.Imported.FirstOrDefault(w => w.Metadata.WebServiceName == "GetNow");
				var getNow = now.Value;
				dynamic result = getNow.Get(null);
				log.Info("GetNow: {Result}", result.IsoTime);
			}

			// run benchmark
			var benchmark = new Benchmark(container);
			var action = new Action(benchmark.ImportWebService);

			logger.Information("Warming up...");
			for (var i = 0; i < 5; i++)
			{
				action();
			}

			logger.Information("Running the benchmark...");
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				action();
			}

			sw.Stop();
			logger.Information("Time elapsed: {0}", sw.Elapsed);
			logger.Information("Dynamic lookup count: {0}", regs.LookupHistory.Count);
			regs.LookupTypes.ForEach(t => Console.WriteLine(t));
		}
	}
}
