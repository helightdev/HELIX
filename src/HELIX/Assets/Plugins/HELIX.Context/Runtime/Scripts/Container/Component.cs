using System;

namespace HELIX.Context {
  public interface IComponent {
    ManagedScope Scope { get; set; }
    void LoadComponent() { }
    void UnloadComponent() { }
  }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  [MixinDefineTarget(MixinOn.ConfigureComponent, MixinOn.RegistrationConfiguratorDelegate)]
  [MixinExpression(
    new[] { MixinOn.ConfigureComponent, MixinOn.ComponentLoad, MixinOn.ComponentUnload },
    new[] { -100_000, 0, 0 },
    @"
@USING UnityEngine;
@USING HELIX.Context;
@CODE<$ConfigureComponent> registration.name = ""@this:name"";
@CODE<IMPLEMENTS> IComponent
@CODE<CLASS> public ManagedScope Scope { get; set; }
@VAR<IsComponent> true
@SCOPE
  @MATCH @this:?is<MonoBehaviour>
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.MonoBehaviour<@this:type>();
  @RETURN
@SCOPE
  @CODE<$ConfigureComponent> registration.activator = DefaultComponentActivators.PlainObject<@this:type>();
  @RETURN
"
  )]
  public class ComponentAttribute : Attribute { }

  public class ServiceAttribute : ComponentAttribute { }
}