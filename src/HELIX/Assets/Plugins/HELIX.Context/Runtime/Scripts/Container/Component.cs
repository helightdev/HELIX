using System;

namespace HELIX.Context {
  public interface IComponent {
    RuntimeComponentData ComponentBinding { get; }
    void LoadComponent() { }
    void UnloadComponent() { }
  }

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

    // public IReadOnlyList<T> ResolveAll<T>(TypeKey key) where T : class => scope.ResolveAll(key);

    public ManagedScopeBuilder CreateScope() => container.CreateScope(scope);
  }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  [MixinDefineTarget(MixinOn.ConfigureComponent, MixinOn.RegistrationConfiguratorDelegate)]
  [MixinExpression(
    new[] { MixinOn.ConfigureComponent },
    new[] { -100_000 },
    @"
@USING UnityEngine;
@USING HELIX.Context;
@CODE<$ConfigureComponent> registration.name = ""@this:name"";
@CODE<IMPLEMENTS> IComponent
@CODE<CLASS> public RuntimeComponentData ComponentBinding { get; } = new();
@VAR<IsComponent> true
"
  )]
  public class ComponentAttribute : Attribute { }


  [MixinExpression(
    new[] { MixinOn.ConfigureComponent },
    new[] { -90_000 },
    @"
@CODE<$ConfigureComponent> registration.scope = @attr#scope;

@SCOPE
  @MATCH @this:?is<MonoBehaviour>
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.MonoBehaviour<@this:type>();
  @RETURN
@SCOPE
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.PlainObject<@this:type>();
  @RETURN
"
  )]
  public class ServiceAttribute : ComponentAttribute {
    public readonly Type scope;

    public ServiceAttribute(Type scope = null) {
      this.scope = scope;
    }
  }
}