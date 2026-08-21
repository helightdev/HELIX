using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace HELIX.Context.Tests.Fixtures {
  public abstract class ManagedContainerTestFixture {
    private readonly List<ManagedContainer> _containers = new();

    [TearDown]
    public void DisposeContainers() {
      foreach (var container in _containers) {
        try { container.Dispose(); } catch (AggregateException) { }
      }
      _containers.Clear();
    }

    protected ManagedContainer CreateContainer(
      ManagedRegistrations registrations,
      bool assignUnscopedToApplication = true,
      int? maxLoadingIterations = null,
      params IScopeHandler[] handlers
    ) {
      if (assignUnscopedToApplication) {
        foreach (var registration in registrations.components.Values) {
          if (registration.scope == null) registration.scope = typeof(ApplicationScope);
        }
      }

      var builder = new ManagedContainerBuilder();
      if (maxLoadingIterations.HasValue) builder.WithMaxLoadingIterations(maxLoadingIterations.Value);
      foreach (var handler in handlers ?? Array.Empty<IScopeHandler>()) builder.AddScopeHandler(handler);

      var container = Track(builder.Build());
      container.PrepareRegistrar(registrations);
      return container;
    }

    protected ManagedContainer Track(ManagedContainer container) {
      _containers.Add(container);
      return container;
    }
  }

  public static class RegistrationFixtures {
    public static ManagedRegistration Add<T>(
      this ManagedRegistrations registrations,
      Func<ManagedLoadContext, T> activator
    ) where T : class {
      ManagedRegistration result = null;
      registrations.Register(typeof(T), entry => {
        result = entry;
        entry.activator = context => activator(context);
      });
      return result;
    }

    public static ManagedRegistration In<TScope>(this ManagedRegistration registration)
      where TScope : IScope {
      return registration.InScope(typeof(TScope));
    }

    public static ManagedRegistration Exposes<T>(
      this ManagedRegistration registration,
      string qualifier = null
    ) {
      return registration.Key(new TypeKey(typeof(T), qualifier));
    }

    public static ManagedRegistration Requires<T>(
      this ManagedRegistration registration,
      string qualifier = null
    ) {
      return registration.Dependency(new ComponentDependency(new TypeKey(typeof(T), qualifier), true));
    }

    public static ManagedRegistration OptionallyRequires<T>(
      this ManagedRegistration registration,
      string qualifier = null
    ) {
      return registration.Dependency(new ComponentDependency(new TypeKey(typeof(T), qualifier), false));
    }

    public static ManagedRegistration Collects<T>(
      this ManagedRegistration registration,
      string qualifier = null
    ) {
      return registration.Dependency(
        new ComponentDependency(new TypeKey(typeof(T), qualifier), false, true)
      );
    }

    public static ManagedRegistration Publishes<T>(
      this ManagedRegistration registration,
      string qualifier = null,
      bool required = true
    ) {
      return registration.Publication(
        new ComponentDependency(new TypeKey(typeof(T), qualifier), required)
      );
    }

    public static T Resolve<T>(this ManagedLoadContext context, string qualifier = null) where T : class {
      return (T)context.Resolve(new TypeKey(typeof(T), qualifier));
    }

    public static T Resolve<T>(this ManagedScope scope, string qualifier = null) where T : class {
      return (T)scope.Resolve(new TypeKey(typeof(T), qualifier));
    }

    public static T ResolveOptional<T>(this ManagedLoadContext context, string qualifier = null) where T : class {
      return context.TryResolve(new TypeKey(typeof(T), qualifier), out var value) ? value as T : null;
    }

    public static IReadOnlyList<T> ResolveAll<T>(
      this ManagedLoadContext context,
      string qualifier = null
    ) where T : class {
      return context.scope.ResolveAll(new TypeKey(typeof(T), qualifier)).Cast<T>().ToList();
    }
  }
}
