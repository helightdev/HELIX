using System;

namespace HELIX.Context {
  public interface IComponent {
    RuntimeComponentData RuntimeComponentData { get; }
    void LoadComponent() { }
    void UnloadComponent() { }
  }

  public class RuntimeComponentData {
    public ManagedScope scope;
    public bool isLoaded;
    public bool isDisposed;

    public bool IsActive => isLoaded && !isDisposed && scope is { IsActive: true };

    public T ResolveKey<T>(TypeKey key) where T : class {
      if (scope == null) throw new ComponentStateException("Component is not yet attached to a scope");
      return scope.Resolve(key) as T;
    }

    public T Resolve<T>(string qualifier = null) where T : class => ResolveKey<T>(new TypeKey(typeof(T), qualifier));
    public T Resolve<T>(Type type, string qualifier = null) where T : class => ResolveKey<T>(new TypeKey(type, qualifier));
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
@CODE<CLASS> public RuntimeComponentData RuntimeComponentData { get; } = new();
@VAR<IsComponent> true
"
  )]
  public class ComponentAttribute : Attribute { }


  [MixinExpression(
    new[] { MixinOn.ConfigureComponent },
    new[] { -90_000 },
    @"
@SCOPE
  @MATCH @this:?is<MonoBehaviour>
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.MonoBehaviour<@this:type>();
  @RETURN
@SCOPE
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.PlainObject<@this:type>();
  @RETURN
"
  )]
  public class ServiceAttribute : ComponentAttribute { }
}