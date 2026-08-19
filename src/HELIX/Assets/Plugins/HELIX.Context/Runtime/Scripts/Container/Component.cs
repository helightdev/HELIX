using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace HELIX.Context {
  public interface IComponent {
    RuntimeComponentData ComponentBinding { get; }
    void LoadComponent(ComponentLoadContext context) {}
    void LoadComponentLate(ComponentLoadContext context) { }
    void UnloadComponent() { }
  }

  [UsedImplicitly]
  public delegate void ComponentLoadMethod(ComponentLoadContext context);

  [UsedImplicitly]
  public delegate void ComponentUnloadMethod();

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed class RuntimeComponentData {
    public ManagedScope scope;
    public ManagedContainer container;
    public bool isLoaded;
    public bool isDisposed;

    public void SetLoaded(bool loaded) {
      isLoaded = loaded;
    }

    public void SetDisposed(bool disposed) {
      isDisposed = disposed;
      if (disposed) {
        isLoaded = false;
      }
    }

    public bool IsActive =>
      isLoaded && !isDisposed && scope is { IsActive: true }; // TODO: Notify from scope to update state

    public T ResolveKey<T>(TypeKey key) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.Resolve(key) as T;
    }

    public T Resolve<T>(string qualifier = null) where T : class => ResolveKey<T>(new TypeKey(typeof(T), qualifier));

    public T Resolve<T>(Type type, string qualifier = null) where T : class =>
      ResolveKey<T>(new TypeKey(type, qualifier));

    public T ResolveOptional<T>(string qualifier = null) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.TryResolve(new TypeKey(typeof(T), qualifier), out var value) ? value as T : null;
    }

    public List<T> ResolveAll<T>(string qualifier = null) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.ResolveAll(new TypeKey(typeof(T), qualifier)).Cast<T>().ToList();
    }

    public ManagedScopeBuilder CreateScope() => container.CreateScope(scope);
  }

  public static class ComponentTargets {
    public const string RegistrationConfiguratorDelegate = "^*~HELIX.Context.RegistrationConfigurator";
    public const string ComponentLoadDelegate = "^LoadComponent:HELIX.Context.ComponentLoadMethod";
    public const string ComponentLoadLateDelegate = "^LoadComponentLate:HELIX.Context.ComponentLoadMethod";
    public const string ComponentUnloadDelegate = "^UnloadComponent:HELIX.Context.ComponentUnloadMethod";
  }

  [AttributeUsage(AttributeTargets.Class)]
  [MixinDefineTarget(MixinOn.ConfigureComponent, ComponentTargets.RegistrationConfiguratorDelegate)]
  [MixinDefineTarget(MixinOn.LoadComponent, ComponentTargets.ComponentLoadDelegate)]
  [MixinDefineTarget(MixinOn.LoadComponentLate, ComponentTargets.ComponentLoadLateDelegate)]
  [MixinDefineTarget(MixinOn.UnloadComponent, ComponentTargets.ComponentUnloadDelegate)]
  [MixinDefineTarget(MixinOn.Init, ComponentTargets.ComponentLoadDelegate)]
  [MixinDefineTarget(MixinOn.Dispose, MixinOn.UnloadComponent)]
  [MixinExpression(new[] { MixinOn.ConfigureComponent }, new[] { -100_000 }, "@CALL<ComponentImpl>")]
  public class ComponentAttribute : Attribute {
    public ComponentAttribute(
      Type scope = null,
      bool optional = false,
      int order = 0,
      int phase = InitPhase.Normal
    ) { }
  }
}
