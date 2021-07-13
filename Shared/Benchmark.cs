using System;
using System.Linq;
using Ultima.Testing;
using Ultima.WebServices;

namespace Ultima
{
	/// <summary>
	/// Imports a web service, runs its method and checks if it's not null.
	/// </summary>
	public class Benchmark
	{
		public Benchmark(IMyContainer container) => Container = container;

		private IMyContainer Container { get; set; }

		public void ImportWebService()
		{
			using (var scope = Container.OpenScope())
			{
				var ws = new ImportHelper<Lazy<Func<IWebService>, IWebServiceMetadata>[]>();
				scope.InjectPropertiesAndFields(ws);

				var now = ws.Imported.FirstOrDefault(w => w.Metadata.WebServiceName == "GetNow");
				var getNow = now.Value();
				var result = getNow.Get(null);
				Assert.IsNotNull(result);
			}
		}
	}
}
