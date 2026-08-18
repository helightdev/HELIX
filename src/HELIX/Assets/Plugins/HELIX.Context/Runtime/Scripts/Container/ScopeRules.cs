using System;
using System.Collections.Generic;
using System.Linq;

namespace HELIX.Context {
  /// <summary>Context supplied to policies that validate a new scope relationship.</summary>
  public readonly struct ScopeValidationContext {
    public readonly ManagedScope parent;
    public readonly IScope child;
    public readonly ComponentRegistrations registrations;
    public readonly IEnumerable<IScope> managedScopes;
    public readonly ApplicationScope application;

    internal ScopeValidationContext(
      ManagedScope parent,
      IScope child,
      ComponentRegistrations registrations,
      IEnumerable<IScope> managedScopes,
      ApplicationScope application
    ) {
      this.parent = parent;
      this.child = child;
      this.registrations = registrations;
      this.managedScopes = managedScopes;
      this.application = application;
    }

    public static ScopeValidationContext Create(ManagedContainer container, ManagedScope parent, IScope child) =>
      new(parent, child, container.registrarScope.registrations, container.scopes.Keys, container.applicationScope);
  }

  /// <summary>Extends container scope validation without coupling custom scope types to HXContainer.</summary>
  public interface IScopeRule {
    void Validate(ScopeValidationContext context);
  }

  /// <summary>Composable validation policy used when scopes are created.</summary>
  public sealed class ScopeRules {
    private readonly IScopeRule[] _rules;

    public static ScopeRules Default { get; } = new(
      new BuiltInScopeRule(),
      new RegistrationScopeRule()
    );

    public ScopeRules(params IScopeRule[] rules) {
      _rules = rules?.Where(static rule => rule != null).ToArray() ?? Array.Empty<IScopeRule>();
    }

    public void Validate(ScopeValidationContext context) {
      foreach (var rule in _rules) rule.Validate(context);
    }
  }

  /// <summary>Applies parent and multiplicity constraints declared by RegisterScope.</summary>
  public sealed class RegistrationScopeRule : IScopeRule {
    public void Validate(ScopeValidationContext context) {
      if (!context.registrations.scopes.TryGetValue(context.child.GetType(), out var registration)) return;
      if (!registration.AllowsParent(context.parent.scope.GetType())) {
        throw new ScopeLifecycleException(
          $"Scope {context.child.GetType().FullName} cannot be created below {context.parent.scope.GetType().FullName}."
        );
      }
      if (!registration.allowMultiple &&
        context.managedScopes.Any(scope => scope.GetType() == context.child.GetType())) {
        throw new ScopeLifecycleException(
          $"Scope {context.child.GetType().FullName} does not allow multiple instances."
        );
      }
    }
  }

  /// <summary>Default policy for the built-in Unity-aware scopes. Custom ScopeRules may replace it entirely.</summary>
  public sealed class BuiltInScopeRule : IScopeRule {
    public void Validate(ScopeValidationContext context) {
      var parent = context.parent.scope;
      switch (context.child) {
        case RegistrarScope:
          throw new ScopeLifecycleException("A registrar scope cannot be created as a child.");
        case ApplicationScope application
          when !ReferenceEquals(application, context.application) || parent is not RegistrarScope:
          throw new ScopeLifecycleException("The container application scope must be a direct child of the registrar.");
        case SessionScope when parent is not ApplicationScope:
          throw new ScopeLifecycleException("A session scope must be a child of the application scope.");
        case SceneScope sceneScope:
          if (parent is not ApplicationScope && parent is not SessionScope)
            throw new ScopeLifecycleException("A scene scope must be a child of an application or session scope.");
          if (!sceneScope.scene.IsValid() || !sceneScope.scene.isLoaded)
            throw new ScopeLifecycleException("A scene scope requires a valid, loaded scene.");
          break;
        case GameObjectScope gameObjectScope:
          ValidateGameObjectScope(context.parent, gameObjectScope);
          break;
      }
    }

    private static void ValidateGameObjectScope(ManagedScope parent, GameObjectScope child) {
      if (child.gameObject == null) throw new ScopeLifecycleException("A GameObject scope requires a GameObject.");
      if (parent.scope is not ApplicationScope && parent.scope is not SessionScope && parent.scope is not SceneScope) {
        throw new ScopeLifecycleException(
          "A GameObject scope must be a child of an application, session or scene scope."
        );
      }
      if (parent.scope is SceneScope scene && child.gameObject.scene != scene.scene)
        throw new ScopeLifecycleException("A GameObject scope beneath a scene scope must belong to that scene.");
    }
  }
}
