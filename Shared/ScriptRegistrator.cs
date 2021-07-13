using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DryIoc;
using DryIoc.MefAttributedModel;
using Ultima.Linq;
using Zyan.Communication.Toolbox;
using ExportedRegistrationInfo = DryIoc.MefAttributedModel.ExportedRegistrationInfo;

namespace Ultima
{
	/// <summary>
	/// Registers dynamically compiled server-side scripts in the container.
	/// </summary>
	internal class ScriptRegistrator
	{
		/// <summary>
		/// Gets the compiled script assembly and indexes all registrations by service type name.
		/// The real application deals with many small assemblies, created out of the compiled scripts.
		/// In the benchmark, all scripts are compiled into one assembly.
		/// </summary>
		/// <param name="asm">The assembly to scan and register.</param>
		public void IndexScriptRegistrations(Assembly asm)
		{
			var registrations = AttributedModel
				.Scan(new[] { asm })
				.Select(r => new
				{
					TypeNames = r.Exports.Select(x => GetSimpleTypeName(x.ServiceTypeFullName)),
					Registration = r.EnsureUniqueExportServiceKeys(ServiceKeyStore).MakeLazy(),
				})
				.ToArray();

			foreach (var lazyRegistration in registrations)
			{
				var exports = lazyRegistration.Registration.Exports;
				for (var i = 0; i < exports.Length; i++)
				{
					var export = exports[i];
					var serviceTypeFullName = GetSimpleTypeName(export.ServiceTypeFullName);
					var regs = IndexedRegistrations.GetOrAdd(serviceTypeFullName, s => new List<KeyedRegistration>());
					lock (regs)
					{
						regs.Add(new KeyedRegistration
						{
							Key = export.ServiceKey,
							Registration = lazyRegistration.Registration,

							// in the real project, ScriptData represents a small lazily-compiled assembly
							// containing a few service implementations and their serialized export registrations
							ScriptData = new ScriptData
							{
								ServiceTypes = new[] { serviceTypeFullName },
								Assembly = asm,
								Registrations = new[] { lazyRegistration.Registration },
							}
						});
					}
				}
			}
		}

		// Container index: ServiceTypeFullName -> ServiceRegistrations
		private ConcurrentDictionary<string, List<KeyedRegistration>> IndexedRegistrations { get; } =
			new ConcurrentDictionary<string, List<KeyedRegistration>>();

		private class KeyedRegistration
		{
			public object Key { get; set; }
			public ExportedRegistrationInfo Registration { get; set; }
			public ScriptData ScriptData { get; set; }
			private DynamicRegistration dynamicRegistration;

			public DynamicRegistration DynamicRegistration
			{
				get
				{
					if (dynamicRegistration == null)
					{
						var factory = Registration.CreateFactory(ScriptData.GetType);
						dynamicRegistration = new DynamicRegistration(factory, serviceKey: Key);
					}

					return dynamicRegistration;
				}
			}

			public override string ToString() => ScriptData.ToString();
		}

		private class ScriptData
		{
			public Assembly Assembly { get; set; }
			public string[] ServiceTypes { get; set; }
			public ExportedRegistrationInfo[] Registrations { get; set; }
			public override string ToString() => $"Types: {ServiceTypes}, Services: {string.Join(", ", ServiceTypes)}";
			public Type GetType(string typeName)
			{
				var result = ScriptRegistrator.GetType(Assembly, typeName);
				return result;
			}
		}

		public IEnumerable<DynamicRegistration> GetDynamicRegistrations(Type serviceType, object serviceKey)
		{
			if (serviceType == null)
			{
				return null;
			}

			// Use type.ToString() instead of type.FullName to avoid assembly-qualified generic parameter names, i.e.
			// System.Collections.Generic.List`1[System.Int32] instead of
			// System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
			var typeFullName = serviceType.ToString();
			if (!IndexedRegistrations.TryGetValue(typeFullName, out var regs))
			{
				return null;
			}

			lock (regs)
			{
				// DryIoc filters by the service key, so we don't bother doing it ourselves
				return regs.Select(r => r.DynamicRegistration).ToArray();
			}
		}

		/// <summary>
		/// Returns the same string as type.ToString(), without assembly names, versions, and public key tokens.
		/// Note: type.ToString() isn't the same as type.FullName or type.AssemblyQualifiedName, it's normally shorter.
		/// </summary>
		/// <param name="assemblyQualifiedTypeName">Assembly-qualified type name.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string GetSimpleTypeName(string assemblyQualifiedTypeName)
		{
			if (assemblyQualifiedTypeName == null)
			{
				return null;
			}

			var commaIndex = assemblyQualifiedTypeName.IndexOf(',');
			if (commaIndex < 0)
			{
				// type name doesn't have assembly+version part
				return string.Intern(assemblyQualifiedTypeName);
			}

			if (assemblyQualifiedTypeName.IndexOf('[') < 0)
			{
				// not a generic type, so we just strip off the assembly part
				return string.Intern(assemblyQualifiedTypeName.Substring(0, commaIndex));
			}

			// it's a generic type, we need to get rid of all occurrences of the assembly parts
			var typeName = AssemblyNameVersionRegex.Replace(assemblyQualifiedTypeName, string.Empty);
			return string.Intern(typeName);
		}

		// 1. Removes the ", mscorlib, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null" parts
		// 2. Removes extra square braces around the type parameters, i.e. "[System.Int32]" -> "System.Int32"
		private static Regex AssemblyNameVersionRegex { get; } =
			new Regex(@"\, \s* [\w\.]+ (\, \s*[\w]+\=[\w\d\.]+)+ \]?
				|\[ (?=\[)
				|(?<=\,) \[",
			RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

		private static Type GetType(Assembly assembly, string typeName)
		{
			var result = assembly.GetType(typeName);
			if (result == null)
			{
				result = TypeHelper.GetType(typeName);
			}

			return result;
		}

		private ServiceKeyStore ServiceKeyStore { get; set; } = new ServiceKeyStore();
	}
}
