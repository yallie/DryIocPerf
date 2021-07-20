using System;
using System.Reflection;
using DryIoc;
using DryIoc.MefAttributedModel;
using Ultima;

namespace DryIocPerf
{
	public class ContainerAdapter : IMyContainer, IMyResolverContext
	{
		public ContainerAdapter(IResolverContext dryIoc) => Container = dryIoc;

		private IResolverContext Container { get; set; }

		public void Dispose() => Container.Dispose();

		public void InjectPropertiesAndFields(object instance) =>
			Container.InjectPropertiesAndFields(instance);

		public IMyResolverContext OpenScope() =>
			new ContainerAdapter(Container.OpenScope());

		public void RegisterExports(params Assembly[] assemblies)
		{
			if (Container is IContainer container)
			{
				container.RegisterExports(assemblies);
				return;
			}

			throw new InvalidOperationException();
		}

		public void AddDynamicRegistrations(Rules.DynamicRegistrationProvider getDynamicRegistrations)
		{
			if (Container is IContainer container)
			{
				Container = container.With(r => r.WithDynamicRegistrations(getDynamicRegistrations));
				return;
			}

			throw new InvalidOperationException();
		}
	}
}
