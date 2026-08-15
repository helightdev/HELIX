using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {

  public interface IScope {}

  public class RegistrarScope : IScope {
    public ComponentRegistrations registrations;
  }
  public class ApplicationScope : IScope {}
  public class SessionScope : IScope {}

  public class SceneScope : IScope {
    public Scene scene;
  }

  public class GameObjectScope : IScope {
    public GameObject gameObject;
  }

  public class ScopeRegistration {
    public Type type;
    public Type parentType;
  }

  public class ManagedScope {
    public readonly IScope scope;
    public ManagedScope parent;
    public readonly List<IScope> children;

    public ManagedScope(IScope scope) {
      this.scope = scope;
      this.children = new List<IScope>();
    }

    public ManagedScope(ManagedScope parent, IScope scope) {
      this.scope = scope;
      this.parent = parent;
      children = new List<IScope>();
    }
  }

  public sealed class HXContainer {
    public readonly Dictionary<IScope, ManagedScope> scopes = new();
    public readonly RegistrarScope registrarScope = new();
    public readonly ApplicationScope applicationScope = new();

    public void PrepareRegistrar(ComponentRegistrations registrations) {
      // Re-initialize the registrar scope with the given data
      registrarScope.registrations = registrations;

      var registrar = new ManagedScope(registrarScope);
      scopes[registrarScope] = registrar;
    }

    public async UniTask StartApplication() {
      await CreateScope(scopes[registrarScope], applicationScope);
    }

    public async UniTask<ManagedScope> CreateScope(ManagedScope parent, IScope scope) {
      var managed = new ManagedScope(scope);
      scopes[scope] = managed;
      parent.children.Add(scope);

      return managed;
    }
  }

  public static class HX {
    public static HXContainer container;



  }

  public interface IComponent { }

  // Stereotype attributes
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  [MixinExpression(
    new[] { MixinOn.ConfigureRegistration },
    new[] { -100_000 },
    @"
@CODE<^*~HELIX.Context.RegistrationConfigurator> registration.name = ""@this:name"";
@CODE<IMPLEMENTS> global::HELIX.Context.IComponent
@VAR<IsComponent> true
"
  )]
  public class ComponentAttribute : Attribute { }

  public class ServiceAttribute : ComponentAttribute { }
}