using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultima
{
	/// <summary>
	/// Simple helper class that allows to import a service using MEF API.
	/// </summary>
	public class ImportHelper<T>
	{
		[Import]
		public T Imported { get; set; }
	}
}
