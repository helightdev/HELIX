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
      ComponentRegistrations registrations,
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
    public static ComponentRegistration Add<T>(
      this ComponentRegistrations registrations,
      Func<ComponentLoadContext, T> activator
    ) where T : class {
      ComponentRegistration result = null;
      registrations.Register(typeof(T), entry => {
        result = entry;
        entry.activator = context => activator(context);
      });
      return result;
    }

    public static ComponentRegistration In<TScope>(this ComponentRegistration registration)
      where TScope : IScope {
      return registration.InScope(typeof(TScope));
    }

    public static ComponentRegistration Exposes<T>(
      this ComponentRegistration registration,
      string qualifier = null
    ) {
      return registration.Key(new TypeKey(typeof(T), qualifier));
    }

    public static ComponentRegistration Requires<T>(
      this ComponentRegistration registration,
      string qualifier = null
    ) {
      return registration.Dependency(new ComponentDependency(new TypeKey(typeof(T), qualifier), true));
    }

    public static ComponentRegistration OptionallyRequires<T>(
      this ComponentRegistration registration,
      string qualifier = null
    ) {
      return registration.Dependency(new ComponentDependency(new TypeKey(typeof(T), qualifier), false));
    }

    public static ComponentRegistration Collects<T>(
      this ComponentRegistration registration,
      string qualifier = null
    ) {
      return registration.OptionallyRequires<T>(qualifier);
    }

    public static ComponentRegistration Publishes<T>(
      this ComponentRegistration registration,
      string qualifier = null,
      bool required = true
    ) {
      return registration.Publication(
        new ComponentDependency(new TypeKey(typeof(T), qualifier), required)
      );
    }

    public static T Resolve<T>(this ComponentLoadContext context, string qualifier = null) where T : class {
      return (T)context.Resolve(new TypeKey(typeof(T), qualifier));
    }

    public static T Resolve<T>(this ManagedScope scope, string qualifier = null) where T : class {
      return (T)scope.Resolve(new TypeKey(typeof(T), qualifier));
    }

    public static T ResolveOptional<T>(this ComponentLoadContext context, string qualifier = null) where T : class {
      return context.TryResolve(new TypeKey(typeof(T), qualifier), out var value) ? value as T : null;
    }

    public static IReadOnlyList<T> ResolveAll<T>(
      this ComponentLoadContext context,
      string qualifier = null
    ) where T : class {
      return context.scope.ResolveAll(new TypeKey(typeof(T), qualifier)).Cast<T>().ToList();
    }
  }
}
