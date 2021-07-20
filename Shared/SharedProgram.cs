using System;
using System.Linq;
using Ultima.Server;
using Ultima.WebServices;
using DryIocPerf;
using IWebService = Ultima.WebServices.IWebService;
using SampleScript = Ultima.Scripting.WebService;
using System.Diagnostics;

namespace Ultima
{
	/// <summary>
	/// Shared code for the Program.
	/// </summary>
	public static class SharedProgram
	{
		public delegate void Tracer(string format, params object[] arguments);

		public static void DynamicBenchmark(ContainerAdapter container, Tracer trace)
		{
			// register builtin services
			container.RegisterExports(Services.Assembly);

			// add dynamic registrations
			var regs = new ScriptRegistrator();
			regs.IndexScriptRegistrations(typeof(SampleScript).Assembly);
			container.AddDynamicRegistrations(regs.GetDynamicRegistrations);

			trace("Started dynamic benchmark.");

			// test if everything is ok
			using (var scope = container.OpenScope())
			{
				var helper = new ImportHelper<Ultima.Log.ILogger>();
				scope.InjectPropertiesAndFields(helper);
				var log = helper.Imported;
				log.Info("Hello there. Dynamic benchmark works fine.");

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

			trace("Warming up...");
			for (var i = 0; i < 5; i++)
			{
				action();
			}

			trace("Running the benchmark...");
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				action();
			}

			sw.Stop();
			trace("Time elapsed: {0}", sw.Elapsed);

			trace("Dynamic lookup count: {0}", regs.LookupHistory.Count);
			//regs.LookupTypes.ForEach(t => Console.WriteLine(t));

			trace("Succeeded lookup count: {0}", regs.SucceededLookupHistory.Count);
			//regs.SucceededLookupTypes.ForEach(t => Console.WriteLine(t));
		}

		public static void StaticBenchmark(ContainerAdapter container, Tracer trace)
		{
			// register builtin services and scripts
			container.RegisterExports(Services.Assembly);
			container.RegisterExports(typeof(SampleScript).Assembly);

			trace("Started static benchmark.");

			// test if everything is ok
			using (var scope = container.OpenScope())
			{
				var helper = new ImportHelper<Ultima.Log.ILogger>();
				scope.InjectPropertiesAndFields(helper);
				var log = helper.Imported;
				log.Info("Hello there. Static benchmark works fine.");

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

			trace("Warming up...");
			for (var i = 0; i < 5; i++)
			{
				action();
			}

			trace("Running the benchmark...");
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				action();
			}

			sw.Stop();
			trace("Time elapsed: {0}", sw.Elapsed);
		}
	}
}
