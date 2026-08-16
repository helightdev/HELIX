using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HELIX.Context {
  public interface IScope { }

  public class RegistrarScope : IScope {
    public ComponentRegistrations registrations;
  }

  public class ApplicationScope : IScope { }

  public class SessionScope : IScope { }

  public class SceneScope : IScope {
    public Scene scene;
  }

  public class GameObjectScope : IScope {
    public GameObject gameObject;
  }

  public class ScopeRegistration {
    public Type type;
    public Type parentType;
    public readonly List<Type> parentTypes = new();
    public bool allowMultiple = true;
    public ScopeRegistration() { }

    public ScopeRegistration(Type type, params Type[] allowedParentTypes) {
      this.type = type;
      if (allowedParentTypes == null) return;
      parentTypes.AddRange(allowedParentTypes.Where(static value => value != null));
      parentType = parentTypes.FirstOrDefault();
    }

    public bool AllowsParent(Type candidate) {
      return parentTypes.Count > 0
        ? parentTypes.Contains(candidate)
        : parentType == null || parentType == candidate;
    }
  }

  public enum ManagedScopeState { Created, Initializing, Active, Disposing, Disposed, Faulted }

  public sealed partial class ManagedScope {
    private readonly Dictionary<TypeKey, List<Binding>> _bindings = new();
    private readonly CancellationTokenSource _cancellation = new();

    public readonly IScope scope;
    public ManagedScope parent;
    public readonly List<IScope> children = new();
    public ManagedScopeState State { get; internal set; } = ManagedScopeState.Created;

    public IReadOnlyList<ManagedScope> ManagedChildren => _managedChildren;
    public CancellationToken CancellationToken => _cancellation.Token;
    public bool IsActive => State == ManagedScopeState.Active;

    public ManagedScope(IScope scope) {
      this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    public ManagedScope(ManagedScope parent, IScope scope) : this(scope) {
      this.parent = parent;
    }

    public object Resolve(TypeKey key) {
      EnsureResolvable();
      if (TryResolveValue(key, out var value)) return value;
      throw new ComponentResolutionException(
        $"No component is registered for '{key}' in scope {scope.GetType().FullName} or its ancestors."
      );
    }

    public bool TryResolve(TypeKey key, out object value) {
      EnsureResolvable();
      return TryResolveValue(key, out value);
    }

    public IReadOnlyList<object> ResolveAll(TypeKey key) {
      EnsureResolvable();
      ValidateKey(key);
      var values = new List<object>();
      for (var current = this; current != null; current = current.parent) {
        if (current._bindings.TryGetValue(key, out var bindings))
          values.AddRange(bindings.Select(static x => x.value));
      }
      return values;
    }

    internal void BindComponent(RegistrationEntry registration, object instance) {
      foreach (var key in registration.keys.Distinct()) AddBinding(registration, key, instance);
    }

    internal void Publish(RegistrationEntry owner, TypeKey key, object value) {
      AddBinding(owner, key, value);
    }

    internal bool HasDependency(ComponentDependency dependency) {
      if (!dependency.flags.HasFlag(DependencyFlags.Required)) return true;
      return IsDependencyAvailable(dependency);
    }

    internal bool IsDependencyAvailable(ComponentDependency dependency) {
      if (dependency.IsTyped) return TryResolveValue(dependency.key, out _);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable))
        return dependency.scripted != null && ScopeLoader.Active.Contains(dependency.scripted);
      return HasWireKey(dependency.wireKey);
    }

    internal bool HasWireKey(string wireKey) {
      if (string.IsNullOrEmpty(wireKey)) return false;
      for (var current = this; current != null; current = current.parent) {
        if (ScopeLoader.ActiveOrNull?.HasPublication(wireKey) == true)
          return true;
      }
      return false;
    }

    internal bool HasLocalDependency(ComponentDependency dependency) {
      if (dependency.IsTyped) return TryResolveLocalValue(dependency.key, out _);
      if (!dependency.flags.HasFlag(DependencyFlags.Wirable))
        return dependency.scripted != null && ScopeLoader.Active.Contains(dependency.scripted);
      return HasLocalWireKey(dependency.wireKey);
    }

    internal bool WasProvidedBy(RegistrationEntry registration, ComponentDependency dependency) {
      if (dependency.IsTyped) {
        return _bindings.TryGetValue(dependency.key, out var bindings) &&
          bindings.Any(x => ReferenceEquals(x.owner, registration));
      }
      return dependency.wireKey != null && ScopeLoader.Active.WasPublishedBy(registration, dependency.wireKey);
    }

    internal void MarkScriptedLoaded(IScriptedDependency dependency) {
      ScopeLoader.Active.Record(dependency);
      if (dependency.Flags.HasFlag(DependencyFlags.Wirable)) ScopeLoader.Active.Publish(dependency.CreateWireKey());
    }

    internal bool IsScriptedLoaded(IScriptedDependency dependency) {
      return ScopeLoader.Active.Contains(dependency);
    }

    internal IEnumerable<LoadedComponent> LoadedComponents => _loadedComponents;

    internal IEnumerable<TypeKey> BoundKeys(RegistrationEntry registration) {
      return _bindings
        .Where(pair => pair.Value.Any(binding => ReferenceEquals(binding.owner, registration)))
        .Select(static pair => pair.Key);
    }

    internal IEnumerable<string> PublishedWireKeys(RegistrationEntry registration) {
      return ScopeLoader.ActiveOrNull?.PublicationsBy(registration) ?? Array.Empty<string>();
    }

    private void AddBinding(RegistrationEntry owner, TypeKey key, object value) {
      EnsureCanPublish();
      ValidateKey(key);
      if (value == null) throw new ComponentResolutionException($"Cannot publish null for '{key}'.");
      if (!key.type.IsInstanceOfType(value)) {
        throw new ComponentResolutionException(
          $"Value of type {value.GetType().FullName} cannot be published as '{key}'."
        );
      }
      if (!_bindings.TryGetValue(key, out var bindings)) _bindings.Add(key, bindings = new List<Binding>());
      if (!bindings.Any(binding => ReferenceEquals(binding.owner, owner) && ReferenceEquals(binding.value, value)))
        bindings.Add(new Binding(owner, value));
      ScopeLoader.Active.Publish(owner, key.CreateWireKey());
    }

    private bool TryResolveValue(TypeKey key, out object value) {
      ValidateKey(key);
      for (var current = this; current != null; current = current.parent) {
        if (current.TryResolveLocalValue(key, out value))
          return true;
      }
      value = null;
      return false;
    }

    private bool TryResolveLocalValue(TypeKey key, out object value) {
      ValidateKey(key);
      if (_bindings.TryGetValue(key, out var bindings) && bindings.Count > 0) {
        value = bindings[^1].value;
        return true;
      }
      value = null;
      return false;
    }

    private bool HasLocalWireKey(string wireKey) {
      if (string.IsNullOrEmpty(wireKey)) return false;
      return ScopeLoader.ActiveOrNull?.HasPublication(wireKey) == true;
    }

    private static void ValidateKey(TypeKey key) {
      if (key.type == null) throw new ArgumentException("A component key must have a type.", nameof(key));
    }

    private void EnsureResolvable() {
      if (State is ManagedScopeState.Initializing or ManagedScopeState.Active) return;
      throw new ScopeLifecycleException(
        $"Scope {scope.GetType().FullName} cannot resolve components while it is {State}."
      );
    }

    private void EnsureCanPublish() {
      if (State is ManagedScopeState.Initializing or ManagedScopeState.Active) return;
      throw new ScopeLifecycleException(
        $"Scope {scope.GetType().FullName} cannot accept publications while it is {State}."
      );
    }

    private readonly List<ManagedScope> _managedChildren = new();
    private readonly List<LoadedComponent> _loadedComponents = new();
    private readonly HashSet<object> _owned = new(ReferenceComparer<object>.Instance);

    internal void AddChild(ManagedScope child) {
      _managedChildren.Add(child);
      children.Add(child.scope);
    }

    internal void RemoveChild(ManagedScope child) {
      _managedChildren.Remove(child);
      var index = children.FindIndex(x => ReferenceEquals(x, child.scope));
      if (index >= 0) children.RemoveAt(index);
    }

    internal void RecordComponent(RegistrationEntry registration, object instance) {
      _loadedComponents.Add(new LoadedComponent(registration, instance));
    }

    internal void Own(object value) {
      EnsureCanPublish();
      switch (value) {
        case null: return;
        case UnityEngine.Object or IDisposable:
          _owned.Add(value);
          return;
        default:
          throw new ArgumentException(
            $"Scope-owned values must be a {nameof(UnityEngine.Object)} or {nameof(IDisposable)}.",
            nameof(value)
          );
      }
    }

    internal void BeginInitialization() {
      if (State != ManagedScopeState.Created)
        throw new ScopeLifecycleException($"Scope {scope.GetType().FullName} cannot initialize while it is {State}.");
      State = ManagedScopeState.Initializing;
    }

    internal void Activate(ManagedContainer container = null) {
      if (container != null && scope is GameObjectScope gameObjectScope && gameObjectScope.gameObject != null) {
        var observer = gameObjectScope.gameObject.AddComponent<GameObjectScopeObserver>();
        observer.Attach(container, scope);
      }
      parent?.AddChild(this);
      State = ManagedScopeState.Active;
    }

    internal List<Exception> Rollback() {
      State = ManagedScopeState.Faulted;
      var failures = new List<Exception>();
      Teardown(failures);
      return failures;
    }

    internal List<Exception> Dispose(Action<ManagedScope> released = null) {
      var failures = new List<Exception>();
      Dispose(failures, released);
      return failures;
    }

    private void Dispose(List<Exception> failures, Action<ManagedScope> released) {
      if (State is ManagedScopeState.Disposed or ManagedScopeState.Disposing) return;
      State = ManagedScopeState.Disposing;
      Cancel(failures);
      foreach (var child in _managedChildren.ToArray()) child.Dispose(failures, released);
      Teardown(failures, false);
      State = ManagedScopeState.Disposed;
      released?.Invoke(this);
    }

    private void Teardown(List<Exception> failures, bool cancel = true) {
      if (cancel) Cancel(failures);
      DetachUnityLifetime(failures);
      UnloadComponents(failures);
      DisposeOwnedResources(failures);
      DestroyOwnedUnityObjects(failures);
      parent?.RemoveChild(this);
    }

    private void DetachUnityLifetime(List<Exception> failures) {
      if (scope is not GameObjectScope gameObjectScope || gameObjectScope.gameObject == null) return;
      try {
        foreach (var observer in gameObjectScope.gameObject.GetComponents<GameObjectScopeObserver>()) {
          if (!observer.Observes(scope)) continue;
          observer.Detach();
          if (Application.isPlaying) UnityEngine.Object.Destroy(observer);
          else UnityEngine.Object.DestroyImmediate(observer);
        }
      } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Failed to detach a Unity scope lifetime.", exception));
      }
    }

    private void Cancel(List<Exception> failures) {
      if (_cancellation.IsCancellationRequested) return;
      try { _cancellation.Cancel(); } catch (Exception exception) {
        failures.Add(new ComponentDeinitializationException("Scope cancellation failed.", exception));
      }
    }

    private void UnloadComponents(List<Exception> failures) {
      if (_loadedComponents == null) return;
      foreach (var loaded in _loadedComponents.AsEnumerable().Reverse()) Unload(loaded, failures);
      _bindings.Clear();
      _loadedComponents.Clear();
    }

    private void DisposeOwnedResources(List<Exception> failures) {
      foreach (var resource in _owned.OfType<IDisposable>()) {
        try { resource.Dispose(); } catch (Exception exception) {
          failures.Add(new ComponentDeinitializationException("Failed to dispose a scope-owned resource.", exception));
        }
      }
      _owned.RemoveWhere(value => value is IDisposable);
    }

    private void DestroyOwnedUnityObjects(List<Exception> failures) {
      foreach (var owned in _owned.OfType<UnityEngine.Object>()) {
        if (owned == null) continue;
        try {
          if (Application.isPlaying) UnityEngine.Object.Destroy(owned);
          else UnityEngine.Object.DestroyImmediate(owned);
        } catch (Exception exception) {
          failures.Add(
            new ComponentDeinitializationException(
              $"Failed to destroy Unity object '{owned.name}' owned by {scope.GetType().Name}.",
              exception
            )
          );
        }
      }
      _owned.RemoveWhere(value => value is UnityEngine.Object);
      _cancellation.Dispose();
    }

    private static void Unload(LoadedComponent loaded, List<Exception> failures) {
      try {
        if (loaded.instance is IComponent component) component.UnloadComponent();
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException($"Failed to unload component '{loaded.registration.name}'.", exception)
        );
      }
      try {
        if (loaded.instance is IEventListener listener) listener.HandlerList.UnregisterAll();
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException(
            $"Failed to unregister handlers for component '{loaded.registration.name}'.",
            exception
          )
        );
      }
      try {
        if (loaded.instance is IDisposable disposable) disposable.Dispose();
      } catch (Exception exception) {
        failures.Add(
          new ComponentDeinitializationException(
            $"Failed to dispose component '{loaded.registration.name}'.",
            exception
          )
        );
      }
    }

    private readonly struct Binding {
      public readonly RegistrationEntry owner;
      public readonly object value;

      public Binding(RegistrationEntry owner, object value) {
        this.owner = owner;
        this.value = value;
      }
    }

    internal readonly struct LoadedComponent {
      public readonly RegistrationEntry registration;
      public readonly object instance;

      public LoadedComponent(RegistrationEntry registration, object instance) {
        this.registration = registration;
        this.instance = instance;
      }
    }
  }
}