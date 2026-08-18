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

  [AttributeUsage(AttributeTargets.Class)]
  [MixinDefineTarget(MixinOn.ConfigureComponent, MixinOn.RegistrationConfiguratorDelegate)]
  [MixinDefineTarget(MixinOn.ComponentLoad, MixinOn.ComponentLoadDelegate)]
  [MixinDefineTarget(MixinOn.ComponentLoadLate, MixinOn.ComponentLoadLateDelegate)]
  [MixinDefineTarget(MixinOn.Init, MixinOn.ComponentLoadDelegate)]
  [MixinDefineTarget(MixinOn.Dispose, MixinOn.ComponentUnload)]
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
